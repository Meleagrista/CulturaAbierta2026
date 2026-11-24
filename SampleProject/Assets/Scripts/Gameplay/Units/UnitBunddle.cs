using UnityEngine;
using System.Collections.Generic;

public class UnitBunddle : MonoBehaviour
{

    [SerializeField] private GameObject UnitPrefab;
    [SerializeField] private int NumberOfUnits = 6;

    
    private List<GameObject> unitList;

    private int unitsLeft;

    private void Start()
    {
        unitsLeft = NumberOfUnits;
        unitList = new List<GameObject>();

        // --- PRUEBA DE SPAWN EN FILA ---
        List<Vector3> testPositions = new List<Vector3>();

        for (int i = 0; i < NumberOfUnits; i++)
        {
            // Usa la posición del spawner como base y suma 'i' en el eje X
            Vector3 spawnPos = transform.position + new Vector3(i * 1f, 0f, 0f);
            testPositions.Add(spawnPos);
        }

        SpawnUnits(testPositions);
    }

    public void SpawnUnits(List<Vector3> UnitPositions)
    {
        foreach(Vector3 position in UnitPositions)
        {
            GameObject unit = Instantiate(UnitPrefab, position, Quaternion.identity);
            UnitBehaviour unitbehaviour = unit.GetComponent<UnitBehaviour>();
            unitList.Add(unit);

            unitbehaviour.Spawn(position);
            unitbehaviour.OnUnitRemoved += UnitDied;
        }
    }

    private void UnitDied(UnitBehaviour unit)
    {
        unit.OnUnitRemoved -= UnitDied;
        unitList.Remove(unit.gameObject);
        unitsLeft--;
    }

}
