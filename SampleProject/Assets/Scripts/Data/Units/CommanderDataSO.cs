using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CommanderData", menuName = "Game Data/ Commander Data")]
public class CommanderDataSO : UnitDataSO
{
    [Header("Ability area (opcional)")]
    [Tooltip("Casillas afectadas por la habilidad, como offsets relativos a la casilla donde se suelta (igual que footprint). Vacío = sin área.")]
    public List<Vector2Int> abilityArea;
    [Tooltip("Color con el que se iluminan esas casillas.")]
    public Color abilityColor = new Color(0.88f, 0.72f, 0.35f, 1f);

    public List<ModifierDataSO> positionModifiers;
    public List<ModifierDataSO> selfModifiers;
}