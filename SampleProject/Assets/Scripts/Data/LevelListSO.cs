using System;
using System.Collections.Generic;
using UnityEngine;

public enum Side
{
    Left,
    Right
}

/// <summary>One playable level: its scene and the money the player starts with.</summary>
[Serializable]
public class LevelEntry
{
    public string sceneName;
    public int startingMoney = 15;
}

/// <summary>A difficulty tier. One level is picked at random from it.</summary>
[Serializable]
public class LevelTier
{
    public string name;
    public List<LevelEntry> levels = new List<LevelEntry>();
}

[CreateAssetMenu(fileName = "LevelListSO", menuName = "Game/Level List")]
public class LevelListSO : SingletonScriptableObject<LevelListSO>
{
    public string titleScene;

    [Tooltip("Tiers in order of difficulty. Winning a level moves to a random level of the next tier.")]
    public List<LevelTier> leftTiers = new List<LevelTier>();
    public List<LevelTier> rightTiers = new List<LevelTier>();

    public List<LevelTier> GetTiers(Side side) => side == Side.Left ? leftTiers : rightTiers;

    public int TierCount(Side side) => GetTiers(side).Count;

    /// <summary>Random level from the given tier, or null if the tier doesn't exist or is empty.</summary>
    public LevelEntry PickRandom(Side side, int tier)
    {
        var tiers = GetTiers(side);
        if (tier < 0 || tier >= tiers.Count || tiers[tier].levels.Count == 0)
        {
            return null;
        }

        var levels = tiers[tier].levels;
        return levels[UnityEngine.Random.Range(0, levels.Count)];
    }

    /// <summary>Finds a level by scene name in any side/tier (used to read its starting money).</summary>
    public LevelEntry Find(string sceneName)
    {
        foreach (var tiers in new[] { leftTiers, rightTiers })
        {
            foreach (var tier in tiers)
            {
                foreach (var level in tier.levels)
                {
                    if (level.sceneName == sceneName)
                    {
                        return level;
                    }
                }
            }
        }
        return null;
    }
}
