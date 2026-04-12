using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the projectile travels forward.")]
    public float speed = 20f;

    [Tooltip("How many seconds the bullet exists before being automatically destroyed.")]
    public float lifetime = 2f;

    void Start()
    {
        // This schedules the GameObject for destruction as soon as it is spawned.
        // It prevents "memory leaks" where thousands of missed bullets exist forever in your scene.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the bullet forward along its local Z-axis (blue arrow).
        // We multiply by Time.deltaTime to ensure movement is consistent regardless of frame rate.
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
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

        // Here is where you would typically check if 'other' has a health script:
        // if (other.CompareTag("Player")) { /* Apply Damage */ }


        // Only remove the bullet if it hits an enemy.
        // Removes cases of bullets disappearing when hitting themselves, or invisible trigger colliders.
        if (other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}