using UnityEngine;

public class TragantiaBuf : Modifier
{
    [SerializeField] private float HealthStolen = 100;
    [SerializeField] private float DamageStolen = 10;
    [SerializeField] private float HealthToDamageRate = 0.1f;

    protected override void ApplyModifier()
    {
        throw new System.NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
