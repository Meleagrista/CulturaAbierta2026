using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WinScreenController : UIScreen
{
    [Tooltip("One line per difficulty tier: element 0 = first tier, 1 = second, 2 = third.")]
    [TextArea(1, 3)]
    [SerializeField] private List<string> messages = new List<string>();

    private Button m_nextButton;
    private Button m_homeButton;
    private Label m_messageLabel;

    private void OnEnable()
    {
        m_nextButton = Root.Q<Button>("next-button");
        m_homeButton = Root.Q<Button>("home-button");
        m_messageLabel = Root.Q<Label>("message-label");

        m_nextButton.clicked += OnNextClicked;
        m_homeButton.clicked += OnHomeClicked;
    }

    private void OnDisable()
    {
        m_nextButton.clicked -= OnNextClicked;
        m_homeButton.clicked -= OnHomeClicked;
    }

    private void Start()
    {
        Hide();
        EventManager.StartListening(Event.GAME_WIN_EVENT, OnGameWin);
    }

    private void OnDestroy() => EventManager.StopListening(Event.GAME_WIN_EVENT, OnGameWin);

    private void OnGameWin(Dictionary<string, object> message)
    {
        SetTierMessage(m_messageLabel, messages);
        Show();
    }

    // Random level from the next tier (back to the start if there is none).
    // The WinLast screens hide this button in their UXML.
    private void OnNextClicked()
    {
        string next = GameStateSO.Instance.AdvanceTier();
        LoadScene(next ?? LevelListSO.Instance.titleScene);
    }

    private void OnHomeClicked() => LoadScene(LevelListSO.Instance.titleScene);
}
