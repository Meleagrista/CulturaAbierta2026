using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoseScreenController : UIScreen
{
    [Tooltip("One line per difficulty tier: element 0 = first tier, 1 = second, 2 = third.")]
    [TextArea(1, 3)]
    [SerializeField] private List<string> messages = new List<string>();

    private Label m_messageLabel;
    private Button m_retryButton;
    private Button m_homeButton;

    private void OnEnable()
    {
        m_retryButton = Root.Q<Button>("retry-button");
        m_homeButton = Root.Q<Button>("home-button");
        m_messageLabel = Root.Q<Label>("message-label");

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
        SetTierMessage(m_messageLabel, messages);
        Show();
    }

    // Replays the same level (same tier, same scene).
    private void OnRetryClicked() => LoadScene(SceneManager.GetActiveScene().name);

    private void OnHomeClicked() => LoadScene(LevelListSO.Instance.titleScene);
}
