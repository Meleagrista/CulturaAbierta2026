using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoseScreenController : UIScreen
{
    private Button m_retryButton;
    private Button m_homeButton;

    private void OnEnable()
    {
        m_retryButton = Root.Q<Button>("retry-button");
        m_homeButton = Root.Q<Button>("home-button");

        m_retryButton.clicked += OnRetryClicked;
        m_homeButton.clicked += OnHomeClicked;
    }

    private void OnDisable()
    {
        m_retryButton.clicked -= OnRetryClicked;
        m_homeButton.clicked -= OnHomeClicked;
    }

    private void Start()
    {
        Hide();
        EventManager.StartListening(Event.GAME_LOSE_EVENT, OnGameLose);
    }

    private void OnDestroy() => EventManager.StopListening(Event.GAME_LOSE_EVENT, OnGameLose);

    private void OnGameLose(Dictionary<string, object> message)
    {
        Show();
    }

    // Replays the same level (same tier, same scene).
    private void OnRetryClicked() => LoadScene(SceneManager.GetActiveScene().name);

    private void OnHomeClicked() => LoadScene(LevelListSO.Instance.titleScene);
}
