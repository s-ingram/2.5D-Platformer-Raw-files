using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class NPCMovementController : MonoBehaviour
{
	private NPCCharacter npc;
	private Animator animator;

	[SerializeField]
	private int offset = 0;
	private int sortingOrderBase = 5000;
	private Renderer rend;

	[SerializeField] private Vector3 destination;
	[SerializeField] private bool isIdle, isMoving;

	private List<PfNode> patrolMarkers;

	void Start()
	{
		npc = gameObject.GetComponent<NPCCharacter>();
		animator = gameObject.GetComponent<Animator>();
		rend = gameObject.GetComponent<Renderer>();

		destination = transform.position;

		//patrolMarkers = GameManager.Instance.GetListOfPatrolMarkers();
		patrolMarkers = LevelGenerator.GetPathfindingNodes();

		InvokeRepeating("FindNextPatrolMarker", Random.Range(0f, 6f), 6f);
	}

	private void FixedUpdate()
	{
		//Debug.DrawLine(transform.position, destination);

		isIdle = transform.position == destination;
		isMoving = !isIdle;

		if (!npc.IsTargetDummy())
		{
			if (npc.hasTarget)
			{
				if (!npc.hasLOS)
					destination = FindFiringPosition();

				MoveToFiringPosition();
			}

			if (!npc.hasTarget)
				MoveIdle();
		}
		else
			isIdle = true;

		ChangeAnimatorState();
	}

	void LateUpdate()
	{
		rend.sortingOrder = (int)(sortingOrderBase - transform.position.y - offset);
	}

	private void MoveIdle()
	{
		transform.position = Vector3.MoveTowards(transform.position, destination, npc.GetMoveSpeed() / 2 * Time.fixedDeltaTime );
	}

	private void MoveToFiringPosition()
	{
		transform.position = Vector3.MoveTowards(transform.position, destination, npc.GetMoveSpeed() * Time.fixedDeltaTime);
	}

	private Vector3 FindFiringPosition()
	{
		/*
		 * Randomly generate a Vector3.
		 * if it is further from the player than the target already is, restart.
		 * ie. if( distance(this, point) > distance(this, target))
		 *			continue;
		 * do a Raycast from the point to the target, to check if that point has LOS.
		 */

		int i = 0;
		bool newPosHasLOS = false;
		float bestWeaponRange = npc.GetComponent<NPCWeaponController>().GetBestWeaponRange();

		Vector3 potentialFiringPosition = transform.position;

		//Debug.DrawLine(transform.position, npc.targetLastKnownPos, Color.red);
		//Debug.DrawLine(transform.position, ((transform.position - npc.targetLastKnownPos).normalized * bestWeaponRange) + npc.targetLastKnownPos, Color.blue);

		do
		{
			potentialFiringPosition = ((transform.position - npc.targetLastKnownPos).normalized * bestWeaponRange) + npc.targetLastKnownPos;

			if(Physics2D.Linecast(potentialFiringPosition, npc.GetCurrentTarget().transform.position, 1 << LayerMask.NameToLayer("Walls")).collider == null)
				newPosHasLOS = true;

			i++;
		}
		while (!newPosHasLOS && i < 10);

		return potentialFiringPosition;
	}

	private void FindNextPatrolMarker()
	{
		/*
		 * if the NPC is idle, find the closest patrolMarker that the NPC can see, go to it.
		 */

		if (!npc.hasTarget)
		{
			patrolMarkers.Shuffle();

			foreach (PfNode marker in patrolMarkers)
			{
				if (Vector3.Distance(transform.position, marker.transform.position) < 1f || Vector3.Distance(transform.position, marker.transform.position) > 10f)
					continue;

				if (Physics2D.Linecast(transform.position, marker.transform.position, 1 << LayerMask.NameToLayer("Walls")).collider == null)
				{
					destination = Utils.GetRandomPointNear(marker.transform.position, 1f);
					break;
				}
			}
		}
	}

	private void ChangeAnimatorState()
	{
		if (isIdle)
			animator.SetInteger("State", 0);
		else if (isMoving)
			animator.SetInteger("State", 1);
	}

	public Vector3 GetDestination()
	{ return destination; }
}
