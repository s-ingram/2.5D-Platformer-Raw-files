using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour, IPickupable
{
	[SerializeField]
	protected ScriptableItem scriptableItem;

	public enum ItemType { ACTIVE, PASSIVE, CONSUMABLE }
	public enum Rarity { COMMON, UNCOMMON, RARE, VERYRARE, LEGENDARY }

	void Awake()
	{
		gameObject.GetComponentInChildren<SpriteRenderer>().sprite = scriptableItem.itemSprite;
	}

	public abstract void ApplyPassiveEffect(PlayerCharacter player);
	public abstract void ApplyImmediateEffect(PlayerCharacter player);
	public abstract void RemovePassiveEffect(PlayerCharacter player);

	//	IPickupable Methods

	public void PickUp(Transform newParent)
	{
		/*
		 * This is an IPickupable interface function. It is called from the inventory manager when the item is to be picked up.
		 * Disable the renderer and collider, and set a new parent transform.
		 */

		ApplyPassiveEffect(newParent.GetComponentInParent<PlayerCharacter>());
		ApplyImmediateEffect(newParent.GetComponentInParent<PlayerCharacter>());

		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		gameObject.GetComponent<Collider2D>().enabled = false;
		gameObject.transform.position = newParent.transform.position;
		gameObject.transform.SetParent(newParent);
	}

	public void Drop(Vector3 pos)
	{
		RemovePassiveEffect(transform.parent.GetComponentInParent<PlayerCharacter>());

		StartCoroutine(ReactivateWAfterDelay());
	}

	public IEnumerator ReactivateWAfterDelay()
	{
		yield return new WaitForSeconds(1.5f);
		GetComponentInChildren<Collider2D>().enabled = true;
	}
}
