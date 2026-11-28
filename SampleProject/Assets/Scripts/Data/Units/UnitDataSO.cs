using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitData", menuName = "Game Data/ Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Unit data")]

    public string Name;
    public string Description;

    public float MaxHealth;
    public float Defense;

    public float AttackDamage;
    public float AttackRate;

    public float Range;
    public int Price;

    public float MovementSpeed;

    [Header("HUD")]
    [Tooltip("Card shown in the HUD (Marco* sprites).")]
    public Sprite cardSprite;
    [Tooltip("Speech bubble shown when hovering the card (Hover* sprites).")]
    public Sprite hoverSprite;
    [Tooltip("Footprint icon shown in the hover bubble (Formacion* sprites).")]
    public Sprite formationSprite;

    public GameObject prefab;

    public bool Is2x2;

    public List<Vector2Int> footprint;

    [Header("Ability area (opcional)")]
    [Tooltip("Casillas afectadas por la habilidad, como offsets relativos a la casilla donde se suelta (igual que footprint). Vacío = sin área.")]
    public List<Vector2Int> abilityArea;
    [Tooltip("Color con el que se iluminan esas casillas.")]
    public Color abilityColor = new Color(0.88f, 0.72f, 0.35f, 1f);
}