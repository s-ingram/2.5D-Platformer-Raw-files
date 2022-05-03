using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ScriptableRoomGenParams", menuName = "ScriptableObjects/ScriptableRoomGenParams")]
public class ScriptableRoomGenParams : ScriptableObject
{
	[SerializeField]
	public bool useStartingRoomPrefabs, useBossRooms, usePrefabsInOrder, usePrefabsOnlyOnce;

	[SerializeField]
	public Material wallMaterial, floorMaterial;

	[SerializeField]
	public float globalLightIntensity = 0.2f, lightIntensity = 1f;

	[SerializeField]
	public int roomMinimum, roomMaximum, npcsPerSpawner;

	[SerializeField]
	public GameObject topCap, bottomCap, leftCap, rightCap;

	[SerializeField]
	public List<GameObject> startingRoomPrefabs;

	[SerializeField]
	public List<GameObject> bossRoomPrefabs;

	[SerializeField]
	public List<GameObject> roomPrefabs;
}
