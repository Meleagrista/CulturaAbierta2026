using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleHUDController : UIScreen
{
    static readonly List<BattleHUDController> s_active = new List<BattleHUDController>();

    [Header("Scene references")]
    [SerializeField] private GridManager playerGrid;
    [SerializeField] private GridMesh gridInput;

    [Header("UI")]
    [SerializeField] private VisualTreeAsset cardTemplate;

    [Header("Tooltip")]
    [Tooltip("Gap in pixels between the bottom of the bubble and the top of the card.")]
    [SerializeField] private float tooltipOffsetY = 10f;
    [Tooltip("Where the bubble's tail is, as a fraction of its width (0 = left, 1 = right).")]
    [Range(0f, 1f)]
    [SerializeField] private float tooltipTailX = 0.4f;

    [Header("Team")]
    [Tooltip("Units this team can buy, in the order they appear in the HUD.")]
    [SerializeField] private List<UnitDataSO> roster = new List<UnitDataSO>();

    private VisualElement m_hud;
    private Label m_moneyLabel;
    private VisualElement m_cardRow;
    private Button m_fightButton;

    private VisualElement m_tooltip;
    private Label m_tooltipName;
    private Label m_tooltipDescription;
    private VisualElement m_tooltipFormation;

    private readonly Dictionary<UnitDataSO, VisualElement> m_cards = new Dictionary<UnitDataSO, VisualElement>();
    private UnitDataSO m_selected;
    private bool m_fightStarted;

    /// <summary>True when the mouse is over a card or button, so the grid ignores the click.</summary>
    public static bool IsPointerOverHUD(Vector2 screenPos)
    {
        foreach (var hud in s_active)
        {
            if (hud.IsPointerOver(screenPos))
            {
                return true;
            }
        }
        return false;
    }

    private void OnEnable()
    {
        m_hud =                 Root.Q("battle-hud");
        m_moneyLabel =          Root.Q<Label>("money-label");
        m_cardRow =             Root.Q("card-row");
        m_fightButton =         Root.Q<Button>("fight-button");
        m_tooltip =             Root.Q("tooltip");
        m_tooltipName =         Root.Q<Label>("tooltip-name");
        m_tooltipDescription =  Root.Q<Label>("tooltip-description");
        m_tooltipFormation =    Root.Q("tooltip-formation");

        m_fightButton.clicked += OnFightClicked;
        BuildCards();

        s_active.Add(this);
    }

    private void OnDisable()
    {
        m_fightButton.clicked -= OnFightClicked;
        s_active.Remove(this);
    }

    private void Start()
    {
        if (playerGrid == null)
        {
            Debug.LogWarning("No player grid assigned.", this);
            return;
        }

        playerGrid.OnMoneyChanged += OnMoneyChanged;
        OnMoneyChanged(playerGrid.Money);

        Select(null);
    }

    private void OnDestroy()
    {
        if (playerGrid != null)
        {
            playerGrid.OnMoneyChanged -= OnMoneyChanged;
        }
    }

    private void BuildCards()
    {
        m_cardRow.Clear();
        m_cards.Clear();

        if (cardTemplate == null)
        {
            Debug.LogWarning("No card template assigned.", this);
            return;
        }

        foreach (var unit in roster)
        {
            if (unit == null)
            {
                continue;
            }

            VisualElement card = cardTemplate.Instantiate().Q("card");
            FillCard(card, unit);

            card.RegisterCallback<ClickEvent>(_ => OnCardClicked(unit));
            card.RegisterCallback<PointerEnterEvent>(_ => ShowTooltip(unit, card));
            card.RegisterCallback<PointerLeaveEvent>(_ => HideTooltip());

            m_cardRow.Add(card);
            m_cards[unit] = card;
        }
    }

    private static void FillCard(VisualElement card, UnitDataSO unit)
    {
        if (unit.cardSprite != null)
        {
            card.style.backgroundImage = new StyleBackground(unit.cardSprite);
        }

        card.Q<Label>("price-label").text = unit.Price.ToString();
        card.Q<Label>("attack-label").text = Mathf.RoundToInt(unit.AttackDamage).ToString();
        card.Q<Label>("defense-label").text = Mathf.RoundToInt(unit.Defense).ToString();
        card.Q<Label>("health-label").text = Mathf.RoundToInt(unit.MaxHealth).ToString();
        card.Q<Label>("name-label").text = unit.Name.ToUpperInvariant();
    }

    private void OnCardClicked(UnitDataSO unit)
    {
        if (m_fightStarted)
        {
            return;
        }

        // Clicking the selected card again deselects it.
        Select(m_selected == unit ? null : unit);
    }

    private void Select(UnitDataSO unit)
    {
        m_selected = unit;

        if (gridInput != null)
        {
            gridInput.selectedUnit = unit;
        }

        foreach (var pair in m_cards)
        {
            pair.Value.EnableInClassList("unit-card--selected", pair.Key == unit);
        }
    }

    private void OnMoneyChanged(int money)
    {
        m_moneyLabel.text = money.ToString();

        foreach (var pair in m_cards)
        {
            pair.Value.EnableInClassList("unit-card--disabled", m_fightStarted || pair.Key.Price > money);
        }
    }

    private void ShowTooltip(UnitDataSO unit, VisualElement card)
    {
        if (unit.hoverSprite != null)
        {
            m_tooltip.style.backgroundImage = new StyleBackground(unit.hoverSprite);
        }

        m_tooltipName.text = unit.Name;
        m_tooltipDescription.text = unit.Description;
        m_tooltipFormation.style.backgroundImage = unit.formationSprite != null ? new StyleBackground(unit.formationSprite) : new StyleBackground(StyleKeyword.None);

        Rect cardRect = m_hud.WorldToLocal(card.worldBound);
        m_tooltip.style.top = StyleKeyword.Auto;
        m_tooltip.style.bottom = m_hud.layout.height - cardRect.yMin + tooltipOffsetY;
        m_tooltip.style.left = cardRect.center.x;
        m_tooltip.style.translate = new Translate(Length.Percent(-tooltipTailX * 100f), 0);

        SetTooltipVisible(true);
    }

    private void HideTooltip() => SetTooltipVisible(false);

    private void SetTooltipVisible(bool visible)
    {
        m_tooltip.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        m_tooltip.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }

    private void OnFightClicked()
    {
        if (m_fightStarted)
        {
            return;
        }

        m_fightStarted = true;
        Select(null);
        HideTooltip();
        m_fightButton.SetEnabled(false);
        OnMoneyChanged(playerGrid != null ? playerGrid.Money : 0);

        EventManager.TriggerEvent(Event.FIGHT_STARTED_EVENT, null);
    }

    private bool IsPointerOver(Vector2 screenPos)
    {
        IPanel panel = m_hud?.panel;
        if (panel == null || m_hud.resolvedStyle.display == DisplayStyle.None)
        {
            return false;
        }

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPos.x, Screen.height - screenPos.y));
        VisualElement picked = panel.Pick(panelPos);

        return picked != null && (m_cardRow.Contains(picked) || m_fightButton.Contains(picked));
    }
}
