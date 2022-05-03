using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
	private Weapon activeWeapon;

	public event EventHandler<Weapon.OnAmmoUpdateArgs> OnAmmoUpdate;

	public void ActivateWeapon(Weapon weapon)
	{
		/*
		 * Subscribes the active weapon to a number of events.
		 */

		activeWeapon = weapon;

		gameObject.GetComponent<PlayerContols>().OnLeftClick += activeWeapon.FireWeapon;
		gameObject.GetComponent<PlayerContols>().OnLeftClickUp += activeWeapon.ReleaseTrigger;
		//gameObject.GetComponent<PlayerContols>().OnRightClick += activeWeapon.AltFireWeapon;	//	Setup for any alternative fire modes operated by right click.
		gameObject.GetComponent<PlayerContols>().OnReloadButton += activeWeapon.Reload;

		// weapon.OnWeaponFire += Pushback;
		weapon.OnWeaponFire += UpdateAmmoCounterUI;
		weapon.OnWeaponReload += UpdateAmmoCounterUI;
		weapon.OnWeaponReload += ShowReloadUI;
		UpdateAmmoCounterUI(weapon, new Weapon.OnAmmoUpdateArgs { currentAmmo = weapon.currentMagAmount, currentSpareAmmo = weapon.currentSpareAmmo });
	}

	public void DeactivateWeapon(Weapon weapon)
	{
		/*
		 * Unsubscribes the active weapon to a number of events.
		 */

		if (activeWeapon == weapon)
		{
			gameObject.GetComponent<PlayerContols>().OnLeftClick -= activeWeapon.FireWeapon;
			gameObject.GetComponent<PlayerContols>().OnLeftClickUp -= activeWeapon.ReleaseTrigger;
			//gameObject.GetComponent<PlayerContols>().OnRightClick -= activeWeapon.AltFireWeapon;	//	Setup for any alternative fire modes operated by right click.
			gameObject.GetComponent<PlayerContols>().OnReloadButton -= activeWeapon.Reload;

			weapon.OnWeaponFire -= UpdateAmmoCounterUI;
			weapon.OnWeaponReload -= UpdateAmmoCounterUI;
			weapon.OnWeaponReload -= ShowReloadUI;
			UpdateAmmoCounterUI(weapon, new Weapon.OnAmmoUpdateArgs { currentAmmo = 0, currentSpareAmmo = 0 });
		}
	}

	public void AddAmmo(int numberOfNewMagazines)
	{
		if (activeWeapon != null)
		{
			activeWeapon.currentSpareAmmo += activeWeapon.maxMagAmount * numberOfNewMagazines;
			activeWeapon.currentSpareAmmo = Mathf.Clamp(activeWeapon.currentSpareAmmo, 0, activeWeapon.maxSpareAmmo);
			UpdateAmmoCounterUI(activeWeapon, new Weapon.OnAmmoUpdateArgs { currentSpareAmmo = activeWeapon.currentSpareAmmo });
		}
	}

	public void UpdateAmmoCounterUI(object sender, Weapon.OnAmmoUpdateArgs args)
	{
		if (OnAmmoUpdate != null) OnAmmoUpdate(this, args);
	}

	public void ShowReloadUI(object sender, Weapon.OnAmmoUpdateArgs args)
	{
		GameManager.Instance.reloadUI.PlayAnimationForSeconds(args.reloadTime);
	}
}
