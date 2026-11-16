using System.Collections.Generic;
using UnityEngine.UIElements;

public class WinScreenController : UIScreen
{
    private Label m_resultLabel;
    private Button m_nextButton;
    private Button m_retryButton;
    private Button m_menuButton;

    private void OnEnable()
    {
        m_resultLabel = Root.Q<Label>("result-label");
        m_nextButton = Root.Q<Button>("next-button");
        m_retryButton = Root.Q<Button>("retry-button");
        m_menuButton = Root.Q<Button>("menu-button");

        m_nextButton.clicked += OnNextClicked;
        m_retryButton.clicked += OnRetryClicked;
        m_menuButton.clicked += OnMenuClicked;
    }

    private void OnDisable()
    {
        m_nextButton.clicked -= OnNextClicked;
        m_retryButton.clicked -= OnRetryClicked;
        m_menuButton.clicked -= OnMenuClicked;
    }

    // Subscribes in Start so the HUD (subscribed in Awake) has saved the results first.
    private void Start()
    {
        Hide();
        EventManager.StartListening(Event.GAME_WIN_EVENT, OnGameWin);
    }

    private void OnDestroy() => EventManager.StopListening(Event.GAME_WIN_EVENT, OnGameWin);

    private void OnGameWin(Dictionary<string, object> message)
    {
        GameStateSO state = GameStateSO.Instance;
        m_resultLabel.text = $"Time {FormatTime(state.CompletionTime)}   ·   Moves {state.CompletionMoveCount}";
        m_nextButton.SetEnabled(state.GetNextSceneName() != null);
        Show();
    }

    private void OnNextClicked() => LoadScene(GameStateSO.Instance.GetNextSceneName());
    private void OnRetryClicked() => LoadScene(GameStateSO.Instance.GetCurrentSceneName());
    private void OnMenuClicked() => LoadScene(LevelListSO.Instance.titleScene);
}
