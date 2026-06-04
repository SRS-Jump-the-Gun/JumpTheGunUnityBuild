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

    [Tooltip("Speed in units/second the debris falls. Lower = more dramatic and visible.")]
    [SerializeField] private float fallSpeed = 5f;

    [Tooltip("Child GameObject used as the warning shadow on the ground. Drag the WarningMarker child here.")]
    [SerializeField] private GameObject warningMarker;

    private Rigidbody rb;
    private bool hasFallen;
    private bool hasDealtDamage;
    private GameObject activeMarker; // reference after detaching from parent

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    private void OnDestroy()
    {
        // Clean up detached marker if debris is destroyed mid-warning
        if (activeMarker != null)
            Destroy(activeMarker);
    }

    /// <summary>
    /// Call this immediately after Instantiate to set the landing position.
    /// </summary>
    public void Initialize(Vector3 targetWorldPos)
    {
        // Cast downward from the player's position — ceiling is above the player so this
        // always finds the floor below rather than the roof above.
        Vector3 groundPos = targetWorldPos;
        if (Physics.Raycast(targetWorldPos, Vector3.down, out RaycastHit hit, 100f))
            groundPos.y = hit.point.y + 0.05f;
        else
            groundPos.y = 0f;

        // Clamp spawn height to ceiling so debris doesn't spawn inside the roof
        float actualHeight = spawnHeight;
        if (Physics.Raycast(groundPos, Vector3.up, out RaycastHit ceiling, spawnHeight + 1f))
            actualHeight = Mathf.Max(2f, ceiling.distance - 0.5f);

        transform.position = groundPos + Vector3.up * actualHeight;
        Debug.Log($"FallingDebris: Spawned at {transform.position} (height={actualHeight:F1})");


        if (warningMarker == null)
            Debug.LogWarning("FallingDebris: Warning Marker is not assigned — no visual warning will appear.", this);

        if (warningMarker != null)
        {
            warningMarker.transform.SetParent(null);
            warningMarker.transform.position = groundPos;
            warningMarker.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            warningMarker.SetActive(true);
            activeMarker = warningMarker;
        }

        StartCoroutine(DropSequence(groundPos));
    }

    private IEnumerator DropSequence(Vector3 landPos)
    {
        float t = 0f;
        bool markerVisible = true;

        while (t < warningDuration)
        {
            t += Time.deltaTime;

            if (activeMarker != null)
            {
                float progress = t / warningDuration;
                bool shouldShow;

                if (progress < 0.65f)
                {
                    // Solid on for the first 65% so the player clearly sees the marker
                    shouldShow = true;
                }
                else
                {
                    // Rapid blink for the final 35% as a "drop imminent" signal
                    float blinkSpeed = Mathf.Lerp(8f, 28f, (progress - 0.65f) / 0.35f);
                    shouldShow = Mathf.Sin(t * blinkSpeed) >= 0f;
                }

                if (shouldShow != markerVisible)
                {
                    markerVisible = shouldShow;
                    activeMarker.SetActive(markerVisible);
                }
            }

            yield return null;
        }

        if (activeMarker != null)
        {
            Destroy(activeMarker);
            activeMarker = null;
        }

        // Fall at controlled speed so the player can see it coming
        rb.isKinematic = true;
        rb.useGravity  = false;
        hasFallen      = true;

        float targetY = landPos.y;
        while (transform.position.y > targetY + 0.05f)
        {
            float newY = Mathf.MoveTowards(transform.position.y, targetY, fallSpeed * Time.deltaTime);
            rb.MovePosition(new Vector3(transform.position.x, newY, transform.position.z));
            yield return null;
        }

        // Snap to ground and deal impact damage
        rb.MovePosition(new Vector3(transform.position.x, targetY, transform.position.z));

        if (!hasDealtDamage)
        {
            hasDealtDamage = true;
            Collider[] nearby = Physics.OverlapSphere(transform.position, impactRadius);
            foreach (Collider col in nearby)
            {
                if (!col.CompareTag("Player")) continue;
                IDamageable d = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();
                d?.TakeDamage(damage);
                break;
            }
        }

        Destroy(gameObject, 2f);
    }
}
