using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedKit : Item
{
	[SerializeField] float healValue;

	public override void ApplyPassiveEffect(PlayerCharacter player)
	{ }

	public override void ApplyImmediateEffect(PlayerCharacter player)
	{
		player.ModifyHealth(healValue);
	}

	public override void RemovePassiveEffect(PlayerCharacter player)
	{ }
}
