using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ScriptableWeapon", menuName = "ScriptableObjects/ScriptableWeapon")]
public class ScriptableWeapon : ScriptableObject
{
	public string WeaponName;
	public string description;

	public float damage;
	public float refireDelay;	//	Previously ROF, refireDelay is the time (in seconds) between shots.
	public float reloadTime;
	public float minDeviation;
	public float maxDeviation;
	public float deviationIncreaseRate;
	public float deviationDecreaseRate;
	public float range;
	public float falloff;
	public float velocity;
	public float baseCost;
	public float critChance;
	public float pierceChance;

	public int projectileCount;
	public int maxMagAmount;
	public int maxSpareAmmo;
	public int ricochetteCount;

	public bool bulletTrails = false;
	public bool npcWeapon = false;

	public Weapon.WeaponType weaponType;
	public Weapon.ProjectileType projectileType;
	public Weapon.FireType fireType;
	public Weapon.DamageType damageType;
	public Weapon.Rarity rarity;

	public Sprite weaponSprite;
	public Sprite projectileSprite;
	public Material material;
	public Material particleMaterial;

	public List<AudioClip> fireSound;
	public List<AudioClip> emptySound;

	public GameObject hitEffect;
}
