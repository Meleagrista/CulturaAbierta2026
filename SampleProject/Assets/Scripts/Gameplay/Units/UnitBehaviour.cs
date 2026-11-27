using UnityEngine;
using System;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class UnitBehaviour : MonoBehaviour
{
    public enum UnitState
    {
        Moving,
        Attacking,
        Idle
    }

    public enum Team
    {
        PlayerTeam,
        EnemyTeam
    }

    private string Name;
    private string Description;

    private float AttackDamage;

    private float AttackRate;

    private float Range;

    private float MovementSpeed;

    private Team unitTeam;

    public int timesAttacked = 0;

    //Current player sate varibles
    public UnitState currentState;
    private NavMeshAgent agentComponent;

    //Variables for attacking
    private float timeOfLastAttack;
    private UnitBehaviour currentTarget = null;

    //Variables for movement
    private Vector3 targetPosition;

    private UnitBunddle bundle;

    void Awake()
    {
        agentComponent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        if (currentState == UnitState.Attacking)
        {
            if (Time.time - timeOfLastAttack >= AttackRate)
            {
                Attack();
            }
        }
    }

    public void Spawn(Vector3 Position, Team UnitTeam, UnitDataSO Data, UnitBunddle Bundle)
    {
        this.bundle = Bundle;

        Name = Data.Name;
        Description = Data.Description;
        AttackDamage = bundle.GetPercentDamage();
        AttackRate = Data.AttackRate;
        Range = Data.Range;
        MovementSpeed = Data.MovementSpeed;

        this.transform.position = Position;
        unitTeam = UnitTeam;
        targetPosition = transform.position;

        currentState = UnitState.Idle;
    }

    public void Respawn(Vector3 Position)
    {
        agentComponent.Warp(Position);
    }

    public void Damage(float Damage)
    {
        timesAttacked++;
        bundle.Hit(Damage);
    }

    public void SetTarget(UnitBehaviour target)
    {
        currentTarget = target;
    }

    public void Attack()
    {
        timeOfLastAttack = Time.time;

        if (currentTarget)
        {
            currentTarget.Damage(AttackDamage);
        }
    }

    public void SetState(UnitState NewState)
    {
        currentState = NewState;
    }

    public void SetTargetPosition(Vector3 NewTargetPosition)
    {
        agentComponent.SetDestination(NewTargetPosition);
    }

    public Team GetTeam()
    {
        return unitTeam;
    }

    public float GetRange()
    {
        return Range;
    }

    public UnitBunddle GetBundle()
    {
        return bundle;
    }
}
