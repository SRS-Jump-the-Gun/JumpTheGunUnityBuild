using UnityEngine;

public class SniperEnemy : EnemyBase
{
    [Header("Sniper Specifics")]
    public LineRenderer laserLine;
    public LayerMask obstacleLayer;
    public Transform firePoint;
    [Range(0, 1)] public float dotThreshold = 0.5f;
    [SerializeField] private int damage = 40;

    [Header("Lock-On Mechanics")]
    public float timeToLock = 3.0f;
    private float lockOnTimer = 0f;

    protected override void Update()
    {

        if (player == null || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        
        // Handle movement (Stops moving when in range)
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

        HandleLaserVisuals();
    }

    private void HandleLaserVisuals()
    {
        if (player == null || firePoint == null || laserLine == null) return;
        
        Vector3 startPos = firePoint.position;
        Vector3 targetPos = player.transform.position;
        Vector3 dirToPlayer = (targetPos - startPos).normalized;

        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (dot > dotThreshold)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, startPos);

            if (Physics.Raycast(startPos, dirToPlayer, out RaycastHit hit, attackRange, obstacleLayer))
            {
                laserLine.SetPosition(1, hit.point);

                if (hit.collider.CompareTag("Player"))
                {
                    lockOnTimer += Time.deltaTime;
                    
                    // Visual "Tell": Laser gets thicker as it gets closer to firing
                    float progress = lockOnTimer / timeToLock;
                    laserLine.startWidth = Mathf.Lerp(0.05f, 0.5f, progress);
                    laserLine.endWidth = Mathf.Lerp(0.05f, 0.5f, progress);

                    if (lockOnTimer >= timeToLock)
                    {
                        Attack(); 
                        ResetLockOn(); 
                        Debug.Log("Sniper Attack Executed!");
                    }
                }
                else
                {
                    ResetLockOn();
                    Debug.Log("Line of sight broken by obstacle: " + hit.collider.name);
                }
            }
            else
            {
                // If it hits nothing (sky), treat it as a break in line-of-sight
                laserLine.SetPosition(1, targetPos);
                ResetLockOn();
            }
        }
        else
        {
            laserLine.enabled = false;
            ResetLockOn();
        }
    }

    protected override void Attack()
    {
        Debug.Log("3-Second Lock Complete! Sniper Firing!");
        playerDamageable?.TakeDamage(damage);
    }

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