using UnityEngine;
using UnityEngine.AI;

// Ensures that any object with this script also has a NavMeshAgent component
[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
    protected NavMeshAgent agent;
    [SerializeField] protected GameObject player;
    protected IDamageable playerDamageable; // Interface to deal damage to the player

    [Header("Detection & Chase")]
    [SerializeField] protected float chaseRange = 15f;  // Distance to start chasing
    [SerializeField] protected float loseRange = 20f;   // Distance to stop chasing
    [SerializeField] protected float attackRange = 2f;  // Distance to stop and start attacking
    [SerializeField] protected float attackCooldown = 1.0f; // Seconds between attacks
    protected float nextAttackTime = 0f;
    protected bool isChasing = false;

    [Header("Patrol")]
    public Transform[] destinations; // List of points the enemy walks between
    private int currentDestination = 0;

    protected virtual void Awake()
    {
        // Initialize the agent reference
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        // Automatically find the player by tag if not assigned in Inspector
        if (player == null) player = GameObject.FindWithTag("Player");

        CachePlayerDamageable();

        // Start patrolling if points are assigned
        if (HasPatrol()) agent.destination = destinations[currentDestination].position;
    }

    protected virtual void Update()
    {
        // Safety check to prevent errors if player is missing or NavMesh isn't ready
        if (player == null || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        UpdateState(dist); // Decide whether to Chase or Patrol

        if (isChasing)
        {
            if (dist <= attackRange)
            {
                // Stop moving and start the attack cycle
                agent.isStopped = true;
                ExecuteAttackLogic();
            }
            else
            {
                // Keep moving toward the player
                agent.isStopped = false;
                ChaseStep();
            }
        }
        else
        {
            // Walk between patrol points
            PatrolStep();
        }
    }

    /// <summary>
    /// Swaps the enemy state between Patrolling and Chasing based on player distance.
    private void UpdateState(float dist)
    {
        // If there's nowhere to patrol, stay in "Chase" mode (searching)
        if (!HasPatrol()) { isChasing = true; return; }

        // Logic for entering and leaving the chase state (Hysteresis prevents flickering)
        if (!isChasing && dist <= chaseRange) isChasing = true;
        if (isChasing && dist > loseRange) isChasing = false;
    }

    /// <summary>
    /// Handles the timing and orientation during an attack.
    private void ExecuteAttackLogic()
    {
        agent.isStopped = true;
        FacePlayer(); // Ensure the enemy is looking at the target while attacking

        // Check if the cooldown period has passed
        if (Time.time >= nextAttackTime)
        {
            Attack(); // Runs the specific code in the child class (Sniper, Projectile, etc.)
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // MANDATORY: Every enemy (SniperEnemy, ProjectileEnemy) must write their own version of this
    protected abstract void Attack();

    /// <summary>
    /// Smoothly rotates the enemy to face the player's position on the Y-axis.
    protected void FacePlayer()
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        dir.y = 0; // Keep the enemy upright (don't tilt up/down)

        if (dir.sqrMagnitude > 0.01f)
        {
            // Slerp provides smooth rotation. Increase '10f' to make them turn faster.
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }

    protected virtual void ChaseStep()
    {
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);
    }

    /// <summary>
    /// Cycles through the 'destinations' array.
    protected virtual void PatrolStep()
    {
        if (!HasPatrol()) return;

        agent.isStopped = false;

        // Check if the enemy has reached the current patrol point
        if (agent.remainingDistance <= 0.5f)
        {
            currentDestination = (currentDestination + 1) % destinations.Length;
            agent.destination = destinations[currentDestination].position;
        }
    }

    // Helper: Returns true if there are patrol points assigned
    protected bool HasPatrol() => destinations != null && destinations.Length > 0;

    /// <summary>
    /// Grabs the IDamageable component from the player so we can call TakeDamage() later.
    protected void CachePlayerDamageable()
    {
        if (player != null) playerDamageable = player.GetComponentInParent<IDamageable>();
    }

    /// <summary>
    /// Dynamically calculates the actual center of the player's hitbox, there might be a cleaner way to do this lol
    /// </summary>
    protected Vector3 GetPlayerHitboxCenter()
    {
        if (player == null) return Vector3.zero;

        // Try to find a CapsuleCollider on the player
        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        if (capsule == null) capsule = player.GetComponentInChildren<CapsuleCollider>();
        if (capsule == null) capsule = player.GetComponentInParent<CapsuleCollider>();

        // If found, calculate the actual center based on the collider's dimensions
        if (capsule != null)
        {
            return player.transform.position + capsule.center;
        }

        // fallback, assumes height of 1 unit
        return player.transform.position + Vector3.up * 0.5f;
    }
}