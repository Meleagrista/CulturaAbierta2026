using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseMenuController : UIScreen
{
    private Button m_resumeButton;
    private Button m_menuButton;

    private bool m_isPaused;
    private bool m_gameEnded;

    private void Awake()
    {
        EventManager.StartListening(Event.GAME_WIN_EVENT, OnGameEnded);
        EventManager.StartListening(Event.GAME_LOSE_EVENT, OnGameEnded);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(Event.GAME_WIN_EVENT, OnGameEnded);
        EventManager.StopListening(Event.GAME_LOSE_EVENT, OnGameEnded);
    }

    private void OnEnable()
    {
        m_resumeButton = Root.Q<Button>("resume-button");
        m_menuButton = Root.Q<Button>("menu-button");

        m_resumeButton.clicked += Resume;
        m_menuButton.clicked += OnMenuClicked;
    }

    private void OnDisable()
    {
        m_resumeButton.clicked -= Resume;
        m_menuButton.clicked -= OnMenuClicked;
    }

    private void Start() => Hide();

    private void Update()
    {
        if (m_gameEnded || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (m_isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        m_isPaused = true;
        Time.timeScale = 0f;
        Show();
        EventManager.TriggerEvent(Event.GAME_PAUSE_EVENT, null);
    }

    private void Resume()
    {
        m_isPaused = false;
        Time.timeScale = 1f;
        Hide();
        EventManager.TriggerEvent(Event.GAME_RESUME_EVENT, null);
    }

    private void OnMenuClicked()
    {
        Time.timeScale = 1f;
        LoadScene(LevelListSO.Instance.titleScene);
    }

    private void OnGameEnded(Dictionary<string, object> message)
    {
        m_gameEnded = true;
        m_isPaused = false;
        Hide();
    }
}
