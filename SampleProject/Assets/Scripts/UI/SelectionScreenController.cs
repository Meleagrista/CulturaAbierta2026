using UnityEngine;
using UnityEngine.UIElements;

public class SelectionScreenController : UIScreen
{
    [SerializeField] private Transform previewAnchor;
    [SerializeField] private float rotationSpeed = 25f;

    private Label m_levelLabel;
    private Button m_leftButton;
    private Button m_rightButton;
    private Button m_playButton;
    private Button m_backButton;

    private int m_currentIndex;
    private GameObject m_currentPreview;

    private void OnEnable()
    {
        m_levelLabel = Root.Q<Label>("level-label");
        m_leftButton = Root.Q<Button>("left-button");
        m_rightButton = Root.Q<Button>("right-button");
        m_playButton = Root.Q<Button>("play-button");
        m_backButton = Root.Q<Button>("back-button");

        m_leftButton.clicked += OnLeftClicked;
        m_rightButton.clicked += OnRightClicked;
        m_playButton.clicked += OnPlayClicked;
        m_backButton.clicked += OnBackClicked;

        Refresh();
    }

    private void OnDisable()
    {
        m_leftButton.clicked -= OnLeftClicked;
        m_rightButton.clicked -= OnRightClicked;
        m_playButton.clicked -= OnPlayClicked;
        m_backButton.clicked -= OnBackClicked;
    }

    private void Update()
    {
        if (m_currentPreview != null)
            m_currentPreview.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnLeftClicked()
    {
        m_currentIndex--;
        Refresh();
    }

    private void OnRightClicked()
    {
        m_currentIndex++;
        Refresh();
    }

    private void OnPlayClicked() => LoadScene(LevelListSO.Instance.levels[m_currentIndex].sceneName);

    private void OnBackClicked() => LoadScene(LevelListSO.Instance.titleScene);

    private void Refresh()
    {
        var levels = LevelListSO.Instance.levels;
        LevelEntry entry = levels[m_currentIndex];

        m_levelLabel.text = $"Level {m_currentIndex + 1} / {levels.Count}";
        m_leftButton.SetEnabled(m_currentIndex > 0);
        m_rightButton.SetEnabled(m_currentIndex < levels.Count - 1);

        if (m_currentPreview != null)
            Destroy(m_currentPreview);

        if (entry.previewPrefab != null && previewAnchor != null)
            m_currentPreview = Instantiate(entry.previewPrefab, previewAnchor.position, previewAnchor.rotation);
    }
}
