using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class PlayerContols : MonoBehaviour
{
	public event EventHandler OnLeftClickDown, OnLeftClick, OnLeftClickUp, OnRightClick, OnReloadButton, OnSpace, OnLeftShiftDown, OnLeftShiftUp, OnTabPressed, OnScrollWheel, OnDropWeaponButton, OnTorchKeyPressed;
	public event EventHandler<OnMovementKeyHeldArgs> OnMouseOffScreen, OnWKeyHeld, OnAKeyHeld, OnSKeyHeld, OnDKeyHeld, OnWKeyReleased, OnAKeyReleased, OnSKeyReleased, OnDKeyReleased;

	public class OnMovementKeyHeldArgs : EventArgs
	{ public Vector3 dir; }

	void Awake()
	{
		DontDestroyOnLoad(this);
	}

	void Update()
	{
		if (GameManager.IsPaused())
			return;

		if (!Utils.MouseIsOnScreen())
			if (OnMouseOffScreen != null) OnMouseOffScreen(this, new OnMovementKeyHeldArgs { dir = Vector2.zero });

		if (Input.GetMouseButtonDown(0))
			if (OnLeftClickDown != null) OnLeftClickDown(this, EventArgs.Empty);

		if (Input.GetMouseButton(0))
			if (OnLeftClick != null) OnLeftClick(this, EventArgs.Empty);

		if (Input.GetMouseButtonUp(0))
			if (OnLeftClickUp != null) OnLeftClickUp(this, EventArgs.Empty);

		if (Input.GetMouseButtonDown(1))
			if (OnRightClick != null) OnRightClick(this, EventArgs.Empty);

		if (Input.GetKeyDown(KeyCode.W))
			if (OnWKeyHeld != null) OnWKeyHeld(this, new OnMovementKeyHeldArgs { dir = Vector2.up });

		if (Input.GetKeyDown(KeyCode.A))
			if (OnAKeyHeld != null) OnAKeyHeld(this, new OnMovementKeyHeldArgs { dir = Vector2.left });

		if (Input.GetKeyDown(KeyCode.S))
			if (OnSKeyHeld != null) OnSKeyHeld(this, new OnMovementKeyHeldArgs { dir = Vector2.down });

		if (Input.GetKeyDown(KeyCode.D))
			if (OnDKeyHeld != null) OnDKeyHeld(this, new OnMovementKeyHeldArgs { dir = Vector2.right });

		if (Input.GetKeyUp(KeyCode.W))
			if (OnWKeyReleased != null) OnWKeyReleased(this, new OnMovementKeyHeldArgs { dir = Vector2.down });

		if (Input.GetKeyUp(KeyCode.A))
			if (OnAKeyReleased != null) OnAKeyReleased(this, new OnMovementKeyHeldArgs { dir = Vector2.right });

		if (Input.GetKeyUp(KeyCode.S))
			if (OnSKeyReleased != null) OnSKeyReleased(this, new OnMovementKeyHeldArgs { dir = Vector2.up });

		if (Input.GetKeyUp(KeyCode.D))
			if (OnDKeyReleased != null) OnDKeyReleased(this, new OnMovementKeyHeldArgs { dir = Vector2.left });

		if (Input.GetKeyDown(KeyCode.R))
			if (OnReloadButton != null) OnReloadButton(this, EventArgs.Empty);

		if (Input.GetKeyDown(KeyCode.X))
			if (OnDropWeaponButton != null) OnDropWeaponButton(this, EventArgs.Empty);

		if (Input.GetKeyDown(KeyCode.Space))
			if (OnSpace != null) OnSpace(this, EventArgs.Empty);

		if (Input.GetKeyDown(KeyCode.LeftShift))
			if (OnLeftShiftDown != null) OnLeftShiftDown(this, EventArgs.Empty);

		if (Input.GetKeyUp(KeyCode.LeftShift))
			if (OnLeftShiftUp != null) OnLeftShiftUp(this, EventArgs.Empty);

		if (Input.GetKey(KeyCode.Tab))
			if (OnTabPressed != null) OnTabPressed(this, EventArgs.Empty);

		if (Input.GetKeyDown(KeyCode.Q))
			if (OnTorchKeyPressed != null) OnTorchKeyPressed(this, EventArgs.Empty);

		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
			if (OnScrollWheel != null) OnScrollWheel(this, EventArgs.Empty);
	}
}
