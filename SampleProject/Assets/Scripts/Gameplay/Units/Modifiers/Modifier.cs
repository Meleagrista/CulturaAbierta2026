using UnityEngine;

public abstract class Modifier : MonoBehaviour
{
    private UnitBunddle bundle;

    public void SetBundle(UnitBunddle bundle)
    {
        this.bundle = bundle;
    }

    protected abstract void ApplyModifier();
}
