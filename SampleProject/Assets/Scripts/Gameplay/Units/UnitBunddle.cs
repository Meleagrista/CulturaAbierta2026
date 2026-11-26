using UnityEngine;
using System.Collections.Generic;

public class UnitBunddle : MonoBehaviour
{

    [SerializeField] private int NumberOfUnits = 6;
    [SerializeField] private UnitBehaviour.Team BunddleTeam;
    [SerializeField] private UnitDataSO Data;

    private List<UnitBehaviour> unitList;

    private GameObject UnitPrefab;

    private float unitsLeft;

    private float bundleMaxHealth;
    private int Price;
    private float Defense;
    private float Damage;

    private float currentHealth;

    private void Start()
    {
        unitList = new List<UnitBehaviour>();
        bundleMaxHealth = Data.MaxHealth;
        Price = Data.Price;
        Defense = Data.Defense;
        UnitPrefab = Data.prefab;
        Damage = Data.AttackDamage;

        currentHealth = bundleMaxHealth;

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

    public float GetPercentDamage()
    {
        return Damage / NumberOfUnits;
    }

    public void SpawnUnits(List<Vector3> UnitPositions)
    {
        foreach (Vector3 position in UnitPositions)
        {
            SpawnUnit(position);
        }
    }

    private void SpawnUnit(Vector3 position)
    {
        GameObject unit = Instantiate(UnitPrefab, position, Quaternion.identity);
        UnitBehaviour unitbehaviour = unit.GetComponent<UnitBehaviour>();
        unitList.Add(unitbehaviour);

        unitbehaviour.Spawn(position, BunddleTeam, Data, this);
        unitbehaviour.transform.parent = this.transform;

        unitsLeft++;
    }

    public void Hit(float damage)
    {
        currentHealth -= (damage * 100) / (100 + Defense);

        while (unitsLeft > 0)
        {
            float currentPercentage = (unitsLeft - 1) / (float)NumberOfUnits;

            if (currentHealth <= bundleMaxHealth * currentPercentage)
            {
                RemoveUnit();
            }
            else
            {
                break;
            }
        }
    }

    public void Heal(float healing)
    {
        currentHealth += healing;

        while (unitsLeft < NumberOfUnits)
        {
            float currentPercentage = (unitsLeft) / (float)NumberOfUnits;

            if (currentHealth >= bundleMaxHealth * currentPercentage)
            {
                RespawnUnit(transform.position);
            }
            else
            {
                break;
            }
        }
    }

    private void RemoveUnit()
    {
        int maxHits = int.MinValue;
        UnitBehaviour maxHitsUnit = null;

        foreach (UnitBehaviour unit in unitList)
        {
            int hits = unit.timesAttacked;

            if (hits > maxHits)
            {
                maxHits = hits;
                maxHitsUnit = unit;
            }
        }
        unitList.Remove(maxHitsUnit);
        unitsLeft--;
        Destroy(maxHitsUnit.gameObject);
    }

    private void RespawnUnit(Vector3 position)
    {
        SpawnUnit(position);
    }



}
