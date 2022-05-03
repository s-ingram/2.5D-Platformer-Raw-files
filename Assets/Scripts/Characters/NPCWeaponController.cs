using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWeaponController : MonoBehaviour
{
	private NPCCharacter npc;
	private Weapon activeWeapon;

	void Awake()
	{
		npc = gameObject.GetComponent<NPCCharacter>();
	}

	private void Update()
	{
		if (npc.hasLOS)
			if (activeWeapon != null)
			{
				activeWeapon.FireWeapon(this, EventArgs.Empty);
				activeWeapon.ReleaseTrigger(this, EventArgs.Empty);
			}

		if(activeWeapon != null)
			if (activeWeapon.IsEmpty())
				activeWeapon.Reload(this, EventArgs.Empty);
	}



	public void ActivateWeapon(Weapon weapon)
	{
		/*
		 * Subscribes the active weapon to a number of events.
		 */

		activeWeapon = weapon;

		//gameObject.GetComponent<PlayerContols>().OnLeftClick += activeWeapon.FireWeapon;
		//gameObject.GetComponent<PlayerContols>().OnLeftClickUp += activeWeapon.ReleaseTrigger;
		//gameObject.GetComponent<PlayerContols>().OnRightClick += activeWeapon.AltFireWeapon;	//	Setup for any alternative fire modes operated by right click.
		//gameObject.GetComponent<PlayerContols>().OnReloadButton += activeWeapon.Reload;
	}

	public void DeactivateWeapon(Weapon weapon)
	{
		/*
		 * Unsubscribes the active weapon to a number of events.
		 */

		if (activeWeapon == weapon)
		{
			activeWeapon = null;
			
			//gameObject.GetComponent<PlayerContols>().OnLeftClick -= activeWeapon.FireWeapon;
			//gameObject.GetComponent<PlayerContols>().OnLeftClickUp -= activeWeapon.ReleaseTrigger;
			//gameObject.GetComponent<PlayerContols>().OnRightClick -= activeWeapon.AltFireWeapon;	//	Setup for any alternative fire modes operated by right click.
			//gameObject.GetComponent<PlayerContols>().OnReloadButton -= activeWeapon.Reload;
		}
	}

	public float GetBestWeaponRange()
	{
		if(activeWeapon != null)
			return activeWeapon.GetWeaponMaxRange() / 2;
		return 0f;
	}

	public Weapon GetActiveWeapon()
	{
		return activeWeapon;
	}
}
