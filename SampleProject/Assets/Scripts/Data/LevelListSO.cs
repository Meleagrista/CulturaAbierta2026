using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelListSO", menuName = "Game/Level List")]
public class LevelListSO : SingletonScriptableObject<LevelListSO>
{
    public string titleScene;
    public string selectionScene;
    public List<LevelEntry> levels;
}

[Serializable]
public class LevelEntry
{
    public string sceneName;
    public GameObject previewPrefab;
}
