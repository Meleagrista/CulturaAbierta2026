using UnityEngine;
using UnityEngine.UIElements;

public class TeamSelectionScreenController : UIScreen
{
    private Button m_leftOptionButton;
    private Button m_rightOptionButton;

    private void OnEnable()
    {
        m_leftOptionButton = Root.Q<Button>("andalucia-half");
        m_rightOptionButton = Root.Q<Button>("mancha-half");

        m_leftOptionButton.clicked += OnLeftClicked;
        m_rightOptionButton.clicked += OnRightClicked;

        m_leftOptionButton.RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
        m_rightOptionButton.RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);

        m_leftOptionButton.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveButton);
        m_rightOptionButton.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveButton);
    }

    private void OnDisable()
    {
        m_leftOptionButton.clicked -= OnLeftClicked;
        m_rightOptionButton.clicked -= OnRightClicked;

        m_leftOptionButton.UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
        m_rightOptionButton.UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);

        m_leftOptionButton.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveButton);
        m_rightOptionButton.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveButton);
    }

    private void OnPointerEnterButton(PointerEnterEvent evt) => ((VisualElement)evt.currentTarget).BringToFront();

    private void OnPointerLeaveButton(PointerLeaveEvent evt) => ((VisualElement)evt.currentTarget).SendToBack();

    private void OnLeftClicked()
    {
        GameStateSO.Instance.SetSurplus(0);
        LoadScene(GameStateSO.Instance.StartRun(Side.Left));
    }

    private void OnRightClicked()
    {
        GameStateSO.Instance.SetSurplus(0);
        LoadScene(GameStateSO.Instance.StartRun(Side.Right));
    }
}
