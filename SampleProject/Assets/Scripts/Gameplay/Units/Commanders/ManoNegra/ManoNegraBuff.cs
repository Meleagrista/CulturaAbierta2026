using UnityEngine;

public class ManoNegraBuff : Modifier
{
    private float ActivationHealthPercentage = 0.4f;
    private float HealthPerSecond = 10;
    private float Duration = 10;


    private float BuffStartTime = -1;
    public override void LoadParameters(ModifierDataSO modifierData)
    {
        ManoNegraModifierDataSO data = (ManoNegraModifierDataSO)modifierData;

        ActivationHealthPercentage = data.ActivationHealthPercentage;
        HealthPerSecond = data.HealthPerSecond;
        Duration = data.Duration;
    }

    protected override void ApplyModifier()
    {
        BuffStartTime = Time.time;
    }

    public void Update()
    {
        if(BuffStartTime != -1)
        {
            if(BuffStartTime + Duration >= Time.time)
            {
                BuffStartTime = -1;
            }
        }
    }
}
