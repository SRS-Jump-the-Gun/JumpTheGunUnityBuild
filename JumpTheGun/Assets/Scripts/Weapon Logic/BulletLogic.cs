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

    void Start()
    {
        // This schedules the GameObject for destruction as soon as it is spawned.
        // It prevents "memory leaks" where thousands of missed bullets exist forever in your scene.
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
        // Log the name of the object hit to the Console for debugging.
        Debug.Log("Hit: " + other.name);

        Parry parry = other.GetComponent<Parry>();
        if (parry != null)
        {
            transform.rotation = parry.direction;
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
                Debug.Log("Enemy projectile dealt " + damage + " damage to player!");
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}