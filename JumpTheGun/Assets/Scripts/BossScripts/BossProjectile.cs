using UnityEngine;

/// <summary>
/// Slow boulder-style projectile fired by the boss.
/// - Travels straight, stops on wall-layer colliders, damages player on contact.
/// - If the player presses E (parry window active) on contact, the boulder is
///   reflected back toward the boss and deals parryDamage on hit.
///
/// Setup on the prefab:
///   - Rigidbody: Is Kinematic = OFF, Use Gravity = OFF
///   - Collider:  Sphere Collider, Is Trigger = OFF
///   - Remove BulletLogic if present
///
/// In the Inspector:
///   - Wall Layer: your wall layer
///   - BossEnemy is set automatically by BossEnemy.FireProjectile at spawn time
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed    = 6f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Damage")]
    [SerializeField] private int damage      = 20;
    [SerializeField] private int parryDamage = 60;

    [Header("Collision")]
    [Tooltip("Layers that stop the boulder (e.g. Wall). Player is handled by tag.")]
    [SerializeField] private LayerMask wallLayer;

    private Rigidbody   rb;
    private BossEnemy   boss;
    private Collider[]  bossColliders;  // cached so we can re-enable collision on parry
    private Collider    myCollider;
    private bool        stopped;
    private bool        isParried;

    // Called by BossEnemy.FireProjectile immediately after Instantiate
    public void SetBoss(BossEnemy b)
    {
        boss          = b;
        bossColliders = b.GetComponentsInChildren<Collider>();
    }

    private void Awake()
    {
        rb              = GetComponent<Rigidbody>();
        myCollider      = GetComponent<Collider>();
        rb.useGravity   = false;
        rb.linearDamping = 0f;
    }

    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stopped) return;

        bool hitPlayer = collision.collider.CompareTag("Player");
        bool hitWall   = (wallLayer.value & (1 << collision.collider.gameObject.layer)) != 0;
        bool hitBoss   = isParried && collision.collider.GetComponentInParent<BossEnemy>() != null;

        // ── Parried projectile hits the boss ──────────────────────────────
        if (hitBoss)
        {
            stopped           = true;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic    = true;
            boss?.TakeDamage(parryDamage);
            Destroy(gameObject);
            return;
        }

        // ── Player contact ────────────────────────────────────────────────
        if (hitPlayer)
        {
            // Check whether the player has an active Parry component
            // (PlayerParry activates/deactivates the Parry child object on E press)
            Parry activeParry = collision.collider.transform.root
                                    .GetComponentInChildren<Parry>();

            if (activeParry != null && boss != null)
            {
                // Re-enable collision with the boss — Physics.IgnoreCollision was set
                // at spawn to prevent self-hits; must be cleared so the return shot lands
                if (myCollider != null && bossColliders != null)
                    foreach (var bc in bossColliders)
                        Physics.IgnoreCollision(myCollider, bc, false);

                // Reflect toward the boss
                Vector3 dirToBoss = (boss.transform.position + Vector3.up * 1f
                                     - transform.position).normalized;
                rb.isKinematic    = false;
                rb.linearVelocity = dirToBoss * speed;
                isParried         = true;
                Debug.Log("Boss projectile parried!");
                return;
            }

            // Normal hit — deal damage and vanish
            stopped           = true;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic    = true;

            IDamageable d = collision.collider.GetComponent<IDamageable>()
                         ?? collision.collider.GetComponentInParent<IDamageable>();
            d?.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // ── Wall contact ──────────────────────────────────────────────────
        if (hitWall)
        {
            stopped           = true;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic    = true;
            Destroy(gameObject, 1.5f);
        }
    }
}
