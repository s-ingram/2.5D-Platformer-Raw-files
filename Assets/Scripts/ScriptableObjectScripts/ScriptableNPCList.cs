using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableNPCList", menuName = "ScriptableObjects/ScriptableNPCList")]
public class ScriptableNPCList : ScriptableObject
{
	[SerializeField]
	public List<GameObject> npcPrefabs;
}
