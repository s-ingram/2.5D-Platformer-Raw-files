using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickupable
{
	GameObject gameObject { get; }

	void PickUp(Transform T);
	void Drop(Vector3 pos);
	IEnumerator ReactivateWAfterDelay();
}
