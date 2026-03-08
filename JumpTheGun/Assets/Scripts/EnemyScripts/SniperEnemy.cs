using UnityEngine;

/// <summary>
/// A high-damage enemy that stays at range, tracks the player with a laser, 
/// and fires a hitscan shot after a lock-on period.
/// </summary>
public class SniperEnemy : EnemyBase
{
    [Header("Sniper Specifics")]
    [Tooltip("The LineRenderer component used to visualize the sniper's aim.")]
    public LineRenderer laserLine;
    
    [Tooltip("Layers the laser cannot pass through (Walls, Ground, etc.).")]
    public LayerMask obstacleLayer;
    
    [Tooltip("The point where the laser and bullets originate.")]
    public Transform firePoint;
    
    [Tooltip("How directly the enemy must face the player to start locking on (0 to 1).")]
    [Range(0, 1)] public float dotThreshold = 0.5f;
    
    [SerializeField] private int damage = 40;

    [Header("Lock-On Mechanics")]
    [Tooltip("How many seconds the player must be in sight before the sniper fires.")]
    public float timeToLock = 3.0f;
    private float lockOnTimer = 0f;

    protected override void Update()
    {
        // Basic safety checks from EnemyBase
        if (player == null || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        
        // Handle Movement State
        if (dist <= attackRange)
        {
            // If in range, stop moving and rotate to face the target
            agent.isStopped = true;
            FacePlayer();
        }
        else
        {
            // If too far, use the NavMeshAgent to close the gap
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }

        // Every frame, check if we should show the laser and increase lock-on progress
        HandleLaserVisuals();
    }

    private void HandleLaserVisuals()
    {
        if (player == null || firePoint == null || laserLine == null) return;
        
        Vector3 startPos = firePoint.position;
        Vector3 targetPos = player.transform.position;
        Vector3 dirToPlayer = (targetPos - startPos).normalized;

        // Calculate 'Dot Product' to see if the enemy is actually facing the player.
        // 1.0 = looking directly at them, 0 = perpendicular, -1 = looking away.
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (dot > dotThreshold)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, startPos);

            // Cast a ray to see if there is a clear line of sight
            if (Physics.Raycast(startPos, dirToPlayer, out RaycastHit hit, attackRange, obstacleLayer))
            {
                // The laser line ends wherever it hits (Wall or Player)
                laserLine.SetPosition(1, hit.point);

                // If the Raycast hits the Player specifically...
                if (hit.collider.CompareTag("Player"))
                {
                    lockOnTimer += Time.deltaTime;
                    
                    // Visual "Tell": Laser gets thicker as the timer approaches timeToLock
                    float progress = lockOnTimer / timeToLock;
                    laserLine.startWidth = Mathf.Lerp(0.05f, 0.5f, progress);
                    laserLine.endWidth = Mathf.Lerp(0.05f, 0.5f, progress);

                    // If the timer reaches the limit, fire!
                    if (lockOnTimer >= timeToLock)
                    {
                        Attack(); 
                        ResetLockOn(); 
                        Debug.Log("Sniper Attack Executed!");
                    }
                }
                else
                {
                    // Hit a wall/obstacle instead
                    ResetLockOn();
                    Debug.Log("Line of sight broken by obstacle: " + hit.collider.name);
                }
            }
            else
            {
                // Ray hit nothing (reached max distance)
                laserLine.SetPosition(1, startPos + (dirToPlayer * attackRange));
                ResetLockOn();
            }
        }
        else
        {
            // Not facing the player enough to track them
            laserLine.enabled = false;
            ResetLockOn();
        }
    }

    /// <summary>
    /// Executes the actual damage logic once the lock-on is complete.
    /// </summary>
    protected override void Attack()
    {
        Debug.Log("3-Second Lock Complete! Sniper Firing!");
        // playerDamageable is cached in the EnemyBase Start() method
        playerDamageable?.TakeDamage(damage);
    }

    /// <summary>
    /// Resets the timer and laser width back to default.
    /// </summary>
    private void ResetLockOn()
    {
        lockOnTimer = 0;
        if(laserLine != null)
        {
            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.05f;
        }
    }
}