using UnityEngine;

[CreateAssetMenu(fileName = "GameStateSO", menuName = "Game/Game State")]
public class GameStateSO : SingletonScriptableObject<GameStateSO>
{
    [SerializeField] private int currentLevelIndex;
    [SerializeField] private float completionTime;
    [SerializeField] private int completionMoveCount;

    public int CurrentLevelIndex => currentLevelIndex;
    public float CompletionTime => completionTime;
    public int CompletionMoveCount => completionMoveCount;

    public void StartLevel(int index)
    {
        currentLevelIndex = index;
        completionTime = 0f;
        completionMoveCount = 0;
    }

    public void SaveCompletion(float time, int moveCount)
    {
        completionTime = time;
        completionMoveCount = moveCount;
    }

    public string GetCurrentSceneName() => LevelListSO.Instance.levels[currentLevelIndex].sceneName;

    public string GetNextSceneName()
    {
        int next = currentLevelIndex + 1;
        return next < LevelListSO.Instance.levels.Count ? LevelListSO.Instance.levels[next].sceneName : null;
    }
}
