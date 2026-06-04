using System.Collections;
using UnityEngine;

/// <summary>
/// Drops from above the player's position after showing a warning marker on the ground.
/// Attach to any rigid-body GameObject (crate, barrel, desk, etc.).
/// The warning marker is a child GameObject — assign it in the Inspector or let the script
/// create a flat quad automatically.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FallingDebris : MonoBehaviour
{
    [Tooltip("How high above the target position this spawns.")]
    [SerializeField] private float spawnHeight = 12f;

    [Tooltip("Seconds to show the warning before the debris starts falling.")]
    [SerializeField] private float warningDuration = 1.5f;

    [Tooltip("Damage dealt on impact.")]
    [SerializeField] private int damage = 35;

    [Tooltip("Radius around the impact point that counts as a hit.")]
    [SerializeField] private float impactRadius = 1.8f;

    [Tooltip("Optional flat disc/quad child GameObject used as the warning shadow.")]
    [SerializeField] private GameObject warningMarker;

    private Rigidbody rb;
    private bool hasFallen;
    private bool hasDealtDamage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Hold still during warning phase
        rb.useGravity  = false;
    }

    /// <summary>
    /// Call this immediately after Instantiate to set the landing position.
    /// </summary>
    public void Initialize(Vector3 targetWorldPos)
    {
        // Position above the target
        transform.position = targetWorldPos + Vector3.up * spawnHeight;

        if (warningMarker != null)
        {
            // Place the warning on the ground directly below
            warningMarker.transform.position = targetWorldPos;
            warningMarker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            warningMarker.SetActive(true);
        }

        StartCoroutine(DropSequence(targetWorldPos));
    }

    private IEnumerator DropSequence(Vector3 landPos)
    {
        // Warning phase — pulse the marker
        float t = 0f;
        while (t < warningDuration)
        {
            t += Time.deltaTime;

            if (warningMarker != null)
            {
                // Blink faster as we get closer to the drop
                float blinkSpeed = Mathf.Lerp(4f, 20f, t / warningDuration);
                warningMarker.SetActive(Mathf.Sin(t * blinkSpeed) > 0f);
            }

            yield return null;
        }

        if (warningMarker != null)
            warningMarker.SetActive(false);

        // Start falling
        rb.isKinematic = false;
        rb.useGravity  = true;
        hasFallen      = true;

        // Self-destruct if it falls forever (e.g., missed the floor)
        Destroy(gameObject, 6f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasFallen || hasDealtDamage) return;

        hasDealtDamage = true;

        // Direct hit — destroy immediately and deal damage
        if (collision.collider.CompareTag("Player"))
        {
            IDamageable d = collision.collider.GetComponent<IDamageable>()
                         ?? collision.collider.GetComponentInParent<IDamageable>();
            d?.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Hit the floor — check if player is close enough to still catch the blast
        Collider[] nearby = Physics.OverlapSphere(transform.position, impactRadius);
        foreach (Collider col in nearby)
        {
            if (!col.CompareTag("Player")) continue;
            IDamageable d = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
            d?.TakeDamage(damage);
            break;
        }

        // Sit on the ground briefly then clean up
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        Destroy(gameObject, 2f);
    }
}
