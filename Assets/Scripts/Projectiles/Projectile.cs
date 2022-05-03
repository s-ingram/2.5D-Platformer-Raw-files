using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	private ScriptableProjectile scriptableProjectile;

	[HideInInspector]
	public float damage, velocity, range;
	[HideInInspector]
	public Weapon.DamageType damageType;

	 void Start()
	{
		//	Destroy this object after a certain time.
		//	Range / Velocity = Lifetime.
		Destroy(gameObject, range / velocity);
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		/*
		 * This is a Unity function. It is called when a trigger enters the collider.
		 * If the collider has the "Shootable" tag, Hit() the collider, then destroy the object.
		 */

		if (collider.CompareTag("Walls") || collider.CompareTag("Player") || collider.CompareTag("NPC"))
		{
			Hit(collider);
			Destroy(gameObject);
		}
	}

	private void Hit(Collider2D collider)
	{
		/*
		 * This function is called when a projectile hits something.
		 */

		PlayHitEffect();

		if (collider.GetComponent<IDamagable>() != null)
		{
			collider.GetComponent<IDamagable>().DoDamage(damage, damageType);

			Debug.Log(collider.transform.name);
		}
	}

	private void PlayHitEffect()
	{
		/*
		 * Play an effect.
		 */

		GameObject fx = new GameObject("HitEffect", typeof(SpriteRenderer));
		fx.GetComponent<SpriteRenderer>().material = scriptableProjectile.material;
		fx.GetComponent<SpriteRenderer>().sprite = scriptableProjectile.hitEffects[Random.Range(0, 4)];
		fx.GetComponent<SpriteRenderer>().sortingOrder = 10000;
		fx.transform.position = transform.position;
		fx.transform.rotation = Utils.GetRandomVector2Direction();
		
		Destroy(fx, 0.03f);
	}
}
