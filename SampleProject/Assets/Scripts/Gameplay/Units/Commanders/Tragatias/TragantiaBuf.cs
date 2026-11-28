using UnityEngine;

public class TragantiaBuf : Modifier
{
    private float HealthStolen = 100;
    private float HealthToDamageRate = 0.1f;

    protected override void ApplyModifier()
    {
        Bunddle.ModifyDamage(HealthStolen * HealthToDamageRate);
    }

    public override void LoadParameters(ModifierDataSO modifierData)
    {
        TragantiaModifierDataSO data = (TragantiaModifierDataSO)modifierData;

        HealthStolen = data.HealthStolen;
        HealthToDamageRate = data.HealthToDamageRate;

        ApplyModifier();
    }
}
