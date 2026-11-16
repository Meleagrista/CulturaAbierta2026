using System.Collections.Generic;
using UnityEngine.UIElements;

public class LoseScreenController : UIScreen
{
    private Button m_retryButton;
    private Button m_menuButton;

    private void OnEnable()
    {
        m_retryButton = Root.Q<Button>("retry-button");
        m_menuButton = Root.Q<Button>("menu-button");

        m_retryButton.clicked += OnRetryClicked;
        m_menuButton.clicked += OnMenuClicked;
    }

    private void OnDisable()
    {
        m_retryButton.clicked -= OnRetryClicked;
        m_menuButton.clicked -= OnMenuClicked;
    }

    private void Start()
    {
        Hide();
        EventManager.StartListening(Event.GAME_LOSE_EVENT, OnGameLose);
    }

    private void OnDestroy() => EventManager.StopListening(Event.GAME_LOSE_EVENT, OnGameLose);

    private void OnGameLose(Dictionary<string, object> message) => Show();

    private void OnRetryClicked() => LoadScene(GameStateSO.Instance.GetCurrentSceneName());
    private void OnMenuClicked() => LoadScene(LevelListSO.Instance.titleScene);
}
