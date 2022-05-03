using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInventoryController : MonoBehaviour
{
	private NPCCharacter npc;
	private NPCWeaponController weaponController;

	private Queue<IPickupable> weaponInventory;
	private List<IPickupable> itemInventory;

	private Weapon activeWeapon;
	private int activeWeaponCount;

	void Awake()
	{
		npc = gameObject.GetComponent<NPCCharacter>();
		weaponController = gameObject.GetComponent<NPCWeaponController>();

		weaponInventory = new Queue<IPickupable>(npc.GetMaxWeapons());
		itemInventory = new List<IPickupable>(npc.GetMaxItems());

		EquipStartingWeapon();

		//gameObject.GetComponent<PlayerContols>().OnScrollWheel += CycleActiveWeapon;
		//gameObject.GetComponent<PlayerContols>().OnDropWeaponButton += DropActiveWeapon;
	}

	public void DropActiveWeapon()
	{
		/*
		 * This function is for dropping the current active weapon. It is triggered by the OnDropWeaponButton event.
		 * It first tells the weaponController to deactivate the active weapon. It then runs the weapons Drop() function, before setting the active weapon to null.
		 */

		if (activeWeapon != null)
		{
			weaponController.DeactivateWeapon(activeWeapon);
			activeWeapon.Drop(transform.position);
			activeWeapon = null;
			activeWeaponCount = 0;
		}
	}

	private void MakeActiveWeapon(Weapon weapon)
	{
		/*
		 * This function is for making a weapon the active weapon. It involves setting a new parent gameObject, setting position and rotation, enabling Renderers.
		 * Once that is done, it sends a reference of the active weapon to the weaponController, to be activated.
		 */

		activeWeapon = weapon;
		activeWeaponCount = 1;
		weapon.transform.SetParent(transform.Find("ActiveWeapon"));
		weapon.transform.position = weapon.transform.parent.position;
		weapon.transform.rotation = weapon.transform.parent.rotation;
		//weapon.GetComponentInChildren<SpriteRenderer>().enabled = true;
		weaponController.ActivateWeapon(weapon);
	}

	private bool TryAddToInventory(IPickupable pickUp)
	{
		/* 
		 * This function is to manage adding new items/weapons to the inventory. It is called in OnTriggerEnter2D(), CycleActiveWeapon(), and EquipStartingWeapon().
		 * A reference to an IPickup is passed with the call. If that item is null, return false. If that item is a weapon, determine if the character can pick it up by checking how many weapons he is currently carrying.
		 * If he can pick it up, and has no active weapon, immediately make it the active weapon. Otherwise, have the weaponController first deactivate it, then change its parent to be the Inventory object.
		 * Then Enqueue it into the inventory. Return true, as the item has been picked up.
		 * 
		 * TODO: Items
		 */

		if (pickUp is null)
		{ return false; }

		if (pickUp is Weapon)
		{
			if (weaponInventory.Count + activeWeaponCount < npc.GetMaxWeapons())
			{
				if (activeWeapon == null)
				{
					pickUp.PickUp(transform.Find("ActiveWeapon"));
					MakeActiveWeapon((Weapon)pickUp);
				}
				else
				{
					weaponController.DeactivateWeapon((Weapon)pickUp);
					pickUp.PickUp(transform.Find("ActiveWeapon"));
					weaponInventory.Enqueue(pickUp);
				}

				return true;
			}
		}

		if (pickUp is Item)
		{ }

		return false;
	}

	private void EquipStartingWeapon()
	{
		/*
		 * Get an Instance of the player's starting weapon, and add it to the inventory.
		 */
		Weapon weapon = Instantiate(npc.GetStartingWeapon()).GetComponent<Weapon>();
		TryAddToInventory(weapon);
	}
}