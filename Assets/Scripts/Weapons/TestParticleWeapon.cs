using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class TestParticleWeapon : MonoBehaviour, IPickupable
{
	[SerializeField]
	private ScriptableWeapon scriptableWeapon;

	//[SerializeField]
	//private Projectile projectile;

	ParticleSystem particleSystem;
	List<ParticleCollisionEvent> collsionEvents;

	[SerializeField]
	public int currentMagAmount, currentSpareAmmo, sortingOrderBase;

	[SerializeField]
	private float currentDeviation;

	[SerializeField]
	private bool isReadyToFire, isCycling, isTriggerReleased, isReloading, isEmpty, isOutOfAmmo;

	private Transform projectileSpawn;

	public enum WeaponType { PISTOL, RIFLE, SMG, SHOTGUN, LAUNCHER }
	public enum ProjectileType { MELEE, PROJECTILE, BEAM }
	public enum FireType { SINGLE, AUTO, BURST }
	public enum DamageType { KINETIC, THERMAL, EM, EXPLOSIVE, NULL }
	public enum Rarity { COMMON, UNCOMMON, RARE, VERYRARE, LEGENDARY }

	public event EventHandler<OnAmmoUpdateArgs> OnWeaponFire, OnWeaponReload, OnWeaponActivate;

	public class OnAmmoUpdateArgs : EventArgs
	{
		public int currentAmmo;
		public int currentSpareAmmo;
		public float reloadTime;
	}

	private void OnParticleCollision(GameObject other)
	{
		int numCollisionEvents = particleSystem.GetCollisionEvents(other, collsionEvents);

		foreach (ParticleCollisionEvent col in collsionEvents)
			if (col.colliderComponent != null)
				Debug.Log("Hit: " + col.colliderComponent.gameObject.name);
	}

	void Awake()
	{
		projectileSpawn = gameObject.transform.Find("ProjectileSpawnPosition");
		particleSystem = gameObject.GetComponent<ParticleSystem>();
		collsionEvents = new List<ParticleCollisionEvent>();

		gameObject.GetComponentInChildren<SpriteRenderer>().sprite = scriptableWeapon.weaponSprite;
		gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = 10000;

		currentMagAmount = scriptableWeapon.maxMagAmount;
		currentSpareAmmo = scriptableWeapon.maxSpareAmmo;
		currentDeviation = scriptableWeapon.minDeviation;

		isReadyToFire = true;
		isTriggerReleased = true;

		InvokeRepeating("DecreaseDeviation", 0f, 0.1f);
	}

	void Update()
	{
		isEmpty = currentMagAmount <= 0;
		isOutOfAmmo = currentSpareAmmo <= 0;

		isReadyToFire = isCycling == false && isReloading == false && isEmpty == false;

		if (scriptableWeapon.fireType == Weapon.FireType.SINGLE)
			isReadyToFire = isReadyToFire && isTriggerReleased == true;
	}

	public void FireWeapon(object sender, EventArgs e)
	{
		/*
		 * This function is to fire this weapon. It is called by the OnLeftClick event.
		 * If this weapon is ready to fire, create a projectile, decrement the current magazine amount, increase the current deviation, and start a refire cooldown.
		 */

		if (isReadyToFire)
		{
			// play fire sound
			gameObject.GetComponent<AudioSource>().PlayOneShot(scriptableWeapon.fireSound[UnityEngine.Random.Range(0, scriptableWeapon.fireSound.Count)]);

			isTriggerReleased = false;
			//Projectile projectile = CreateProjectile(projectileSpawn.position, currentDeviation);

			//ShootAParticle();
			ShootAParticle();

			currentMagAmount--;
			currentMagAmount = Mathf.Clamp(currentMagAmount, 0, scriptableWeapon.maxMagAmount);

			if (OnWeaponFire != null) OnWeaponFire(this, new OnAmmoUpdateArgs { currentAmmo = currentMagAmount, currentSpareAmmo = currentSpareAmmo, reloadTime = scriptableWeapon.reloadTime });

			currentDeviation += scriptableWeapon.deviationIncreaseRate;
			currentDeviation = Mathf.Clamp(currentDeviation, scriptableWeapon.minDeviation, scriptableWeapon.maxDeviation);

			StartCoroutine(RefireCooldown());
		}
	}

	private void ShootAParticle()
	{
		var main = particleSystem.main;
		var shape = particleSystem.shape;

		main.startLifetime = scriptableWeapon.range / scriptableWeapon.velocity;
		main.startSpeed = scriptableWeapon.velocity;

		var angle = currentDeviation;  //	change to current Deviation
		shape.angle = angle;

		particleSystem.Emit(1);
	}

	public void Reload(object sender, EventArgs e)
	{
		/*
		 * This function is to trigger a reload. It is called by the OnReloadButton event.
		 * If this weapon is not already reloading, and is not out of ammo, start a reload.
		 */

		if (!isReloading && !isOutOfAmmo)
			StartCoroutine(DoReload());
	}

	public void ReleaseTrigger(object sender, EventArgs e)
	{
		/*
		 * This function is to release the trigger. It is primarily used for semi-automatic or single fire weapons.
		 * It is trigger by the OnLeftClickUp event.
		 */

		isTriggerReleased = true;
	}

	private IEnumerator DoReload()
	{
		/*
		 * This function is to manage the reloading.
		 * Set the weapon to be reloading, wait an amount of time, add the current magazine amount to the amount of spare ammo, then determine how large the new magazine is, deduct that from the spare ammo, and stop reloading.
		 */

		isReloading = true;
		yield return new WaitForSeconds(scriptableWeapon.reloadTime);
		currentSpareAmmo += currentMagAmount;
		currentMagAmount = Mathf.Min(currentSpareAmmo, scriptableWeapon.maxMagAmount);
		currentSpareAmmo -= currentMagAmount;

		if (OnWeaponReload != null) OnWeaponReload(this, new OnAmmoUpdateArgs { currentAmmo = currentMagAmount, currentSpareAmmo = currentSpareAmmo, reloadTime = scriptableWeapon.reloadTime });

		isReloading = false;
	}

	private IEnumerator RefireCooldown()
	{
		/*
		 * This is a cooldown to prevent the player firing again too quickly.
		 */

		isCycling = true;
		yield return new WaitForSeconds(scriptableWeapon.refireDelay);
		isCycling = false;
	}

	public void DecreaseDeviation()
	{
		/*
		 * This function is to periodically reduce the current deviation down to the minimum. It is invoked in Awake(), and is called every one-tenth second.
		 * If the current deviation is not already at the minimum, reduce it by an amount. Clamp the min and max.
		 */

		if (currentDeviation > scriptableWeapon.minDeviation)
			currentDeviation -= scriptableWeapon.deviationDecreaseRate;

		currentDeviation = Mathf.Clamp(currentDeviation, scriptableWeapon.minDeviation, scriptableWeapon.maxDeviation);
	}

	public IEnumerator ReactivateWAfterDelay()
	{
		yield return new WaitForSeconds(1.5f);
		GetComponent<Collider2D>().enabled = true;
	}

	//	Interface Methods

	public void PickUp(Transform newParent)
	{
		/*
		 * This is an IPickupable interface function. It is called from the inventory manager when the item is to be picked up.
		 * Disable the renderer and collider, and set a new parent transform.
		 */

		Debug.Log("PickUp");
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		gameObject.GetComponent<Collider2D>().enabled = false;
		gameObject.transform.SetParent(newParent);
	}

	public void Drop(Vector3 pos)
	{
		/*
		 * This is an IPickupable interface function. It is called from the inventory manage when an item is dropped.
		 * Reset the parent so that it is independant, give it a random rotation, enable the Renderer, wait some time, then enable the collider.
		 * The waiting is so that the player does not immediately pick the item back up.
		 */

		Debug.Log("Drop");
		gameObject.transform.SetParent(null);
		gameObject.transform.rotation = Utils.GetRandomVector2Direction();
		gameObject.GetComponent<SpriteRenderer>().enabled = true;
		gameObject.GetComponent<SpriteRenderer>().sortingOrder = (int)(sortingOrderBase - transform.position.y);


		StartCoroutine(ReactivateWAfterDelay());
	}

	//	Getters/Setters

	public bool IsEmpty()
	{ return isEmpty; }

	public bool IsReloading()
	{ return isReloading; }

	public float GetReloadTime()
	{ return scriptableWeapon.reloadTime; }

	public float GetWeaponMaxRange()
	{ return scriptableWeapon.range; }
}


/*



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ParticleSystemWeaponTest : MonoBehaviour
{
	ParticleSystem ps;
	List<ParticleCollisionEvent> collsionEvents;

	[SerializeField]
	ScriptableWeapon scriptableWeapon;
	
	// Start is called before the first frame update
    void Awake()
    {
		ps = gameObject.GetComponent<ParticleSystem>();
		collsionEvents = new List<ParticleCollisionEvent>();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.F))
			shoot();
    }

	private void shoot()
	{
		var main = ps.main;
		var shape = ps.shape;

		main.startLifetime = scriptableWeapon.range / scriptableWeapon.velocity;
		main.startSpeed = scriptableWeapon.velocity;

		var angle = scriptableWeapon.maxDeviation;	//	change to current Deviation
		shape.angle = angle;

		ps.Emit(1);
	}

	private void OnParticleCollision(GameObject other)
	{
		int numCollisionEvents = ps.GetCollisionEvents(other, collsionEvents);

		foreach(ParticleCollisionEvent col in collsionEvents)
		{
			if(col.colliderComponent != null)
			{
				Debug.Log("Hit: " + col.colliderComponent.gameObject.name);
			}
		}


		/*
		Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
		int i = 0;

		while (i < numCollisionEvents)
		{
			if(rb)
			{
				Debug.Log("Hit: " + rb.gameObject.name);
			}
			i++;
		}
	}
}
*/