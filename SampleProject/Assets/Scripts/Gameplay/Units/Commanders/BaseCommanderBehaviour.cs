using UnityEngine;
using System.Collections.Generic;

public class BaseCommanderBehaviour : MonoBehaviour
{
    public List<ModifierDataSO> positionModifiers;
    public List<ModifierDataSO> selfModifiers;
    public List<Vector2Int> tilesAffected;

    private UnitBunddle bundle;
    private GridManager gridManager;

    private void Start()
    {
        CreateModifiers();

        bundle = GetComponent<UnitBunddle>();
        gridManager = bundle.gridManager;
    }

    private void CreateModifiers()
    {
        Vector2Int vector = gridManager.WorldToCell(transform.position);
        foreach(ModifierDataSO modifierData in positionModifiers)
        {
            foreach(Vector2Int vector2 in tilesAffected)
            {
                Vector2Int cell = vector + vector2;
                UnitBunddle bundleat = gridManager.GetAt(cell);

                if (bundleat)
                {
                    Modifier modifierComponent = (Modifier)bundleat.gameObject.AddComponent(modifierData.GetModifierType());
                    modifierComponent.LoadParameters(modifierData);
                }
            }
        }

        foreach(ModifierDataSO modifierData in selfModifiers)
        {
            Modifier modifierComponent = (Modifier)gameObject.AddComponent(modifierData.GetModifierType());
            modifierComponent.LoadParameters(modifierData);
        }
    }
}
