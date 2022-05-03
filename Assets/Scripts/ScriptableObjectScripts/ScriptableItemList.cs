using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableItemList", menuName = "ScriptableObjects/ScriptableItemList")]
public class ScriptableItemList : ScriptableObject
{
	public float r_max;

	[SerializeField]
	public List<kvp> list = new List<kvp>();

	[Serializable]
	public struct kvp
	{
		public float weight;
		public GameObject prefab;
	}

	public GameObject GetWeightedItem()
	{
		float r = UnityEngine.Random.Range(0f, r_max);

		foreach (kvp pair in list)
		{
			r -= pair.weight;

			if (r <= 0)
				return pair.prefab;
		}

		return null;
	}
}
