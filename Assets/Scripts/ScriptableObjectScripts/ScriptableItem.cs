using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableItem", menuName = "ScriptableObjects/ScriptableItem")]
public class ScriptableItem : ScriptableObject
{
	public string name;
	public string description;

	public float maxDuration;
	public float maxCooldown;

	public int maxStacks;

	public Item.ItemType itemType;
	public Item.Rarity rarity;

	public Sprite itemSprite;
}
