using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyArmor : Item
{
	[SerializeField] float armorIncreaseValue;
	[SerializeField] Weapon.DamageType armorDamageType;

	public override void ApplyPassiveEffect(PlayerCharacter player)
	{
		player.ModifyResistances(armorIncreaseValue);
	}

	public override void ApplyImmediateEffect(PlayerCharacter player)
	{ }

	public override void RemovePassiveEffect(PlayerCharacter player)
	{
		player.ModifyResistances(-armorIncreaseValue);
	}
}
