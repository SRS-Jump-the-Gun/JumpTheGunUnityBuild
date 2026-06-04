using UnityEngine;

/// <summary>
/// Expanding ring hazard spawned at the boss's feet after a melee slam.
/// Attach to a flat disc/ring GameObject that has a trigger collider.
/// The ring expands outward over its lifetime and deals damage once per player contact.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GroundShockwave : MonoBehaviour
{
    [Tooltip("Maximum radius the ring expands to.")]
    [SerializeField] private float maxRadius = 10f;

    [Tooltip("Time in seconds to reach maxRadius.")]
    [SerializeField] private float duration = 1.2f;

    [Tooltip("Damage dealt to the player when hit by the ring.")]
    [SerializeField] private int damage = 20;

    private float elapsed;
    private bool hasHitPlayer;

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // Scale the disc outward
        float radius = Mathf.Lerp(0f, maxRadius, t);
        transform.localScale = new Vector3(radius * 2f, transform.localScale.y, radius * 2f);

        if (elapsed >= duration)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHitPlayer) return;
        if (!other.CompareTag("Player")) return;

        IDamageable d = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
        if (d != null)
        {
            d.TakeDamage(damage);
            hasHitPlayer = true;
        }
    }
}
