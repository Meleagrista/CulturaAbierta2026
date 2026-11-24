using UnityEngine;
using System.Collections.Generic;

public class RangeDetector : MonoBehaviour
{

    private SphereCollider detectionCollider;
    private List<UnitBehaviour> unitBehaviours;


    void Awake()
    {
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true;

        unitBehaviours = new List<UnitBehaviour>();
    }

    private void OnTriggerEnter(Collider other)
    {
        UnitBehaviour unit = other.GetComponent<UnitBehaviour>();
        if(unit)
        {
            if (!unitBehaviours.Contains(unit))
            {
                unitBehaviours.Add(unit);
                unit.OnUnitRemoved += HandleUnitRemoved;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        UnitBehaviour unit = other.GetComponent<UnitBehaviour>();
        if (unit)
        {
            unitBehaviours.Remove(unit);
        }
    }

    public UnitBehaviour GetCloserUnit()
    {
        if (unitBehaviours.Count == 0)
        {
            return null;
        }
        float minDistance = Vector3.Distance(transform.position, unitBehaviours[0].transform.position);
        UnitBehaviour minBehaviour = unitBehaviours[0];


        for (int i = 1; i < unitBehaviours.Count; i++)
        {
            float currentDistance = Vector3.Distance(transform.position, unitBehaviours[i].transform.position);

            if(currentDistance < minDistance)
            {
                minDistance = currentDistance;
                minBehaviour = unitBehaviours[i];
            }
        }

        return minBehaviour;
    }

    public void SetRange(float Range)
    {
        detectionCollider.radius = Range;
    }

    private void HandleUnitRemoved(UnitBehaviour unit)
    {
        unit.OnUnitRemoved -= HandleUnitRemoved;
        unitBehaviours.Remove(unit);
    }
}
