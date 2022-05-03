using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utilities;

public class Weapon : MonoBehaviour, IPickupable
{
	[SerializeField] ScriptableWeapon scriptableWeapon;
	Projectile projectile;

	[SerializeField]
	public int currentMagAmount, currentSpareAmmo, sortingOrderBase, maxMagAmount, maxSpareAmmo;

	[SerializeField]
	float currentDeviation;

	[SerializeField]
	bool isReadyToFire, isFiring, isCycling, isTriggerReleased, isReloading, isEmpty, isOutOfAmmo;

	ParticleSystem particleSystem;
	LineRenderer lineRenderer;
	List<ParticleCollisionEvent> collsionEvents;
	Transform projectileSpawn;
	Animator animator;
	UnityEngine.Rendering.Universal.Light2D muzzleFlash;

	public enum WeaponType { PISTOL, RIFLE, SMG, SHOTGUN, LAUNCHER }
	public enum ProjectileType { MELEE, PROJECTILE, BEAM }
	public enum FireType { SINGLE, AUTO, BURST }
	public enum DamageType { KINETIC, THERMAL, EM, EXPLOSIVE, NULL }
	public enum Rarity { COMMON, UNCOMMON, RARE, VERYRARE, LEGENDARY }

	public event EventHandler<OnAmmoUpdateArgs> OnWeaponFire, OnWeaponReload, OnWeaponActivate;

	public class OnAmmoUpdateArgs : EventArgs
	{
		public int currentAmmo = -1;
		public int currentSpareAmmo = -1;
		public float reloadTime;
	}

	private void OnParticleCollision(GameObject other)
	{
		particleSystem.GetCollisionEvents(other, collsionEvents);

		foreach (ParticleCollisionEvent col in collsionEvents)
			if (col.colliderComponent != null)
			{
				if (col.colliderComponent.gameObject.layer != transform.parent.gameObject.layer && col.colliderComponent.GetComponent<IDamagable>() != null)
					col.colliderComponent.GetComponent<IDamagable>().DoDamage(scriptableWeapon.damage, scriptableWeapon.damageType);

				PlayHitEffect(col.intersection);
			}
	}

	void Awake()
	{
		projectileSpawn = transform.Find("ProjectileSpawnPosition");

		ParticleSystemSetup();
		LineRendererSetup();

		gameObject.GetComponentInChildren<SpriteRenderer>().sprite = scriptableWeapon.weaponSprite;
		gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = 10000;
		animator = gameObject.GetComponentInChildren<Animator>();
		muzzleFlash = gameObject.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
		muzzleFlash.enabled = false;

		maxMagAmount = scriptableWeapon.maxMagAmount;
		maxSpareAmmo = scriptableWeapon.maxSpareAmmo;
		currentMagAmount = scriptableWeapon.maxMagAmount;
		currentSpareAmmo = scriptableWeapon.maxSpareAmmo;
		currentDeviation = scriptableWeapon.minDeviation;

		isReadyToFire = true;
		isTriggerReleased = true;

		InvokeRepeating("DecreaseDeviation", 0f, 0.1f);
	}

	private void ParticleSystemSetup()
	{
		collsionEvents = new List<ParticleCollisionEvent>();
		particleSystem = GetComponent<ParticleSystem>();
		ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();

		var main = particleSystem.main;
		var emission = particleSystem.emission;
		var shape = particleSystem.shape;
		var col = particleSystem.collision;
		var tex = particleSystem.textureSheetAnimation;
		var trail = particleSystem.trails;
		var cot = particleSystem.colorOverLifetime;

		main.loop = false;
		main.startLifetime = (scriptableWeapon.range + scriptableWeapon.falloff) / scriptableWeapon.velocity;
		main.startSpeed = scriptableWeapon.velocity;
		main.startSize3D = true;

		if (scriptableWeapon.npcWeapon)
		{
			main.startSizeX = 0.1f;
			main.startSizeY = 0.1f;
		}
		else
			main.startSizeX = scriptableWeapon.projectileSprite.rect.height / scriptableWeapon.projectileSprite.pixelsPerUnit;

		//main.startSizeY = scriptableWeapon.projectileSprite.rect.width / scriptableWeapon.projectileSprite.pixelsPerUnit;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		main.playOnAwake = false;
		emission.rateOverTime = 200;
		shape.enabled = true;
		shape.angle = scriptableWeapon.maxDeviation;
		shape.radius = 0.0001f;
		shape.rotation = new Vector3(0f, 90f, 0f);
		shape.scale = new Vector3(0f, 1f, 1f);
		col.type = ParticleSystemCollisionType.World;
		col.mode = ParticleSystemCollisionMode.Collision2D;
		col.dampen = scriptableWeapon.ricochetteCount;
		col.lifetimeLoss = 1f;
		col.collidesWith = (1 << LayerMask.NameToLayer("Walls") | 1 << LayerMask.NameToLayer("NPC") | 1 << LayerMask.NameToLayer("Player"));
		//col.collidesWith = 1 << LayerMask.NameToLayer("NPC");
		//col.collidesWith = 1 << LayerMask.NameToLayer("Player");
		col.sendCollisionMessages = true;
		col.enabled = true;
		col.voxelSize = 0.1f;
		col.radiusScale = 0.1f;
		tex.mode = ParticleSystemAnimationMode.Sprites;
		tex.SetSprite(0, scriptableWeapon.projectileSprite);
		tex.enabled = true;
		trail.minVertexDistance = 0.001f;
		trail.dieWithParticles = false;
		trail.widthOverTrail = .1f;
		Gradient gradient = new Gradient();
		GradientAlphaKey[] gradientAlphaKey = new GradientAlphaKey[2];
		GradientColorKey[] gradientColorKey = new GradientColorKey[2];
		gradientAlphaKey[0] = new GradientAlphaKey(0.10f, 0f);
		gradientColorKey[0].color = Color.white;
		gradientAlphaKey[1] = new GradientAlphaKey(0f, 1f);
		gradientColorKey[1].color = Color.white;
		gradient.SetKeys(gradientColorKey, gradientAlphaKey);
		trail.colorOverLifetime = gradient;
		trail.enabled = scriptableWeapon.bulletTrails;

		if(scriptableWeapon.npcWeapon)
			renderer.renderMode = ParticleSystemRenderMode.Billboard;
		else
			renderer.renderMode = ParticleSystemRenderMode.Stretch;
		renderer.lengthScale = 2f;
		renderer.material = scriptableWeapon.particleMaterial;
		renderer.trailMaterial = scriptableWeapon.material;
		renderer.flip = new Vector3(1f, 0f, 0f);
		renderer.sortingOrder = 10000;

		gradient = new Gradient();
		gradientAlphaKey = new GradientAlphaKey[3];
		gradientColorKey = new GradientColorKey[3];
		gradientAlphaKey[0] = new GradientAlphaKey(1f, 0f);
		gradientColorKey[0].color = Color.white;
		gradientAlphaKey[1] = new GradientAlphaKey(1f, scriptableWeapon.range / (scriptableWeapon.range + scriptableWeapon.falloff));
		gradientColorKey[1].color = Color.white;
		gradientAlphaKey[2] = new GradientAlphaKey(0f, 1f);
		gradientColorKey[2].color = Color.white;
		gradient.SetKeys(gradientColorKey, gradientAlphaKey);

		cot.color = gradient;
		cot.enabled = true;

		particleSystem.transform.SetParent(projectileSpawn);
	}

	private void LineRendererSetup()
	{
		lineRenderer = gameObject.GetComponent<LineRenderer>();
	}

	void Update()
	{
		isEmpty = currentMagAmount <= 0;
		isOutOfAmmo = currentSpareAmmo <= 0;

		isReadyToFire = isCycling == false && isReloading == false && isEmpty == false;

		if (scriptableWeapon.fireType == Weapon.FireType.SINGLE)
			isReadyToFire = isReadyToFire && isTriggerReleased == true;

		if (scriptableWeapon.projectileType == ProjectileType.BEAM)
		{
			if (isEmpty)
				lineRenderer.enabled = false;
			else if (!isTriggerReleased)
			{
				lineRenderer.SetPosition(0, projectileSpawn.transform.position);

				LayerMask layers = (1 << LayerMask.NameToLayer("Walls") | 1 << LayerMask.NameToLayer("NPC") | 1 << LayerMask.NameToLayer("Player"));
				RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, scriptableWeapon.range, layers);

				if (hit.collider)
				{
					lineRenderer.SetPosition(1, hit.point);
					PlayHitEffect(hit.point);
				}
				else
					lineRenderer.SetPosition(1, projectileSpawn.transform.position + transform.right * scriptableWeapon.range);
			}
		}
	}

	public void FireWeapon(object sender, EventArgs e)
	{
		/*
		 * This function is to fire this weapon. It is called by the OnLeftClick event.
		 * If this weapon is ready to fire, create a projectile, decrement the current magazine amount, increase the current deviation, and start a refire cooldown.
		 */

		if (isCycling)
			return;

		if (isReadyToFire)
		{
			PlayFireSound();
			PlayFireAnimation();

			isTriggerReleased = false;

			if (scriptableWeapon.projectileType == ProjectileType.PROJECTILE)
			{
				//Projectile projectile = CreateProjectile(projectileSpawn.position, currentDeviation);

				for(int i = 0; i < scriptableWeapon.projectileCount; i++)
					ShootAParticle();

				StartCoroutine(PlayMuzzleFlash());

				currentMagAmount--;
				currentMagAmount = Mathf.Clamp(currentMagAmount, 0, scriptableWeapon.maxMagAmount);

				currentDeviation += scriptableWeapon.deviationIncreaseRate;
				currentDeviation = Mathf.Clamp(currentDeviation, scriptableWeapon.minDeviation, scriptableWeapon.maxDeviation);
			}
			else if (scriptableWeapon.projectileType == ProjectileType.BEAM)
			{
				ShootABeam();
				muzzleFlash.enabled = true;

				currentMagAmount--;
				currentMagAmount = Mathf.Clamp(currentMagAmount, 0, scriptableWeapon.maxMagAmount);
			}

			if (OnWeaponFire != null) OnWeaponFire(this, new OnAmmoUpdateArgs { currentAmmo = currentMagAmount, currentSpareAmmo = currentSpareAmmo, reloadTime = scriptableWeapon.reloadTime });
			// pushback in player movement controller
			// subscribe function to onweaponfire

			StartCoroutine(RefireCooldown());
		}
		else if (isEmpty)
		{
			isTriggerReleased = false;
			PlayEmptySound();
			StartCoroutine(RefireCooldown());
		}
	}

	private void ShootAParticle()
	{
		var shape = particleSystem.shape;
		shape.angle = currentDeviation;

		particleSystem.Emit(1);
	}

	private void ShootABeam()
	{
		lineRenderer.enabled = true;

		lineRenderer.SetPosition(0, projectileSpawn.transform.position);

		LayerMask layers = (1 << LayerMask.NameToLayer("Walls") | 1 << LayerMask.NameToLayer("NPC") | 1 << LayerMask.NameToLayer("Player"));
		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, scriptableWeapon.range, layers);

		if (hit.collider)
		{
			lineRenderer.SetPosition(1, hit.point);

			if (hit.collider.GetComponent<IDamagable>() != null)
				hit.collider.GetComponent<IDamagable>().DoDamage(scriptableWeapon.damage, scriptableWeapon.damageType);

		}
		else
			lineRenderer.SetPosition(1, projectileSpawn.transform.position + transform.right * scriptableWeapon.range);
	}

	public void Reload(object sender, EventArgs e)
	{
		/*
		 * This function is to trigger a reload. It is called by the OnReloadButton event.
		 * If this weapon is not already reloading, and is not out of ammo, start a reload.
		 */

		if (!isReloading && !isOutOfAmmo && currentMagAmount != scriptableWeapon.maxMagAmount)
			StartCoroutine(DoReload());
	}

	public void ReleaseTrigger(object sender, EventArgs e)
	{
		/*
		 * This function is to release the trigger. It is primarily used for semi-automatic or single fire weapons.
		 * It is trigger by the OnLeftClickUp event.
		 */

		lineRenderer.enabled = false;
		if (scriptableWeapon.projectileType == ProjectileType.BEAM)
			muzzleFlash.enabled = false;

		isTriggerReleased = true;
	}

	private IEnumerator DoReload()
	{
		/*
		 * This function is to manage the reloading.
		 * Set the weapon to be reloading, wait an amount of time, add the current magazine amount to the amount of spare ammo, then determine how large the new magazine is, deduct that from the spare ammo, and stop reloading.
		 */

		isReloading = true;
		if (OnWeaponReload != null) OnWeaponReload(this, new OnAmmoUpdateArgs { reloadTime = scriptableWeapon.reloadTime });
		yield return new WaitForSeconds(scriptableWeapon.reloadTime);
		currentSpareAmmo += currentMagAmount;
		currentMagAmount = Mathf.Min(currentSpareAmmo, scriptableWeapon.maxMagAmount);
		currentSpareAmmo -= currentMagAmount;

		if (OnWeaponReload != null) OnWeaponReload(this, new OnAmmoUpdateArgs { currentAmmo = currentMagAmount, currentSpareAmmo = currentSpareAmmo });

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

	private Projectile CreateProjectile(Vector3 pos, float deviation)
	{
		/*
		 * This is a function to create a projectile. It is called in the FireWeapon function()
		 * Create a clone of the projectile prefab, at a given position and rotation.
		 * Set the stats of that projectile, ie range, damage, etc.
		 * Alter the rotation based on the current weapon deviation.
		 * Then give it speed.
		 */

		Projectile clone = Instantiate(projectile, pos, transform.rotation);

		Physics2D.IgnoreCollision(clone.GetComponent<Collider2D>(), gameObject.transform.parent.GetComponentInParent<Collider2D>());

		clone.GetComponent<Projectile>().velocity = scriptableWeapon.velocity;
		clone.GetComponent<Projectile>().range = scriptableWeapon.range;
		clone.GetComponent<Projectile>().damage = scriptableWeapon.damage;
		clone.GetComponent<Projectile>().damageType = scriptableWeapon.damageType;

		clone.transform.eulerAngles = new Vector3(0f, 0f, projectileSpawn.eulerAngles.z + UnityEngine.Random.Range(-deviation / 2, deviation / 2));
		clone.GetComponent<Rigidbody2D>().rotation += clone.transform.eulerAngles.z;
		clone.GetComponent<Rigidbody2D>().velocity = clone.transform.right * scriptableWeapon.velocity;

		return clone;
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

	private void PlayFireSound()
	{
		// play fire sound
		gameObject.GetComponent<AudioSource>().PlayOneShot(scriptableWeapon.fireSound[UnityEngine.Random.Range(0, scriptableWeapon.fireSound.Count)]);
	}

	private void PlayEmptySound()
	{
		// play fire sound
		gameObject.GetComponent<AudioSource>().PlayOneShot(scriptableWeapon.emptySound[UnityEngine.Random.Range(0, scriptableWeapon.emptySound.Count)]);
	}

	private void PlayFireAnimation()
    {
		if (animator != null)
			animator.Play("Weapon_Fire");
    }

	private IEnumerator PlayMuzzleFlash()
	{
		muzzleFlash.enabled = true;
		yield return new WaitForSeconds(0.05f);
		muzzleFlash.enabled = false;
	}

	private void PlayHitEffect(Vector2 pos)
	{
		Instantiate(scriptableWeapon.hitEffect, pos, Quaternion.identity);
	}

	//	IPickupable Methods

	public void PickUp(Transform newParent)
	{
		/*
		 * This is an IPickupable interface function. It is called from the inventory manager when the item is to be picked up.
		 * Disable the renderer and collider, and set a new parent transform.
		 */

		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		gameObject.GetComponent<Collider2D>().enabled = false;
		gameObject.transform.position = newParent.transform.position;
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

	public IEnumerator ReactivateWAfterDelay()
	{
		yield return new WaitForSeconds(1.5f);
		GetComponent<Collider2D>().enabled = true;
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
