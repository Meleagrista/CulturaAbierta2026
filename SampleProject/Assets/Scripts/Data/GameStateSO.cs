using UnityEngine;

/// <summary>
/// State of the current run: which side the player picked and which difficulty tier they are on.
/// </summary>
[CreateAssetMenu(fileName = "GameStateSO", menuName = "Game/Game State")]
public class GameStateSO : SingletonScriptableObject<GameStateSO>
{
    [SerializeField] private Side side;
    [SerializeField] private int tier;

    public Side Side => side;
    public int Tier => tier;

    /// <summary>Starts a new run on the given side and returns the first level's scene.</summary>
    public string StartRun(Side newSide)
    {
        side = newSide;
        tier = 0;
        return LevelListSO.Instance.PickRandom(side, tier)?.sceneName;
    }

    /// <summary>Moves to the next tier and returns a random level from it, or null if the run is complete.</summary>
    public string AdvanceTier()
    {
        tier++;
        return LevelListSO.Instance.PickRandom(side, tier)?.sceneName;
    }

    public bool IsLastTier => tier >= LevelListSO.Instance.TierCount(side) - 1;
}
