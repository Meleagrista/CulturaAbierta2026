using UnityEngine;
using System;

public class UnitBehaviour : MonoBehaviour
{

    public event Action<UnitBehaviour> OnUnitRemoved;
    public enum UnitState
    {
        Moving,
        Attacking
    }

    [SerializeField] private UnitDataSO Data;

    private string Name;
    private string Description;

    private float MaxHealth;
    private float Defense;

    private float AttackDamage;
    private float AttackRate;

    private float Range;
    private int Price;

    private float MovementSpeed;

    //Unit components

    private Rigidbody unitRB;
    private RangeDetector rangeDetector;

    //Current player sate varibles
    private float currentHealt;
    public UnitState currentState;

    //Variables for attacking
    private float timeOfLastAttack;
    private UnitBehaviour currentTarget = null;

    //Variables for movement
    private Vector3 targetPosition;

    private void OnDisable()
    {
        OnUnitRemoved?.Invoke(this);
    }

    void Start()
    {
        Name = Data.Name;
        Description = Data.Description;
        MaxHealth = Data.MaxHealth;
        AttackDamage = Data.AttackDamage;
        AttackRate = Data.AttackRate;
        Range = Data.Range;
        Price = Data.Price;
        MovementSpeed = Data.MovementSpeed;
        Defense = Data.Defense;

        unitRB = GetComponent<Rigidbody>();
        rangeDetector = GetComponentInChildren<RangeDetector>();

        rangeDetector.SetRange(Range);

        targetPosition = transform.position;

        Spawn(transform.position);
    }
    void Update()
    {
        if(currentState == UnitState.Moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, MovementSpeed * Time.deltaTime);
        }
        if(currentState == UnitState.Attacking)
        {
            if(Time.time - timeOfLastAttack >= AttackRate)
            {
                Attack();
            }
        }
    }

    public void Spawn(Vector3 Position)
    {
        currentHealt = MaxHealth;

        this.transform.position = Position;
    }

    public void Damage(float Damage)
    {
        currentHealt -= Damage;

        if(currentHealt <= 0)
        {
            Die();
        }
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

    private void Die()
    {
        Destroy(this.gameObject);
    }

    public void SetState(UnitState NewState)
    {
        currentState = NewState;

        if(NewState == UnitState.Attacking)
        {
            if(currentTarget == null)
            {
                SetTarget(rangeDetector.GetCloserUnit());
            }
        }
    }

    public void SetTargetPosition(Vector3 NewTargetPosition)
    {
        targetPosition = NewTargetPosition;
    }
}
