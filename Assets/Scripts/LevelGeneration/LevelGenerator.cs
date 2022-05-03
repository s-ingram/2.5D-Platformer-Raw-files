using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utilities;

public class LevelGenerator : MonoBehaviour
{
	public static GameObject level;

	[SerializeField]
	private bool usePrefabsInOrder = false, useStartingRoomPrefabs = false, useBossRooms = false, usePrefabsOnlyOnce = false;
	private bool finalRoomAdded = false;

	[SerializeField]
	private int roomCount, roomMinimum = 1, roomMaximum = 10, npcsPerSpawner = 1, sortingOrderBase = 5000, offset = -10;
	private int prefabItr = 0;

	Material wallMaterial, floorMaterial;
	float lightIntensity;

	GameObject topCap, bottomCap, leftCap, rightCap;
	List<GameObject> startingRoomPrefabs;
	List<GameObject> roomPrefabs;
	List<GameObject> roomPrefabsOriginal;
	List<GameObject> bossRoomPrefabs;
	List<Room> levelRoomList;

	static List<PfNode> pathfindingNodes;

	ScriptableItemList scriptableItemList;
	ScriptableNPCList scriptableNPCList;

	public event EventHandler OnLevelGenerated, OnPathsGenerated, OnMapGenerationComplete;

	void Awake()
	{
		/*
		 * Subscribe to various events.
		 */

		OnLevelGenerated += CorrectSortingLayers;
		OnLevelGenerated += ApplyMaterials;
		OnLevelGenerated += SetLightIntensity;
		OnLevelGenerated += CapRemainingDoors;
		OnLevelGenerated += GeneratePaths;
		OnLevelGenerated += GenerateLevelFloorMesh;
		OnPathsGenerated += SpawnItems;
		OnPathsGenerated += SpawnNPCs;
		OnPathsGenerated += FinishGeneratingMap;
	}

	public void SetGenerationParams(ScriptableScene.LevelParams levelParams)
	{
		/*
		 * Read in room params.
		 */

		roomCount = 0;
		roomMinimum = levelParams.roomGenParams.roomMinimum;
		roomMaximum = levelParams.roomGenParams.roomMaximum;

		scriptableItemList = levelParams.roomItemList;
		scriptableNPCList = levelParams.roomNPCList;

		usePrefabsInOrder = levelParams.roomGenParams.usePrefabsInOrder;
		useStartingRoomPrefabs = levelParams.roomGenParams.useStartingRoomPrefabs;
		useBossRooms = levelParams.roomGenParams.useBossRooms;
		usePrefabsOnlyOnce = levelParams.roomGenParams.usePrefabsOnlyOnce;

		startingRoomPrefabs = levelParams.roomGenParams.startingRoomPrefabs;
		roomPrefabsOriginal = levelParams.roomGenParams.roomPrefabs;
		roomPrefabs = new List<GameObject>(roomPrefabsOriginal);
		bossRoomPrefabs = levelParams.roomGenParams.bossRoomPrefabs;

		levelRoomList = new List<Room>();
		pathfindingNodes = new List<PfNode>();

		wallMaterial = levelParams.roomGenParams.wallMaterial;
		floorMaterial = levelParams.roomGenParams.floorMaterial;
		lightIntensity = levelParams.roomGenParams.lightIntensity;

		roomMaximum = usePrefabsOnlyOnce ? roomPrefabs.Count : levelParams.roomGenParams.roomMaximum;
		roomMinimum = Mathf.Clamp(roomMinimum, levelParams.roomGenParams.roomMinimum, roomMaximum);
		npcsPerSpawner = levelParams.roomGenParams.npcsPerSpawner;

		topCap = levelParams.roomGenParams.topCap;
		bottomCap = levelParams.roomGenParams.bottomCap;
		leftCap = levelParams.roomGenParams.leftCap;
		rightCap = levelParams.roomGenParams.rightCap;
	}

	public IEnumerator GenerateLevel(ScriptableScene.LevelParams levelParams)
	{
		/*
		 * This function is the start point of the level generation. It is called by the GameManager.
		 * This function creates an new gameObject called "Level" and uses a recursive function GenerateRoomRecursive() to add new rooms.
		 * If there are too few rooms, the level is scrapped and is made again.
		 */

		SetGenerationParams(levelParams);

		level = new GameObject("Level");

		do
		{
			finalRoomAdded = false;
			
			//level = new GameObject("Level");
			GameObject map = new GameObject("Map");
			map.transform.SetParent(level.transform);
			map.AddComponent<UnityEngine.Rendering.Universal.CompositeShadowCaster2D>();

			//	These will be used for pathfinding, eventually, maybe.
			map.AddComponent<MeshFilter>();
			map.AddComponent<MeshRenderer>();

			//	Generate the first room, then use that room as the starting point for the recursive function.
			Room room = null;
			yield return StartCoroutine(TryAddRoom(null, null, map.transform, (r) => { room = r; }));
			yield return StartCoroutine(GenerateRoomRecursive(room, map.transform));
			yield return new WaitForFixedUpdate();

			//	If you want a boss room on this level, make one now, other, all rooms have been added to the level.
			if (useBossRooms)
				yield return StartCoroutine(AddBossRoom(map.transform));
			else
				finalRoomAdded = true;

			//	Check if the level has enough rooms, or if the final room has not been added (probably due to a lack of available doors when generating).
			//	If there are too few rooms, scrap this level, and start again.
			if (roomCount <= roomMinimum || !finalRoomAdded)
			{
				Debug.Log("Invalid level; restarting.");

				roomCount = 0;
				roomPrefabs.Clear();
				roomPrefabs = new List<GameObject>(roomPrefabsOriginal);

				levelRoomList.Clear();

				Destroy(map);
			}

			//	Do this loop until there are a desired number of rooms.
		}
		while (roomCount <= roomMinimum);

		foreach (Transform room in level.transform.Find("Map").transform)
			room.GetComponent<Collider2D>().enabled = false;

		//	Trigger the event; OnLevelGenerated.
		if (OnLevelGenerated != null) OnLevelGenerated(this, EventArgs.Empty);
	}

	private IEnumerator GenerateRoomRecursive(Room room, Transform map)
	{
		/*
		 * This is the recursive generator function. It is called by GenerateLevel().
		 * It takes in a room, a position, and a transform that will be the rooms parent (usually "Level")
		 * Loop through each door in the current room, and try and spawn a new room at that doors position.
		 */

		//	Do the following for every door in the room that was passed into this Coroutine.
		foreach (Door door in room.doorList)
		{
			//	If something is already attached to this door, or it is not marked as needing something to attach to it, continue to the next door.
			if (door.isCapped || !door.needsCapping)
				continue;

			//	If the level is marked to only use the provided prefabs once AND there are not prefabs available, mark this door to be capped, and break out of this Coroutine.
			if (usePrefabsOnlyOnce && roomPrefabs.Count <= 0)
			{
				door.needsCapping = true;
				yield break;
			}

			bool roomAdded = false;
			int attempts = 0;

			do
			{
				//	Try and add a room at this door. Use a callback to get a reference to the room.
				yield return StartCoroutine(TryAddRoom(room, door, map, (r) => { room = r; }));

				//	Check if a room was successfully created.
				if (room != null)
					roomAdded = true;
				else
					attempts++;

				//	If we've been unsuccessful too many times, mark the door as needing capping, and break out of this loop.
				if (attempts > 10)
				{
					door.needsCapping = true;
					break;
				}

				//	Do this loop until there is a room added.
			}
			while (!roomAdded);

			//	If we're not at the required number of rooms, start again for the room that was just created.
			if (roomCount <= roomMaximum)
				yield return StartCoroutine(GenerateRoomRecursive(room, map));
		}
	}

	private IEnumerator TryAddRoom(Room room, Door door, Transform parent, Action<Room> callback)
	{
		/*
		 * This function is for adding a new room to the level.
		 * When the prefab is instantiated, attach a Room component.
		 * Add the doors to the rooms list of doors.
		 */

		int i = usePrefabsInOrder ? prefabItr++ : UnityEngine.Random.Range(0, roomPrefabs.Count);
		i = usePrefabsOnlyOnce ? 0 : i;

		//	Do this if this is the first room being placed.
		if (roomCount == 0)
		{
			//	Check if we want to use specific starting rooms, if so, use them, or just use the normal list.
			//	Instantiate the desired room, then attach a Room component to it. 
			GameObject startingPrefabRoom = useStartingRoomPrefabs ? startingRoomPrefabs[UnityEngine.Random.Range(0, startingRoomPrefabs.Count)] : roomPrefabs[i];
			GameObject startingRoomObj = Instantiate(startingPrefabRoom, Vector3.zero, Quaternion.identity, parent);
			Room startingRoom = startingRoomObj.AddComponent<Room>();

			//	Find every door in this room, attach a Door component to it, then add it to the list of doors for this room.
			foreach (Transform T in startingRoom.transform.Find("Walls").Find("Doors"))
				startingRoom.AddDoor(T.gameObject.AddComponent<Door>());

			//	Increase the room count, set the callback, and break out of this Coroutine.
			levelRoomList.Add(startingRoom);
			if(room != null) room.ConnectRoom(startingRoom);
			roomCount++;
			callback(startingRoom);
			yield break;
		}

		//	Select a prefab, instantiate it at the door position, then attach a Room component to it.
		GameObject selectedPrefab = roomPrefabs[i];
		GameObject roomObj = Instantiate(selectedPrefab, door.transform.position, Quaternion.identity, parent);
		Room newRoom = roomObj.AddComponent<Room>();

		//	Find every door in this room, attach a Door component to it, then add it to the list of doors for this room.
		foreach (Transform T in newRoom.transform.Find("Walls").Find("Doors"))
			newRoom.AddDoor(T.gameObject.AddComponent<Door>());

		//	Check if the room can be aligned to the door. If it can't, destroy this room, set the callback to null, and break out of this Coroutine.
		if (!CheckAndAlignDoors(door, newRoom))
		{
			DestroyRoom(newRoom);
			callback(null);
			yield break;
		}

		//	Wait for one update. Physics checks, for things like colliders, won't work on the same frame as an object was instantiated.
		yield return new WaitForFixedUpdate();

		//	Now that the collider works, check if the room is colliding with any other.
		//	If it is NOT colliding with any other rooms, then this room is good. Add it to the list, increase the room count, cap the door and the new one it's connected to.
		//	If we're using the prefabs only one, remove it from the list. Now set the callback, and break out of the Coroutine.
		//	If the room does collide with other, then the room gets destroys, the callback set to null, and break out of the Coroutine.
		if (CheckIfRoomFits(newRoom))
		{
			levelRoomList.Add(newRoom);
			if (room != null) room.ConnectRoom(newRoom);
			roomCount++;

			newRoom.doorList.Find(x => x.transform.position == door.transform.position).needsCapping = false;
			newRoom.doorList.Find(x => x.transform.position == door.transform.position).isCapped = true;
			door.needsCapping = false;
			door.isCapped = true;

			if(usePrefabsOnlyOnce)
				roomPrefabs.Remove(selectedPrefab);

			callback(newRoom);
			yield break;
		}
		else
		{
			DestroyRoom(newRoom);
			callback(null);
			yield break;
		}
	}

	private bool CheckIfRoomFits(Room newRoom)
	{
		/*
		 * This function checks whether a room collides with others.
		 */

		Collider2D[] overlappingColliders = new Collider2D[1000];
		Collider2D newRoomCollider = newRoom.gameObject.GetComponent<Collider2D>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.useTriggers = true;
		filter.SetLayerMask(LayerMask.GetMask("OverlapCollider"));
		filter.useLayerMask = true;

		if (newRoomCollider.OverlapCollider(filter, overlappingColliders) == 0)
			return true;
		return false;
	}

	private void DestroyRoom(Room room)
	{
		levelRoomList.Remove(room);
		Destroy(room.gameObject);
	}

	private IEnumerator AddBossRoom(Transform map)
	{
		/*
		 * Essentially the same as TryAddRoom(). 
		 */

		Door[] doors = map.gameObject.GetComponentsInChildren<Door>();

		foreach (Door d in doors)
		{
			if (d.needsCapping)
			{
				foreach (GameObject prefab in bossRoomPrefabs)
				{
					GameObject roomObj = Instantiate(prefab, d.transform.position, Quaternion.identity, map);
					Room bossRoom = roomObj.AddComponent<Room>();

					foreach (Transform T in bossRoom.transform.Find("Walls").Find("Doors"))
						bossRoom.AddDoor(T.gameObject.AddComponent<Door>());

					if (!CheckAndAlignDoors(d, bossRoom))
					{
						DestroyRoom(bossRoom);
						continue;
					}

					if (bossRoom == null)
						continue;

					yield return new WaitForFixedUpdate();

					if (CheckIfRoomFits(bossRoom))
					{
						bossRoom.doorList.Find(x => x.transform.position == d.transform.position).needsCapping = false;
						bossRoom.doorList.Find(x => x.transform.position == d.transform.position).isCapped = true;
						d.needsCapping = false;
						d.isCapped = true;

						finalRoomAdded = true;

						yield break;
					}
					else
						DestroyRoom(bossRoom);
				}
			}
		}
	}

	private bool CheckAndAlignDoors(Door door, Room newRoom)
	{
		/*
		 * This function checks if a potential new room has a door that can connect to an existing door.
		 * If it does, align the transform.
		 */

		Door newDoor = null;

		//	Find a door in the new room that corresponds to the provides door.
		switch (door.doorType)
		{
			case Door.DoorType.TOP:
				newDoor = newRoom.doorList.Find(x => x.doorType == Door.DoorType.BOTTOM);
				break;
			case Door.DoorType.BOTTOM:
				newDoor = newRoom.doorList.Find(x => x.doorType == Door.DoorType.TOP);
				break;
			case Door.DoorType.LEFT:
				newDoor = newRoom.doorList.Find(x => x.doorType == Door.DoorType.RIGHT);
				break;
			case Door.DoorType.RIGHT:
				newDoor = newRoom.doorList.Find(x => x.doorType == Door.DoorType.LEFT);
				break;
		}

		//	If it finds a corresponding, the room is moved to line the two doors up.
		if (newDoor != null)
		{
			newRoom.transform.position = new Vector3(	newRoom.transform.position.x - newDoor.transform.localPosition.x,
														newRoom.transform.position.y - newDoor.transform.localPosition.y);
			return true;
		}

		return false;
	}

	private void CapRemainingDoors(object sender, EventArgs args)
	{
		/*
		 * This function is run after all the rooms have been placed. It places caps over the remaining exposed doors.
		 */

		Door[] doorArray = level.gameObject.GetComponentsInChildren<Door>();

		foreach (Door door in doorArray)
			if (door.needsCapping)
				CapThisDoor(door);
	}

	private void CapThisDoor(Door door)
	{
		/*
		 * This function determines what cap should cover a given door, then places that cap over it, and fixes its sorting layer.
		 */

		GameObject cap = null;

		switch (door.doorType)
		{
			case Door.DoorType.TOP:
				cap = topCap;
				break;
			case Door.DoorType.BOTTOM:
				cap = bottomCap;
				break;
			case Door.DoorType.LEFT:
				cap = leftCap;
				break;
			case Door.DoorType.RIGHT:
				cap = rightCap;
				break;
		}

		if (cap != null)
		{
			cap = Instantiate(cap, door.transform.position, Quaternion.identity, door.transform.parent);

			Renderer[] renderers = cap.GetComponentsInChildren<Renderer>();

			foreach (Renderer rend in renderers)
				rend.sortingOrder = (int)(sortingOrderBase - rend.transform.position.y + 1);
		}

		door.needsCapping = false;
		door.isCapped = true;
	}

	//---

	private void CorrectSortingLayers(object sender, EventArgs args)
	{
		/*
		 * This function is run after all the rooms have been placed. It fixes the sorting layers for wall spriteRenderers.
		 */

		SpriteRenderer[] renderers = level.GetComponentsInChildren<SpriteRenderer>();

		foreach (SpriteRenderer rend in renderers)
			if (rend.gameObject.layer == LayerMask.NameToLayer("Walls"))
				rend.sortingOrder = (int)(sortingOrderBase - rend.transform.position.y);
	}

	private void ApplyMaterials(object sender, EventArgs args)
	{
		SpriteRenderer[] renderers = level.GetComponentsInChildren<SpriteRenderer>();

		foreach (SpriteRenderer rend in renderers)
		{
			if (rend.gameObject.layer == LayerMask.NameToLayer("Walls"))
				rend.material = wallMaterial;
			if (rend.gameObject.layer == LayerMask.NameToLayer("Floor"))
				rend.material = floorMaterial;
		}
	}

	private void SetLightIntensity(object sender, EventArgs args)
	{
		UnityEngine.Rendering.Universal.Light2D[] lights = level.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>();

		foreach (UnityEngine.Rendering.Universal.Light2D light in lights)
			light.intensity = lightIntensity;
	}

	private void GenerateLevelFloorMesh(object sender, EventArgs args)
	{
		/*
		 * This function joins all the preexisting floorspace meshes from each room, into a single level spanning floorspace mesh.
		 * This will be used for mesh-based AI pathfinding. Maybe.
		 */

		List<MeshFilter> meshFilters = new List<MeshFilter>();
		Transform map = level.transform.Find("Map");

		foreach (Transform room in map)
			meshFilters.Add(room.GetComponent<MeshFilter>());

		CombineInstance[] combine = new CombineInstance[meshFilters.ToArray().Length];

		for (int i = 0; i < combine.Length; i++)
		{
			combine[i].mesh = meshFilters[i].sharedMesh;

			Matrix4x4 matrix = meshFilters[i].transform.localToWorldMatrix;
			matrix[0, 3] = meshFilters[i].transform.localPosition.x;
			matrix[1, 3] = meshFilters[i].transform.localPosition.y;
			matrix[2, 3] = meshFilters[i].transform.localPosition.z;

			combine[i].transform = matrix;
		}

		map.GetComponent<MeshFilter>().mesh = new Mesh();
		map.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
		map.GetComponent<MeshFilter>().mesh.Optimize();
	}

	private void GeneratePaths(object sender, EventArgs args)
	{
		foreach (PfNode node in level.GetComponentsInChildren<PfNode>())
			pathfindingNodes.Add(node);

		Pathfinding.ConnectNodes(pathfindingNodes);
		Pathfinding.RemoveUnconnectedNodes(pathfindingNodes);

		if (OnPathsGenerated != null) OnPathsGenerated(this, EventArgs.Empty);
	}

	private void SpawnItems(object sender, EventArgs e)
	{
		GameObject items = new GameObject("Items");
		items.transform.SetParent(level.transform);

		foreach (ItemSpawner spawner in level.GetComponentsInChildren<ItemSpawner>())
		{
			GameObject item = scriptableItemList.GetWeightedItem();

			if (item != null)
				Instantiate(item, spawner.transform.position, Quaternion.identity, items.transform);
		}
	}

	private void SpawnNPCs(object sender, EventArgs e)
	{
		GameObject chars = new GameObject("Characters");
		chars.transform.SetParent(level.transform);

		List<NPCSpawner> npcSpawners = new List<NPCSpawner>();

		foreach (NPCSpawner spawner in level.GetComponentsInChildren<NPCSpawner>())
			npcSpawners.Add(spawner);

		if (npcSpawners.Count == 0)
			return;

		int npcCount = 0;

		do
		{
			npcSpawners.Shuffle();

			Instantiate(scriptableNPCList.npcPrefabs[UnityEngine.Random.Range(0, scriptableNPCList.npcPrefabs.Count)], npcSpawners[0].transform.position, Quaternion.identity, chars.transform);

			npcCount++;
		}
		while (npcCount < npcSpawners.Count * npcsPerSpawner);
	}

	private void FinishGeneratingMap(object sender, EventArgs args)
	{
		if (OnMapGenerationComplete != null) OnMapGenerationComplete(this, EventArgs.Empty);
	}

	public void DestroyLevel()
	{
		Destroy(level);

	//levelRoomList.Clear();

	//	foreach(Transform t in level.transform)
	//		Destroy(t);
	}

	public static List<PfNode> GetPathfindingNodes()
	{
		return pathfindingNodes;
	}

	public static PlayerSpawner GetPlayerSpawner()
	{
		return level.GetComponentInChildren<PlayerSpawner>();
	}

	//	Nested Classes

	public class Room : MonoBehaviour
	{
		[SerializeField] public List<Door> doorList = new List<Door>();
		[SerializeField] public List<Room> connectedRooms = new List<Room>();

		~Room()
		{
			foreach (Door d in doorList)
				Destroy(d);

			doorList.Clear();
		}

		public void AddDoor(Door door)
		{
			if (door.transform.name.Contains("Door_Left"))
				door.doorType = Door.DoorType.LEFT;
			else if (door.transform.name.Contains("Door_Bottom"))
				door.doorType = Door.DoorType.BOTTOM;
			else if (door.transform.name.Contains("Door_Right"))
				door.doorType = Door.DoorType.RIGHT;
			else if (door.transform.name.Contains("Door_Top"))
				door.doorType = Door.DoorType.TOP;
			else
				Debug.Log("Could not determine type of door.");

			doorList.Add(door);
		}

		public void ConnectRoom(Room room)
		{
			connectedRooms.Add(room);
		}
	}

	public class Door : MonoBehaviour
	{
		[SerializeField]
		public DoorType doorType;
		public bool isCapped = false;
		public bool needsCapping = true;

		public enum DoorType { TOP, BOTTOM, LEFT, RIGHT, NULL }
	}
}
