using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class NPCAimController : MonoBehaviour
{
	private NPCCharacter npc;

	private Transform aimTransform;

	void Awake()
	{
		npc = gameObject.GetComponent<NPCCharacter>();
		aimTransform = transform.Find("ActiveWeapon");
	}

	void FixedUpdate()
	{
		if(npc.hasLOS)
			AimAtTarget();

		if (!npc.hasTarget)
			AimAtPoint(npc.GetComponent<NPCMovementController>().GetDestination());
	}

	public void AimAtTarget()
	{
		PlayerCharacter target = npc.GetCurrentTarget();

		Vector3 airDirection = (target.transform.position - transform.position).normalized;
		float lookAngle = (Mathf.Atan2(airDirection.y, airDirection.x) * Mathf.Rad2Deg);

		aimTransform.localPosition = Utils.RotatePointAroundPoint(aimTransform.localPosition, Vector3.zero, lookAngle - aimTransform.eulerAngles.z);
		aimTransform.eulerAngles = new Vector3(0f, 0f, lookAngle);

		// ***** flipping the sprite *****
		Vector3 flippedScale = new Vector3(-1f, 1f, 1f);
		if (lookAngle > 90f || lookAngle < -90f)
			gameObject.GetComponent<SpriteRenderer>().flipX = true;
		else
			gameObject.GetComponent<SpriteRenderer>().flipX = false;
	}

	public void AimAtPoint(Vector3 point)
	{
		Vector3 airDirection = (point - transform.position).normalized;
		float lookAngle = (Mathf.Atan2(airDirection.y, airDirection.x) * Mathf.Rad2Deg);

		aimTransform.localPosition = Utils.RotatePointAroundPoint(aimTransform.localPosition, Vector3.zero, lookAngle - aimTransform.eulerAngles.z);
		aimTransform.eulerAngles = new Vector3(0f, 0f, lookAngle);

		// ***** flipping the sprite *****
		Vector3 flippedScale = new Vector3(-1f, 1f, 1f);
		if (lookAngle > 90f || lookAngle < -90f)
			gameObject.GetComponent<SpriteRenderer>().flipX = true;
		else
			gameObject.GetComponent<SpriteRenderer>().flipX = false;
	}
}
