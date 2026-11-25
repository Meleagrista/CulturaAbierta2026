using System.Collections.Generic;
using UnityEngine.UIElements;

public class GameOverlayController : UIScreen
{
    private Label m_timerLabel;
    private Label m_movesLabel;

    private float m_elapsedTime;
    private int m_moveCount;
    private bool m_isTimerRunning;

    // Subscribes in Awake so it saves the results before end screens (subscribed in Start) read them.
    private void Awake()
    {
        EventManager.StartListening(Event.GAME_STARTED_EVENT, OnGameStarted);
        EventManager.StartListening(Event.PLAYER_MOVED_EVENT, OnPlayerMoved);
        EventManager.StartListening(Event.GAME_WIN_EVENT, OnGameEnded);
        EventManager.StartListening(Event.GAME_LOSE_EVENT, OnGameEnded);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(Event.GAME_STARTED_EVENT, OnGameStarted);
        EventManager.StopListening(Event.PLAYER_MOVED_EVENT, OnPlayerMoved);
        EventManager.StopListening(Event.GAME_WIN_EVENT, OnGameEnded);
        EventManager.StopListening(Event.GAME_LOSE_EVENT, OnGameEnded);
    }

    private void OnEnable()
    {
        m_timerLabel = Root.Q<Label>("timer-label");
        m_movesLabel = Root.Q<Label>("moves-label");
    }

    private void Start() => ResetLevel();

    private void Update()
    {
        if (!m_isTimerRunning)
            return;

        m_elapsedTime += UnityEngine.Time.deltaTime;
        m_timerLabel.text = FormatTime(m_elapsedTime);
    }

    private void ResetLevel()
    {
        m_elapsedTime = 0f;
        m_moveCount = 0;
        m_isTimerRunning = true;
        m_timerLabel.text = FormatTime(0f);
        m_movesLabel.text = "0";
    }

    private void OnGameStarted(Dictionary<string, object> message) => ResetLevel();

    private void OnPlayerMoved(Dictionary<string, object> message)
    {
        m_moveCount = (int)message["count"];
        m_movesLabel.text = m_moveCount.ToString();
    }

    private void OnGameEnded(Dictionary<string, object> message)
    {
        m_isTimerRunning = false;
    }
}
