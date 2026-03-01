using UnityEngine;

public class SniperEnemy : EnemyBase
{
    [Header("Sniper Specifics")]
    public LineRenderer laserLine;
    public LayerMask obstacleLayer;
    public Transform firePoint;
    [Range(0, 1)] public float dotThreshold = 0.5f;
    [SerializeField] private int damage = 40;

    protected override void Update()
    {
        // Run the base movement/logic first
        base.Update();

        // Then handle the constant Laser visual logic
        HandleLaserVisuals();
    }

  private void HandleLaserVisuals()
{
    if (player == null || firePoint == null || laserLine == null) return;
    Vector3 startPos = firePoint.position + Vector3.up * 1.0f; ;
    Vector3 targetPos = player.transform.position + Vector3.up * 1.0f;
    Vector3 dirToPlayer = (targetPos - startPos).normalized;

    float dot = Vector3.Dot(transform.forward, dirToPlayer);

    if (dot > dotThreshold)
    {
        laserLine.enabled = true;
        laserLine.SetPosition(0, startPos);
      
    // This is your RED ray - if you see Green/Blue but not Red, the Dot check failed
        Debug.DrawRay(startPos, dirToPlayer * chaseRange, Color.red);

        if (Physics.Raycast(startPos, dirToPlayer, out RaycastHit hit, chaseRange, obstacleLayer))
            {
                laserLine.SetPosition(1, hit.point);
                Debug.Log(hit.point);
        
        }
            else
            {
                Debug.Log("THIS IS RUNNING");
                laserLine.SetPosition(1, targetPos);
            }
    }
    else
    {
        laserLine.enabled = false;
    }
}
    // This is called automatically by the Base class when in range/cooldown
    protected override void Attack()
    {
        Debug.Log("Sniper Firing!");
        
        // Final check: Is the player actually visible?
        Vector3 dir = (player.transform.position - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, chaseRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                playerDamageable.TakeDamage(damage);
                // Optional: Trigger a muzzle flash or sound effect here
            }
        }
    }
}