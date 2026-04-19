using UnityEngine;

/// <summary>
/// A specialized enemy that inherits from EnemyBase to handle ranged projectile attacks.
/// This class focuses on spawning a projectile (like a bullet) when the player is within attack range.
/// </summary>
public class ProjectileEnemy : EnemyBase
{
    [Header("Projectile Settings")]
    [Tooltip("The bullet or object the enemy will fire.")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("The exact position and rotation where the bullet will spawn.")]
    [SerializeField] private Transform firePoint;

    [Header("Line of Sight Detection")]
    [Tooltip("Layers the line of sight cannot pass through (Walls, Ground, etc.).")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("How directly the enemy must face the player to attempt firing (0 to 1).")]
    [Range(0, 1)][SerializeField] private float dotThreshold = 0.5f;

    protected override void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist <= attackRange)
        {
            agent.isStopped = true;
            FacePlayer();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }

        CheckLineOfSightAndAttack();
    }

    private void CheckLineOfSightAndAttack()
    {
        if (player == null || firePoint == null) return;

        Vector3 startPos = firePoint.position;
        Vector3 playerCenter = GetPlayerHitboxCenter();
        Vector3 dirToPlayer = (playerCenter - startPos).normalized;
        float distToPlayer = Vector3.Distance(startPos, playerCenter);

        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        if (dot <= dotThreshold) return;

        bool blocked = Physics.Raycast(startPos, dirToPlayer, distToPlayer, obstacleLayer);
        if (blocked) return;

        if (Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    /// <summary>
    /// Spawns a projectile aimed at the player's center
    /// </summary>
    protected override void Attack()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        Vector3 dirToPlayer = (GetPlayerHitboxCenter() - firePoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dirToPlayer));

        if (projectile.TryGetComponent<Collider>(out var projectileCollider))
        {
            foreach (Collider enemyCollider in GetComponents<Collider>())
                Physics.IgnoreCollision(projectileCollider, enemyCollider);
        }
    }
}
