using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class PlayerAimController : MonoBehaviour
{
	[SerializeField] private float offset = -10f;
	[SerializeField] private float lerpSpeed = 10f;
	[SerializeField] private float maxCameraDistance = 5f;

	Transform aimTransform;
	Transform torch;

	private float distance, allowableDistance;

	void Awake()
	{
		aimTransform = transform.Find("ActiveWeapon");
		torch = transform.Find("Torch");
	}

	void FixedUpdate()
	{
		Vector3 mousePosition = Utils.GetMouseWorldPosition();

		ChangePlayerFacing(mousePosition);
		MoveCamera(mousePosition);
	}

	public void ChangePlayerFacing(Vector3 mousePosition)
	{
		/*
		 * This function is to update the direction of the player and weapon. It is called in FixedUpdate().
		 * It determines a Vector2 that is the direction from the player to the mouse cursor. It converts that Vector2 into a angle in degrees, and sets the weapon to point in that direction.
		 * It also passes that angle to FixModelRotation() so that the model is flipped when looking in the other direction.
		 */

		Vector2 lookDirection = ((Vector2) mousePosition - (Vector2) transform.position).normalized;
		float lookAngle = (Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg);

		aimTransform.localPosition = Utils.RotatePointAroundPoint(aimTransform.localPosition, Vector3.zero, lookAngle - aimTransform.eulerAngles.z);
		aimTransform.eulerAngles = new Vector3(0f, 0f, lookAngle);

		torch.localPosition = Utils.RotatePointAroundPoint(torch.localPosition, Vector3.zero, lookAngle - torch.eulerAngles.z);
		torch.eulerAngles = new Vector3(0f, 0f, lookAngle);

		Vector3 flippedScale = new Vector3(-1f, 1f, 1f);
		if (lookAngle > 90f || lookAngle < -90f)
		{
			gameObject.GetComponent<SpriteRenderer>().flipX = true;
			if(aimTransform.GetComponentInChildren<SpriteRenderer>() != null)
				aimTransform.GetComponentInChildren<SpriteRenderer>().flipY = true;
		}
		else
		{
			gameObject.GetComponent<SpriteRenderer>().flipX = false;
			if (aimTransform.GetComponentInChildren<SpriteRenderer>() != null)
				aimTransform.GetComponentInChildren<SpriteRenderer>().flipY = false;
		}
	}

	public void MoveCamera(Vector3 mousePosition)
	{
		/*
		 * This function is to update the camera position. It is called in FixedUpdate().
		 * It creates a Vector3 that is the midpoint between the players position and the mouse cursor position (and fixing z offset).
		 * When the mouse cursor is on the screen, the camera is lerped to that position.
		 */

		Vector3 cameraPos = new Vector3((transform.position.x + mousePosition.x) / 2, (transform.position.y + mousePosition.y) / 2);
		cameraPos.z = offset;

		if (Utils.MouseIsOnScreen())
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, cameraPos, Time.deltaTime * 5);
	}

	public void ResetCamera()
	{
		Camera.main.transform.position = transform.position;
	}
}
