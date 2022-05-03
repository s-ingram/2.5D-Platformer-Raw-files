using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptablePlaylist", menuName = "ScriptableObjects/ScriptablePlaylist")]

public class ScriptablePlaylist : ScriptableObject
{
    public List<AudioClip> tracks;
}
