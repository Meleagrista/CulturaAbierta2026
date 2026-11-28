using System.Collections.Generic;
using UnityEngine.UIElements;

public class WinScreenController : UIScreen
{
    private Button m_nextButton;
    private Button m_homeButton;

    private void OnEnable()
    {
        m_nextButton = Root.Q<Button>("next-button");
        m_homeButton = Root.Q<Button>("home-button");

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
        // After the last tier there is no next level: only "back to start" is offered.
        m_nextButton.style.display = GameStateSO.Instance.IsLastTier ? DisplayStyle.None : DisplayStyle.Flex;
        Show();
    }

    // Random level from the next difficulty tier.
    private void OnNextClicked()
    {
        string next = GameStateSO.Instance.AdvanceTier();
        LoadScene(next ?? LevelListSO.Instance.titleScene);
    }

    private void OnHomeClicked() => LoadScene(LevelListSO.Instance.titleScene);
}
