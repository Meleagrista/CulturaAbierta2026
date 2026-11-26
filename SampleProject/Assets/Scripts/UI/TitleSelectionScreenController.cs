using UnityEngine;
using UnityEngine.UIElements;

public class TitleSelectionScreenController : UIScreen
{
    private Button m_leftOptionButton;
    private Button m_rightOptionButton;

    private void OnEnable()
    {
        m_leftOptionButton = Root.Q<Button>("left-half");
        m_rightOptionButton = Root.Q<Button>("right-half");

        m_leftOptionButton.clicked += OnLeftClicked;
        m_rightOptionButton.clicked += OnRightClicked;
    }

    private void OnDisable()
    {
        m_leftOptionButton.clicked -= OnLeftClicked;
        m_rightOptionButton.clicked -= OnRightClicked;
    }

    private void OnLeftClicked()
    {
        LoadScene(LevelListSO.Instance.leftLevels[0]);
    }

    private void OnRightClicked()
    {
        LoadScene(LevelListSO.Instance.rightLevels[0]);
    }
}
