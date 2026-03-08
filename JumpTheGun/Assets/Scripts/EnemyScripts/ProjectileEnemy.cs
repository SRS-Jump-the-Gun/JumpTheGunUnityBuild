using UnityEngine;

/// <summary>
/// A specialized enemy that inherits from EnemyBase to handle ranged projectile attacks.
/// This class focuses on spawning a projectile (like a bullet) when the player is within attack range.
public class ProjectileEnemy : EnemyBase
{
    [Header("Projectile Settings")]
    [Tooltip("The bullet or object the enemy will fire.")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("The exact position and rotation where the bullet will spawn.")]
    [SerializeField] private Transform firePoint;

    /// <summary>
    /// Overrides the abstract Attack method from EnemyBase. 
    /// This is called automatically by the base class when the player is within AttackRange.
    /// 
    protected override void Attack()
    {
        // Safety check to ensure we have a bullet to shoot and a place to shoot it from
        if (projectilePrefab != null && firePoint != null)
        {
            // Spawn the projectile at the FirePoint's position and rotation
            // The 'Instantiate' function creates a clone of the prefab in the game world
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            // Deal damage to the player when the projectile hits them. This can be done by adding a script to the projectile prefab that handles collision and damage logic.
            Debug.Log("Projectile Fired!");
        }
        else
        {
            Debug.LogWarning("ProjectileEnemy: Missing Projectile Prefab or Fire Point!");
        }
    }

    // Since you mentioned the enemy turns slowly, you can override Update 
    // to call the base logic, or simply adjust the rotation speed in FacePlayer.
}