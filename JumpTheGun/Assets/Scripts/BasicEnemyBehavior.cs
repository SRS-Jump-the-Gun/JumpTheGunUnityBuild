using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyBehavior : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Target")]
    [SerializeField] private GameObject player;

    [Header("Patrol (Optional)")]
    public Transform[] destinations;
    private int currentDestination = 0;

    [Header("Chase")]
    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float loseRange = 20f;
    [SerializeField] private float targetSampleDistance = 2f;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int damage = 10;
    [SerializeField] private bool stopToAttack = true;

    private IDamageable playerDamageable;
    private float nextAttackTime = 0f;
    private bool chasing = false;

    private bool HasPatrol => destinations != null && destinations.Length > 0 && destinations[0] != null;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
        CachePlayerDamageable();

        if (HasPatrol && agent != null)
        {
            agent.isStopped = false;
            agent.destination = destinations[currentDestination].position;
        }
    }

    private void Update()
    {
        if (agent == null) return;

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null) return;
            CachePlayerDamageable();
        }

        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (!HasPatrol)
        {
            chasing = true;
        }
        else
        {
            if (!chasing && dist <= chaseRange) chasing = true;
            if (chasing && dist > loseRange) chasing = false;
        }

        if (!chasing)
        {
            PatrolStep();
            return;
        }

        if (dist <= meleeRange)
        {
            if (stopToAttack)
            {
                agent.isStopped = true;
                FacePlayer();
            }
            TryMeleeAttack();
        }
        else
        {
            agent.isStopped = false;
            ChaseStep();
        }
    }

    private void PatrolStep()
    {
        if (!HasPatrol) return;

        if (agent.remainingDistance <= 0.5f)
        {
            currentDestination = (currentDestination + 1) % destinations.Length;
            agent.destination = destinations[currentDestination].position;
        }
    }

    private void ChaseStep()
    {
        Vector3 target = player.transform.position;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, targetSampleDistance, NavMesh.AllAreas))
            target = hit.position;

        agent.SetDestination(target);
    }

    private void CachePlayerDamageable()
    {
        playerDamageable = null;
        if (player == null) return;

        playerDamageable = player.GetComponent<IDamageable>();
        if (playerDamageable == null)
            playerDamageable = player.GetComponentInChildren<IDamageable>();
    }

    private void TryMeleeAttack()
    {
        if (Time.time < nextAttackTime) return;

        if (playerDamageable == null) CachePlayerDamageable();
        if (playerDamageable != null)
            playerDamageable.TakeDamage(damage);

        nextAttackTime = Time.time + attackCooldown;
    }

    private void FacePlayer()
    {
        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.gameObject;
        CachePlayerDamageable();
    }
}