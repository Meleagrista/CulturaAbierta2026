using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquadAI : MonoBehaviour
{
    const float ThinkInterval = 0.25f;
    const float RangeTolerance = 0.5f;   // para que el melee de rango 1 llegue tambien en diagonal
    const float BaseAgentRadius = 0.4f;  // radio de una unidad normal; lo que pase de esto suma rango (comandante)
    const float RepathDistance = 0.3f;   // no repetir SetDestination si el destino apenas se ha movido

    UnitBunddle bundle, target;
    List<UnitBunddle> enemyBundles;

    float thinkTimer;
    bool bHasLockedTarget = false;

    // Lo que ha mandado la IA a cada integrante (para gizmos y para detectar objetivos muertos)
    struct Order
    {
        public UnitBehaviour enemy;
        public bool bAttacking;
    }

    readonly Dictionary<UnitBehaviour, Order> orders = new Dictionary<UnitBehaviour, Order>();
    readonly Dictionary<UnitBehaviour, Vector3> lastDestinations = new Dictionary<UnitBehaviour, Vector3>();

    void Awake()
    {
        bundle = GetComponent<UnitBunddle>();
        thinkTimer = Random.Range(0f, ThinkInterval);
    }

    public void Init(List<UnitBunddle> enemies)
    {
        enemyBundles = enemies;
    }

    void Update()
    {
        if (enemyBundles == null) return;

        // Si alguien estaba pegando a un integrante que acaba de morir, pensar ya y no esperar al siguiente turno
        if (SomeTargetDied())
        {
            thinkTimer = 0f;
        }

        thinkTimer -= Time.deltaTime;
        if (thinkTimer > 0f) return;

        thinkTimer += ThinkInterval;
        Think();
    }

    void Think()
    {
        if (bundle == null || !bundle.IsAlive())
        {
            orders.Clear();
            return;
        }

        if (target == null || !target.IsAlive())
        {
            target = null;
            bHasLockedTarget = false;
        }

        if (!bHasLockedTarget)
        {
            target = FindClosestEnemyBundle();
        }

        if (target == null)
        {
            SetAllIdle();
            return;
        }

        // 4. Ordenes a cada integrante
        bool bAnyInRange = false;

        foreach (UnitBehaviour member in bundle.GetUnitList())
        {
            if (!IsUnitAlive(member)) continue;

            UnitBehaviour closestEnemy = FindClosestMember(member, target, out float sqrDistance);
            if (closestEnemy == null) continue;

            float range = GetAttackRange(member, closestEnemy);

            if (sqrDistance <= range * range)
            {
                OrderAttack(member, closestEnemy);
                bAnyInRange = true;
            }
            else
            {
                OrderMove(member, closestEnemy);
            }
        }

        bHasLockedTarget = bAnyInRange;
    }

    // ---------- Buscar objetivos ----------

    UnitBunddle FindClosestEnemyBundle()
    {
        UnitBunddle closest = null;
        float bestDistance = float.MaxValue;

        foreach (UnitBunddle enemyBundle in enemyBundles)
        {
            if (enemyBundle == null || !enemyBundle.IsAlive()) continue;

            float distance = BundleSqrDistance(bundle, enemyBundle);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = enemyBundle;
            }
        }

        return closest;
    }

    // Minima distancia entre cualquier pareja de integrantes vivos
    float BundleSqrDistance(UnitBunddle a, UnitBunddle b)
    {
        float best = float.MaxValue;

        foreach (UnitBehaviour unitA in a.GetUnitList())
        {
            if (!IsUnitAlive(unitA)) continue;

            foreach (UnitBehaviour unitB in b.GetUnitList())
            {
                if (!IsUnitAlive(unitB)) continue;

                float distance = FlatSqrDistance(unitA.transform.position, unitB.transform.position);
                if (distance < best) best = distance;
            }
        }

        return best;
    }

    UnitBehaviour FindClosestMember(UnitBehaviour member, UnitBunddle enemyBundle, out float bestSqrDistance)
    {
        UnitBehaviour closest = null;
        bestSqrDistance = float.MaxValue;

        foreach (UnitBehaviour enemy in enemyBundle.GetUnitList())
        {
            if (!IsUnitAlive(enemy)) continue;

            float distance = FlatSqrDistance(member.transform.position, enemy.transform.position);
            if (distance < bestSqrDistance)
            {
                bestSqrDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    // ---------- Ordenes ----------

    void OrderAttack(UnitBehaviour member, UnitBehaviour enemy)
    {
        NavMeshAgent agent = member.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // El agente parado no rota solo: mirar al enemigo a mano
        Vector3 direction = enemy.transform.position - member.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
        {
            member.transform.rotation = Quaternion.LookRotation(direction);
        }

        member.SetTarget(enemy);
        member.SetState(UnitBehaviour.UnitState.Attacking);

        orders[member] = new Order { enemy = enemy, bAttacking = true };
        lastDestinations.Remove(member);
    }

    void OrderMove(UnitBehaviour member, UnitBehaviour enemy)
    {
        NavMeshAgent agent = member.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        member.SetTarget(null);
        member.SetState(UnitBehaviour.UnitState.Moving);

        Vector3 destination = enemy.transform.position;
        bool bNeedsNewPath = !lastDestinations.TryGetValue(member, out Vector3 lastDestination)
            || FlatSqrDistance(lastDestination, destination) > RepathDistance * RepathDistance;

        if (bNeedsNewPath)
        {
            member.SetTargetPosition(destination);
            lastDestinations[member] = destination;
        }

        orders[member] = new Order { enemy = enemy, bAttacking = false };
    }

    void SetAllIdle()
    {
        foreach (UnitBehaviour member in bundle.GetUnitList())
        {
            if (!IsUnitAlive(member)) continue;

            NavMeshAgent agent = member.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            member.SetTarget(null);
            member.SetState(UnitBehaviour.UnitState.Idle);
        }

        orders.Clear();
        lastDestinations.Clear();
    }

    // ---------- Utilidades ----------

    bool SomeTargetDied()
    {
        foreach (KeyValuePair<UnitBehaviour, Order> pair in orders)
        {
            if (pair.Value.bAttacking && !IsUnitAlive(pair.Value.enemy)) return true;
        }

        return false;
    }

    float GetAttackRange(UnitBehaviour attacker, UnitBehaviour defender)
    {
        return attacker.GetRange() + RangeTolerance + SizeBonus(attacker) + SizeBonus(defender);
    }

    // Las unidades grandes (comandante 2x2) llevan un agente mas ancho: su tamano extra cuenta como alcance
    static float SizeBonus(UnitBehaviour unit)
    {
        NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
        if (agent == null) return 0f;

        return Mathf.Max(0f, agent.radius - BaseAgentRadius);
    }

    // Los muertos se desactivan (SetActive(false)), no se destruyen: hay que mirar las dos cosas
    static bool IsUnitAlive(UnitBehaviour unit)
    {
        return unit != null && unit.isActiveAndEnabled;
    }

    static float FlatSqrDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return (a - b).sqrMagnitude;
    }

    // ---------- Depuracion ----------

    void OnDrawGizmos()
    {
        foreach (KeyValuePair<UnitBehaviour, Order> pair in orders)
        {
            if (!IsUnitAlive(pair.Key) || !IsUnitAlive(pair.Value.enemy)) continue;

            Gizmos.color = pair.Value.bAttacking ? Color.red : Color.yellow;
            Gizmos.DrawLine(pair.Key.transform.position + Vector3.up, pair.Value.enemy.transform.position + Vector3.up);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (bundle == null) return;

        Gizmos.color = Color.cyan;
        foreach (UnitBehaviour member in bundle.GetUnitList())
        {
            if (!IsUnitAlive(member)) continue;

            Gizmos.DrawWireSphere(member.transform.position, member.GetRange() + RangeTolerance + SizeBonus(member));
        }
    }
}
