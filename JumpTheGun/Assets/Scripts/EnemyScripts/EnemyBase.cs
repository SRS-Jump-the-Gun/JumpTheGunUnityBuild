using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
    protected NavMeshAgent agent;
    [SerializeField] protected GameObject player;
    protected IDamageable playerDamageable;

    [Header("Detection & Chase")]
    [SerializeField] protected float chaseRange = 15f;
    [SerializeField] protected float loseRange = 20f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1.0f;
    protected float nextAttackTime = 0f;
    protected bool isChasing = false;

    [Header("Patrol")]
    public Transform[] destinations;
    private int currentDestination = 0;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
        CachePlayerDamageable();
        
        if (HasPatrol()) agent.destination = destinations[currentDestination].position;
    }

    protected virtual void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        UpdateState(dist);

        if (isChasing)
        {
            if (dist <= attackRange)
            {
                agent.isStopped = true;
                ExecuteAttackLogic();
                
            }
            else
            {
                agent.isStopped = false;
                ChaseStep();
            }
        }
        else
        {
            PatrolStep();
        }
    }

    private void UpdateState(float dist)
    {
        if (!HasPatrol()) { isChasing = true; return; }
        if (!isChasing && dist <= chaseRange) isChasing = true;
        if (isChasing && dist > loseRange) isChasing = false;
    }

    // Logic shared by all: Rotation and Cooldown check
    private void ExecuteAttackLogic()
    {
        agent.isStopped = true;
        FacePlayer();

        if (Time.time >= nextAttackTime)
        {
            Attack(); // Call the specific implementation
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // MANDATORY: Each child defines this
    protected abstract void Attack();

    protected void FacePlayer()
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    protected virtual void ChaseStep()
    {
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);
    }

    protected virtual void PatrolStep()
    {
        if (!HasPatrol()) return;
        agent.isStopped = false;
        if (agent.remainingDistance <= 0.5f)
        {
            currentDestination = (currentDestination + 1) % destinations.Length;
            agent.destination = destinations[currentDestination].position;
        }
    }

    protected bool HasPatrol() => destinations != null && destinations.Length > 0;

    protected void CachePlayerDamageable()
    {
        if (player != null) playerDamageable = player.GetComponentInParent<IDamageable>();
    }
}