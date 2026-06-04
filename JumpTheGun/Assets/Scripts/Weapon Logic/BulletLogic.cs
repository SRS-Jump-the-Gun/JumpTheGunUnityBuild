using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the projectile travels forward.")]
    public float speed = 20f;

    [Tooltip("How many seconds the bullet exists before being automatically destroyed.")]
    public float lifetime = 2f;

    [Header("Damage Settings")]
    [Tooltip("Damage dealt on impact.")]
    public int damage = 0;

    [Tooltip("True for bullets fired by the player; false for enemy bullets.")]
    public bool isPlayerBullet = false;

    void Start()
    {
        // This schedules the GameObject for destruction as soon as it is spawned.
        // It prevents "memory leaks" where thousands of missed bullets exist forever in the scene.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Vector3 moveDirection = transform.forward;
        float moveDistance = speed * Time.deltaTime;
        // mostly a safety to prevent tunneling, continuous collision detection attribute should prevent though
        if (Physics.Raycast(transform.position, moveDirection, out RaycastHit hit, moveDistance))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position += moveDirection * moveDistance;
        }
    }


    /// <summary>
    /// This runs when the bullet's trigger collider touches another collider.
    /// Note: One of the two objects must have a Rigidbody for this to work.
    /// 
    /// <param name="other">The collider that the bullet hit.</param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit: " + other.name);

        Parry parry = other.GetComponent<Parry>();
        if (parry != null)
        {
            transform.rotation = parry.direction;
            return;
        }

        bool hitPlayer = other.CompareTag("Player");

        // Player bullets skip the player; enemy bullets skip everything except the player
        if (isPlayerBullet == hitPlayer)
        {
            Destroy(gameObject);
            return;
        }

        // Player bullets use GetComponent only — prevents crawling up to PlayerHealth
        // through child objects like ShotgunConeCollisionDetect.
        // Enemy bullets use GetComponentInParent so they can reach PlayerHealth on the root.
        IDamageable damageable = isPlayerBullet
            ? other.GetComponent<IDamageable>()
            : other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Pass through trigger volumes with no damage target (e.g. pistol/shotgun collision zones,
        // boss arena triggers). Only solid colliders stop the bullet.
        if (other.isTrigger) return;

        Destroy(gameObject);
    }
}