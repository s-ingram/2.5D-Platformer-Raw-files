using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class PlayerCharacter : MonoBehaviour, IDamagable
{
	[SerializeField]
	float currentHp, currentMaxHp, currentShield, currentMaxShield, currentVisibility;
	[SerializeField]
	float kineticResistance, thermalResistance, emResistance, explosiveResistance;
	bool recharge = true;

	HealthBar healthBarUI;
	ShieldBar shieldBarUI;
	AmmoCounter ammoCounterUI;

	[SerializeField]
	public ScriptableCharacter scriptableCharacter;

	public event EventHandler<OnHPandShieldChangeArgs> OnHPandShieldChange;

	public class OnHPandShieldChangeArgs : EventArgs
	{
		public float damage = -1f;
		public float currentHp = -1f;
		public float maxHp = -1f;
		public float currentShield = -1f;
		public float maxShield = -1f;
		public Weapon.DamageType damageType = Weapon.DamageType.NULL;
	}

	void Awake()
	{
		DontDestroyOnLoad(this);

		currentHp = scriptableCharacter.baseMaxHp;
		currentMaxHp = scriptableCharacter.baseMaxHp;
		currentShield = scriptableCharacter.baseMaxShield;
		currentMaxShield = scriptableCharacter.baseMaxShield;
		currentVisibility = scriptableCharacter.baseVisibility;

		kineticResistance = scriptableCharacter.armourKineticResistance;
		thermalResistance = scriptableCharacter.armourThermalResistance;
		emResistance = scriptableCharacter.armourEMResistance;
		explosiveResistance = scriptableCharacter.armourExplosiveResistance;

		healthBarUI = GameManager.Instance.healthBarUI;
		shieldBarUI = GameManager.Instance.shieldBarUI;
		ammoCounterUI = GameManager.Instance.ammoCounterUI;

		gameObject.GetComponent<PlayerWeaponController>().OnAmmoUpdate += ammoCounterUI.SetAmmo;
		OnHPandShieldChange += healthBarUI.SetHealth;
		OnHPandShieldChange += shieldBarUI.SetShield;
	}

	void Start()
	{
		InvokeRepeating("ManageVisibility", 0f, 0.1f);
		InvokeRepeating("RechargeShields", 0f, 0.1f);
	}

	#region Getter/Setters
	#region Getters
	public float GetMoveSpeed()
	{ return scriptableCharacter.moveSpeed; }

	public float GetDashSpeed()
	{ return scriptableCharacter.dashSpeed; }

	public float GetDashLockTime()
	{ return scriptableCharacter.dashLockTime; }

	public float GetDashCooldownTime()
	{ return scriptableCharacter.dashCooldownTime; }

	public float GetSneakSpeed()
	{ return scriptableCharacter.sneakSpeed; }

	public float GetBaseVisibility()
	{ return scriptableCharacter.baseVisibility; }

	public float GetCurrentVisibility()
	{ return currentVisibility; }

	public int GetMaxWeapons()
	{ return scriptableCharacter.maxWeapons; }

	public int GetMaxItems()
	{ return scriptableCharacter.maxItems; }

	public GameObject GetStartingWeapon()
	{ return scriptableCharacter.startingWeapon; }
	#endregion
	#region Setters
	public void SetCurrentVisibility(float visibility)
	{ currentVisibility = visibility; }

	public void ResetVisibility()
	{ currentVisibility = GetBaseVisibility(); }

	public void ModifyHealth(float value)
	{
		currentHp += value;
		currentHp = Mathf.Clamp(currentHp, 0f, currentMaxHp);

		if (OnHPandShieldChange != null) OnHPandShieldChange(this, new OnHPandShieldChangeArgs { currentHp = currentHp });
	}

	public void ModifyResistances(float value)
	{
		kineticResistance += value;
		thermalResistance += value;
		emResistance += value;
		explosiveResistance += value;
	}
	#endregion
	#endregion

	public void ManageVisibility()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, GetCurrentVisibility(), 1 << LayerMask.NameToLayer("NPC"));

		foreach (Collider2D collider in colliders)
		{
			gameObject.GetComponent<Collider2D>().enabled = false;
			RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, collider.transform.position);
			gameObject.GetComponent<Collider2D>().enabled = true;

			RaycastHit2D hit = Array.Find(hits, x => x.collider.CompareTag("Walls"));

			if(hit == false)
				//if (hit.collider == null)
					collider.GetComponent<NPCCharacter>().SetTarget(this, new NPCDetectionController.OnTargetUpdatedEventArgs { target = this, position = transform.position });
		}
	}

	private void RechargeShields()
	{
		if(recharge)
		{
			currentShield += 1f;
			currentShield = Mathf.Clamp(currentShield, 0f, currentMaxShield);

			if (OnHPandShieldChange != null) OnHPandShieldChange(this, new OnHPandShieldChangeArgs { currentShield = currentShield, maxShield = currentMaxShield });
		}
	}
	
	private IEnumerator DisableShields()
	{
		recharge = false;
		yield return new WaitForSeconds(3f);
		recharge = true;
	}

	public void UpdateHPandShieldUI()
	{
		if (OnHPandShieldChange != null) OnHPandShieldChange(this, new OnHPandShieldChangeArgs { currentHp = currentHp });
	}

	//	IDamagable Methods.

	public void DoDamage(float damage, Weapon.DamageType damageType)
	{
		/*
		 * WIP for taking damage. Don't yet know how we want armor/resistance/damage to work. This is just an example.
		 */
		
		float resistance = 0f;

		switch(damageType)
		{
			case Weapon.DamageType.KINETIC:
				resistance = kineticResistance / 100;
				break;
			case Weapon.DamageType.THERMAL:
				resistance = thermalResistance / 100;
				break;
			case Weapon.DamageType.EM:
				resistance = emResistance / 100;
				break;
			case Weapon.DamageType.EXPLOSIVE:
				resistance = explosiveResistance / 100;
				break;
		}

		//	Example for 30% armour resistance
		//	1000 -=	100 * (1 - 0.3)
		//	1000 -= 100 * 0.7
		//	1000 -= 70

		if (currentShield > 0f)
		{
			float prev = currentShield;
			currentShield -= damage * (1 - resistance);
			currentShield = Mathf.Clamp(currentShield, 0f, currentMaxShield);
			damage -= prev;
			damage = Mathf.Clamp(damage, 0f, damage);
		}

		currentHp -= damage * (1 - resistance);
		currentHp = Mathf.Clamp(currentHp, 0f, currentMaxHp);

		if (OnHPandShieldChange != null) OnHPandShieldChange(this, new OnHPandShieldChangeArgs { currentHp = currentHp, currentShield = currentShield, damage = damage });
		if (currentHp <= 0f) StartCoroutine(Kill());

		StartCoroutine(DisableShields());
	}

	public IEnumerator Kill()
	{
		GameManager.KillCharacter(gameObject);
		yield return null;
	}

}
