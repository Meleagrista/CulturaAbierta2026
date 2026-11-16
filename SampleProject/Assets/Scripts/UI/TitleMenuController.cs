using UnityEngine;
using UnityEngine.UIElements;

public class TitleMenuController : UIScreen
{
    private Button m_startButton;
    private Button m_quitButton;

    private void OnEnable()
    {
        m_startButton = Root.Q<Button>("start-button");
        m_quitButton = Root.Q<Button>("quit-button");

        m_startButton.clicked += OnStartClicked;
        m_quitButton.clicked += OnQuitClicked;
    }

    private void OnDisable()
    {
        m_startButton.clicked -= OnStartClicked;
        m_quitButton.clicked -= OnQuitClicked;
    }

    private void OnStartClicked() => LoadScene(LevelListSO.Instance.selectionScene);

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
