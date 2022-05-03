using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCCharacter : MonoBehaviour, IDamagable
{
	[SerializeField] public ScriptableCharacter scriptableCharacter;

	[SerializeField] private int dropChance = 5;	// chance of 5 means 1 in 5 chance of dropping
	[SerializeField] private float currentHp, currentMaxHp, currentShield, currentMaxShield;
	[SerializeField] public Vector3 targetLastKnownPos;

	[SerializeField] public bool isTracking, isAttacking, hasTarget, hasLOS, hasDied = false;
	PlayerCharacter target;

	void Awake()
	{
		currentHp = scriptableCharacter.baseMaxHp;
		currentShield = scriptableCharacter.baseMaxShield;

		gameObject.GetComponent<NPCDetectionController>().OnTargetLost += SetTarget;
		gameObject.GetComponent<NPCDetectionController>().OnTrackTarget += UpdateLastKnownPosition;
	}

	#region Getters/Setters
	public float GetMoveSpeed()
	{ return scriptableCharacter.moveSpeed; }

	public float GetLoseSightDistance()
	{ return scriptableCharacter.loseSightDistance; }

	public int GetLOSFailThreshold()
	{ return scriptableCharacter.LOSFailThreshold; }

	public int GetMaxWeapons()
	{ return scriptableCharacter.maxWeapons; }

	public int GetMaxItems()
	{ return scriptableCharacter.maxItems; }

	public GameObject GetStartingWeapon()
	{ return scriptableCharacter.startingWeapon; }

	public PlayerCharacter GetCurrentTarget()
	{ return target; }

	public bool IsTargetDummy()
	{ return scriptableCharacter.isTargetDummy; }
	#endregion

	public void SetTarget(object sender, NPCDetectionController.OnTargetUpdatedEventArgs args)
	{
		hasTarget = args.target != null;

		if (hasTarget)
		{
			target = args.target;
			targetLastKnownPos = args.position;
		}
		else
			target = null;
		
	}

	//	Event methods.

	public void UpdateLastKnownPosition(object sender, NPCDetectionController.OnTargetUpdatedEventArgs args)
	{
		targetLastKnownPos = args.position;
	}

	//	Interface methods.

	public void DoDamage(float damage, Weapon.DamageType damageType)
	{
		float resistance = 0f;

		switch (damageType)
		{
			case Weapon.DamageType.KINETIC:
				resistance = scriptableCharacter.armourKineticResistance / 100;
				break;
			case Weapon.DamageType.THERMAL:
				resistance = scriptableCharacter.armourThermalResistance / 100;
				break;
			case Weapon.DamageType.EM:
				resistance = scriptableCharacter.armourEMResistance / 100;
				break;
			case Weapon.DamageType.EXPLOSIVE:
				resistance = scriptableCharacter.armourExplosiveResistance / 100;
				break;
		}

		if (currentShield > 0f)
		{
			float prev = currentShield;
			currentShield -= damage * (1 - resistance);
			damage -= prev;
			damage = Mathf.Clamp(damage, 0f, damage);
		}

		currentHp -= damage * (1 - resistance);

		if (currentHp <= 0f) StartCoroutine(Kill());
	}

	public IEnumerator Kill()
	{
		if (!hasDied)
		{
			hasDied = true;

			gameObject.GetComponent<NPCDetectionController>().OnTargetLost -= SetTarget;
			gameObject.GetComponent<NPCDetectionController>().OnTrackTarget -= UpdateLastKnownPosition;

			DropItem();

			gameObject.GetComponent<Collider2D>().enabled = false;
			gameObject.GetComponent<NPCWeaponController>().DeactivateWeapon(gameObject.GetComponent<NPCWeaponController>().GetActiveWeapon());
			//gameObject.GetComponent<NPCInventoryController>().DropActiveWeapon();
			gameObject.GetComponent<Dissolve>().MakeDissolve();
			yield return new WaitForSeconds(1f);
			GameManager.KillCharacter(gameObject);
		}
	}

	private void DropItem()
    {
		if (!IsTargetDummy())
        {
			int number = UnityEngine.Random.Range(0, dropChance);
			if (number == 1)
				Instantiate(scriptableCharacter.droppableGameObjects[UnityEngine.Random.Range(0, scriptableCharacter.droppableGameObjects.Length)], transform.position, transform.rotation);
		}
	}
}
