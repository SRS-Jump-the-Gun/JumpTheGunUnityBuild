using UnityEngine;

public class ProjectileEnemy : EnemyBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    // You can leave Start/Update empty or delete them if not adding extra logic
    // But since EnemyBase has a Start(), you should call base.Start() if you use it!

    protected override void Attack()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // Spawn the projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            // If your Projectile script needs to know who shot it or the target:
            // projectile.GetComponent<Projectile>().Setup(player.transform);
            
            Debug.Log("Projectile Fired!");
        }
    }
}