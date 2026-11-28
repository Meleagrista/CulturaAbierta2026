using UnityEngine;

[RequireComponent(typeof(UnitBunddle))]
public abstract class Modifier : MonoBehaviour
{
    protected UnitBunddle Bunddle;

    private void Awake()
    {
        Bunddle = GetComponent<UnitBunddle>();
    }
    protected abstract void ApplyModifier();
    public abstract void LoadParameters(ModifierDataSO modifierData);
}
