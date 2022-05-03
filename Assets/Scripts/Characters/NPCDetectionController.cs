using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDetectionController : MonoBehaviour
{
	private NPCCharacter npc;

	[SerializeField] private int LOSChecksFailed;
	//private PlayerCharacter target;

	public event EventHandler<OnTargetUpdatedEventArgs> OnTargetAcquired, OnTargetLost, OnTrackTarget;

	public class OnTargetUpdatedEventArgs : EventArgs
	{
		public PlayerCharacter target;
		public Vector3 position;
	}

	void Awake()
	{
		npc = gameObject.GetComponent<NPCCharacter>();

		InvokeRepeating("TrackTarget", 0f, 0.1f);
	}

	private void TrackTarget()
	{
		/*
		 * This function is for the NPC to keep track of the target. It is invoked in Awake() and repeats every one-tenth second.
		 * If the NPC does have a target, it will do a Raycast in the direction of the target to check whether it still has line of sight.
		 * If it does NOT have LOS, it will increase a tally of failed LOS checks. If the number of failed LOS checks exceeds a threshold, the target is lost.
		 * If the NPC does NOT have a target, it will skip of the Raycasting.
		 */

		if (npc.hasTarget)
		{
			npc.hasLOS = CheckLOS();

			if (npc.hasLOS)
				LOSChecksFailed = 0;
			else
				LOSChecksFailed++;

			if (LOSChecksFailed > npc.GetLOSFailThreshold())
				TargetLost();
		}
	}

	public bool CheckLOS()
	{
		gameObject.GetComponent<Collider2D>().enabled = false;
		RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, npc.GetCurrentTarget().transform.position);
		gameObject.GetComponent<Collider2D>().enabled = true;

		//RaycastHit2D hitWalls = Array.Find(hits, x => x.collider.CompareTag("Walls"));
		//RaycastHit2D hitPlayer = Array.Find(hits, x => x.collider.CompareTag("Player"));

		bool hitWalls = false;
		bool hitPlayer = false;

		foreach (RaycastHit2D hit in hits)
			if (hit.collider.CompareTag("Walls"))
				hitWalls = true;

		foreach (RaycastHit2D hit in hits)
			if (hit.collider.CompareTag("Player"))
				hitPlayer = true;

		if (hitPlayer && !hitWalls && Vector2.Distance(transform.position, npc.GetCurrentTarget().transform.position) <= npc.GetLoseSightDistance())
		{
			if (OnTrackTarget != null) OnTrackTarget(this, new OnTargetUpdatedEventArgs { target = npc.GetCurrentTarget(), position = npc.GetCurrentTarget().transform.position });
			return true;
		}

		return false;
	}

	private void TargetLost()
	{
		/*
		 * Resets the NPC's current target to null.
		 */

		LOSChecksFailed = 0;

		if (OnTargetLost != null) OnTargetLost(this, new OnTargetUpdatedEventArgs { target = null });
	}
}
