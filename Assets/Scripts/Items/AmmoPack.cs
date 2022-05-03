using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPack : Item
{
	[SerializeField] int ammoValue;

	public override void ApplyPassiveEffect(PlayerCharacter player)
	{ }

	public override void ApplyImmediateEffect(PlayerCharacter player)
	{
		player.GetComponent<PlayerWeaponController>().AddAmmo(ammoValue);
	}

	public override void RemovePassiveEffect(PlayerCharacter player)
	{ }
}
