using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableCharacter", menuName = "ScriptableObjects/ScriptableCharacter")]
public class ScriptableCharacter : ScriptableObject
{
	public string characterName;
	public string description;

	//	Remember, these are only the base values.
	public float baseMaxHp;
	//public float hpKineticResistance;
	//public float hpThermalResistance;
	//public float hpEMResistance;
	//public float hpExplosiveResistance;

	//	Refer to PlayerCharacter.DoDamage().
	public float baseMaxArmour;
	public float armourKineticResistance;
	public float armourThermalResistance;
	public float armourEMResistance;
	public float armourExplosiveResistance;

	public float baseMaxShield;
	//public float shieldKineticResistance;
	//public float shieldThermalResistance;
	//public float shieldEMResistance;
	//public float shieldExplosiveResistance;

	public float baseVisibility;

	public float moveSpeed;
	public float dashSpeed;
	public float dashCooldownTime;
	public float dashLockTime;
	public float sneakSpeed;

	public float critChance;
	public float dodgeChance;

	public int maxWeapons;
	public int maxItems;

	public Sprite sprite;
	public GameObject startingWeapon;

	// UI elements
	public GameObject healthBar, shieldBar;
	public GameObject ammoCounter, spareAmmoCounter;

	//	This stuff is primarily for NPC's.
	public bool isTargetDummy;
	public float loseSightDistance;

	public GameObject[] droppableGameObjects;

	public int LOSFailThreshold;
}
