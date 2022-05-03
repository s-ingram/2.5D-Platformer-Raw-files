using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableProjectile", menuName = "ScriptableObjects/ScriptableProjectile")]
public class ScriptableProjectile : ScriptableObject
{
	public Sprite[] hitEffects;
	public Material material;
}
