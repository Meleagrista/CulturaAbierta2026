using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelListSO", menuName = "Game/Level List")]
public class LevelListSO : SingletonScriptableObject<LevelListSO>
{
    public string titleScene;
    public List<string> leftLevels;
    public List<string> rightLevels;
}