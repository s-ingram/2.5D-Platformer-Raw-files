using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
	private Rigidbody2D rigidbody2D;

	private PlayerCharacter player;

	private Animator animator;

	[SerializeField]
	private int offset = -10;
	private int sortingOrderBase = 5000;
	private Renderer rend;

	[SerializeField]
	bool isIdle, isMoving, isAttempingSneaking, isSneaking, isDashing, isLocked, dashIsOnCooldown;

	Vector3 moveDirection, newMoveDirection;
	Vector3 upDir, downDir, leftDir, rightDir;

	void Awake()
	{
		DontDestroyOnLoad(this);

		player = gameObject.GetComponent<PlayerCharacter>();
		rigidbody2D = GetComponent<Rigidbody2D>();
		rend = gameObject.GetComponentInChildren<Renderer>();
		animator = gameObject.GetComponentInChildren<Animator>();

		gameObject.GetComponent<PlayerContols>().OnMouseOffScreen += UpdateDirection;
		gameObject.GetComponent<PlayerContols>().OnWKeyHeld += UpdateDirection;
		gameObject.GetComponent<PlayerContols>().OnAKeyHeld += UpdateDirection;
		gameObject.GetComponent<PlayerContols>().OnSKeyHeld += UpdateDirection;
		gameObject.GetComponent<PlayerContols>().OnDKeyHeld += UpdateDirection;
		gameObject.GetComponent<PlayerContols>().OnWKeyReleased += MovementKeysReleased;
		gameObject.GetComponent<PlayerContols>().OnAKeyReleased += MovementKeysReleased;
		gameObject.GetComponent<PlayerContols>().OnSKeyReleased += MovementKeysReleased;
		gameObject.GetComponent<PlayerContols>().OnDKeyReleased += MovementKeysReleased;
		gameObject.GetComponent<PlayerContols>().OnSpace += Dash;
		gameObject.GetComponent<PlayerContols>().OnLeftShiftDown += Sneak;
		gameObject.GetComponent<PlayerContols>().OnLeftShiftUp += Sneak;
	}

	void FixedUpdate()
	{
		if (!isLocked)
			moveDirection = newMoveDirection;

		isIdle = moveDirection.magnitude == 0;
		isMoving = !isIdle && !isDashing && !isSneaking;

		//if (isIdle)

		if (isMoving)
		{
			HandleMovement();
		}
		if (isDashing)
			HandleDash();
		if (isSneaking)
			HandleSneak();
		else
			player.ResetVisibility();

		ChangeAnimatorState();
	}

	void LateUpdate()
	{
		rend.sortingOrder = (int)(sortingOrderBase - transform.position.y - offset);
	}

	private void UpdateDirection(object sender, PlayerContols.OnMovementKeyHeldArgs e)
	{
		/*
		 * This function is to determine which direction to move the character. It is triggered by a number of OnKey events.
		 * Set the new move direction based on what movement keys are being pressed.
		 */

		//newMoveDirection += e.dir;

		if (e.dir == Vector3.up)
			upDir = Vector3.up;
		if (e.dir == Vector3.down)
			downDir = Vector3.down;
		if (e.dir == Vector3.left)
			leftDir = Vector3.left;
		if (e.dir == Vector3.right)
			rightDir = Vector3.right;

		newMoveDirection = upDir + downDir + leftDir + rightDir;
	}

	private void MovementKeysReleased(object sender, PlayerContols.OnMovementKeyHeldArgs e)
	{
		if (e.dir == Vector3.up)
			downDir = Vector3.zero;
		if (e.dir == Vector3.down)
			upDir = Vector3.zero;
		if (e.dir == Vector3.left)
			rightDir = Vector3.zero;
		if (e.dir == Vector3.right)
			leftDir = Vector3.zero;

		newMoveDirection = upDir + downDir + leftDir + rightDir;
	}

	private void HandleMovement()
	{
		/*
		 * This funtion moves the character in a direction at a speed.
		 */

		transform.position += moveDirection * player.GetMoveSpeed() * Time.deltaTime;
	}

	private void Dash(object sender, EventArgs e)
	{
		/*
		 * This function is to start a dash. It is triggered by the OnSpace event.
		 * If dash is not on cooldown, and the character is not already dashing, then sets isDashing to true.
		 */

		if (!dashIsOnCooldown && !isDashing)
			isDashing = true;
	}

	private void HandleDash()
	{
		/*
		 * This function is to hand the dash movement. It is called in FixedUpdate().
		 * If the player can move in the dash direction, then it moves.
		 * Then lock the players movement controls for a time. After that time, stop dashing.
		 */
	
		if (CanMove(moveDirection, 1f))
			transform.position += moveDirection * player.GetDashSpeed() * Time.deltaTime;

		StartCoroutine(LockControls(player.GetDashLockTime()));
		Invoke("StopDashing", player.GetDashLockTime());
	}

	private void Sneak(object sender, EventArgs e)
	{
		isSneaking = !isSneaking;
	}

	private void HandleSneak()
	{
		if (!isDashing)
		{
			transform.position += moveDirection * player.GetSneakSpeed() * Time.deltaTime;
			player.SetCurrentVisibility(player.GetBaseVisibility() * 0.2f);
		}
	}

	private IEnumerator ResetDashCooldown()
	{
		/*
		 * This function is to have dash on cooldown for a certain time.
		 */

		dashIsOnCooldown = true;
		yield return new WaitForSeconds(player.GetDashCooldownTime());
		dashIsOnCooldown = false;
	}

	public IEnumerator LockControls(float seconds)
	{
		/*
		 * This function is to lock the controls for a certain time.
		 */

		isLocked = true;
		yield return new WaitForSeconds(seconds);
		isLocked = false;
	}

	private void StopDashing()
	{
		/*
		 * This function is stop a dash movement
		 */

		isDashing = false;
		StartCoroutine(ResetDashCooldown());
	}

	private bool CanMove(Vector3 direction, float distance)
	{
		/*
		 * This function is to determine whether the player can move in a given direction and distace.
		 * It fires a Raycast from the players position, in a given direction and distance, looking for colliders with the "Walls" layer mask.
		 * Returns true if there are no walls in the way. Otherwise, returns false.
		 */ 

		return Physics2D.Raycast(transform.position, direction, distance, LayerMask.GetMask("Walls")).collider == null;
	}

	private void ChangeAnimatorState()
	{
		if (isIdle)
			animator.SetInteger("State", 0);
		else if (isMoving)
			animator.SetInteger("State", 1);
	}

	public void ResetMoveDirection()
	{
		moveDirection = Vector3.zero;
		newMoveDirection = Vector3.zero;
	}
}
