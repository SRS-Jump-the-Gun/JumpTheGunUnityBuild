using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [SerializeField] private int damage = 10;

    protected override void Attack()
    {
        Debug.Log("Melee Hit!");
        if (playerDamageable != null) playerDamageable.TakeDamage(damage);
    }
}
