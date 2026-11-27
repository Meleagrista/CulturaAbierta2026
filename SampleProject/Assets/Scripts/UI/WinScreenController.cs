using System.Collections.Generic;
using UnityEngine.UIElements;

public class WinScreenController : UIScreen
{
    private Button m_menuButton;

    private void OnEnable()
    {
        m_menuButton = Root.Q<Button>("menu-button");
        m_menuButton.clicked += OnMenuClicked;
    }

    private void OnDisable()
    {
        m_menuButton.clicked -= OnMenuClicked;
    }

    private void Start()
    {
        Hide();
        EventManager.StartListening(Event.GAME_WIN_EVENT, OnGameWin);
    }

    private void OnDestroy() => EventManager.StopListening(Event.GAME_WIN_EVENT, OnGameWin);

    private void OnGameWin(Dictionary<string, object> message)
    {
        m_menuButton.text = GameStateSO.Instance.IsLastTier ? "Main menu" : "Next";
        Show();
    }

    // Goes to a random level of the next difficulty tier, or back to the title when the run is complete.
    private void OnMenuClicked()
    {
        string next = GameStateSO.Instance.IsLastTier ? null : GameStateSO.Instance.AdvanceTier();
        LoadScene(next ?? LevelListSO.Instance.titleScene);
    }
}
