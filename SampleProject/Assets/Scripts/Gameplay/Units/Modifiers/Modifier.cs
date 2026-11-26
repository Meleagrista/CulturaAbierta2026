using UnityEngine;

[RequireComponent(typeof(UnitBehaviour))]
public abstract class Modifier : MonoBehaviour
{
    protected abstract void ApplyModifier();
    public abstract void LoadParameters(ModifierDataSO modifierData);
}
