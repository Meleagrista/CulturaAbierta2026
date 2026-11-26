using UnityEngine;
using System;

public class UnitBehaviour : MonoBehaviour
{

    public event Action<UnitBehaviour> OnUnitRemoved;
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

    //Unit components

    private Rigidbody unitRB;
    private RangeDetector rangeDetector;

    //Current player sate varibles
    public UnitState currentState;

    //Variables for attacking
    private float timeOfLastAttack;
    private UnitBehaviour currentTarget = null;

    //Variables for movement
    private Vector3 targetPosition;

    private UnitBunddle bundle;

    private void OnDisable()
    {
        OnUnitRemoved?.Invoke(this);
    }

    void Start()
    {
        unitRB = GetComponent<Rigidbody>();
        rangeDetector = GetComponentInChildren<RangeDetector>();

        rangeDetector.SetRange(Range);

        rangeDetector.SetCurrentTeam(unitTeam);

        currentState = UnitState.Idle;
    }
    void Update()
    {
        if (currentState == UnitState.Moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, MovementSpeed * Time.deltaTime);
        }
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
        else
        {
            SetTarget(rangeDetector.GetCloserUnit());
            if (currentTarget)
            {
                currentTarget.Damage(AttackDamage);
            }
        }
    }

    public void SetState(UnitState NewState)
    {
        currentState = NewState;

        if (NewState == UnitState.Attacking)
        {
            if (currentTarget == null)
            {
                SetTarget(rangeDetector.GetCloserUnit());
            }
        }
    }

    public void SetTargetPosition(Vector3 NewTargetPosition)
    {
        targetPosition = NewTargetPosition;
    }

    public Team GetTeam()
    {
        return unitTeam;
    }
}
