using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
	GameObject gameObject { get; }

	void DoDamage(float damage, Weapon.DamageType damageType);

	IEnumerator Kill();
}
