using UnityEngine;
using System.Collections.Generic;

public class BaseCommanderBehaviour : MonoBehaviour
{
    [SerializeField] private List<ModifierDataSO> positionModifiers;
    [SerializeField] private List<ModifierDataSO> selfModifiers;
    [SerializeField] private List<Vector3> tilesAffected;

    private void Start()
    {
        
    }

    private void CreateModifiers()
    {
        foreach(ModifierDataSO modifierData in positionModifiers)
        {

        }

        foreach(ModifierDataSO modifierData in selfModifiers)
        {
            Modifier modifierComponent = (Modifier)gameObject.AddComponent(modifierData.GetModifierType());
            modifierComponent.LoadParameters(modifierData);
        }
    }
}
