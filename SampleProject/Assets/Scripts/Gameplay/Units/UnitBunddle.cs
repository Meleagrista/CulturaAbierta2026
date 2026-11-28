using UnityEngine;
using System.Collections.Generic;
using System;


public class UnitBunddle : MonoBehaviour
{
    public event Action<UnitBunddle> OnBundleDied;
    public event Action<UnitBehaviour> OnMemberDied;
    public event Action<UnitBehaviour> OnMemberSpawned;


    [SerializeField] private int NumberOfUnits = 6;
    [SerializeField] private UnitBehaviour.Team BunddleTeam;
    [SerializeField] private UnitDataSO Data;

    private List<UnitBehaviour> unitList;
    private Queue<UnitBehaviour> deadUnit;

    private GameObject UnitPrefab;

    private int unitsLeft;

    private float bundleMaxHealth;
    private int Price;
    private float Defense;
    private float Damage;

    private float DamageModifier;

    private float currentHealth;

    public GridManager gridManager;

    public void SetUp(UnitDataSO data, GridManager gridManager)
    {
        Data = data;

        unitList = new List<UnitBehaviour>();
        deadUnit = new Queue<UnitBehaviour>();
        bundleMaxHealth = Data.MaxHealth;
        Price = Data.Price;
        Defense = Data.Defense;
        UnitPrefab = Data.prefab;
        Damage = Data.AttackDamage;
        this.gridManager = gridManager;
        currentHealth = bundleMaxHealth;
    }

    public float GetPercentDamage()
    {
        return (Damage + DamageModifier) / NumberOfUnits;
    }

    public void SpawnUnits(List<Vector3> UnitPositions)
    {
        foreach (Vector3 position in UnitPositions)
        {
            SpawnUnit(position);
        }
    }

    public void ModifyDamage(float DamageModifier)
    {
        this.DamageModifier = DamageModifier;
    }

    private void SpawnUnit(Vector3 position)
    {
        GameObject unit = Instantiate(UnitPrefab, position, Quaternion.identity,this.transform);
        UnitBehaviour unitbehaviour = unit.GetComponent<UnitBehaviour>();
        unitList.Add(unitbehaviour);

        unitbehaviour.Spawn(position, BunddleTeam, Data, this);

        unitsLeft++;
    }

    public void Hit(float damage)
    {
        currentHealth -= (damage * 100) / (100 + Defense);

        while (unitsLeft > 0)
        {
            float currentPercentage = ((float)unitsLeft - 1) / (float)NumberOfUnits;

            if (currentHealth <= bundleMaxHealth * currentPercentage)
            {
                RemoveUnit();
            }
            else
            {
                break;
            }
        }

        if(currentHealth <= 0)
        {
            OnBundleDied?.Invoke(this);
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
        deadUnit.Enqueue(maxHitsUnit);
        OnMemberDied?.Invoke(maxHitsUnit);
        maxHitsUnit.gameObject.SetActive(false);

    }

    private void RespawnUnit(Vector3 position)
    {
        UnitBehaviour unit = deadUnit.Dequeue();
        unit.gameObject.SetActive(true);
        unit.Respawn(position);
        unitList.Add(unit);
        unitsLeft++;
        OnMemberSpawned?.Invoke(unit);

    }

    public UnitBehaviour.Team GetTeam()
    {
        return BunddleTeam;
    }

    public List<UnitBehaviour> GetUnitList()
    {
        return unitList;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public float GetMaxHealth()
    {
        return bundleMaxHealth;
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
