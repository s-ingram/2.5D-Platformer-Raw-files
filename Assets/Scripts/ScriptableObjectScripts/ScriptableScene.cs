using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableScene", menuName = "ScriptableObjects/ScriptableScene")]
public class ScriptableScene : ScriptableObject
{
	[Serializable]
	public struct LevelParams
	{
		public ScriptableRoomGenParams roomGenParams;
		public ScriptableNPCList roomNPCList;
		public ScriptableItemList roomItemList;
	}

	[SerializeField]
	public List<LevelParams> levelParams;
}
