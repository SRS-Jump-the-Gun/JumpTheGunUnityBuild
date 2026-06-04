using UnityEngine;

public class zoneDetector : MonoBehaviour
{
    [SerializeField] private GameObject pistol;
    [SerializeField] private int damage = 20;

    void Update() { }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Debug.Log("HITTING " + other.name);

        // Enemies that implement IDamageable (e.g. the boss) take HP damage.
        // Regular enemies without IDamageable are destroyed instantly.
        IDamageable damageable = other.GetComponent<IDamageable>()
                              ?? other.GetComponentInParent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(damage);
        else
            Destroy(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        IDamageable damageable = other.GetComponent<IDamageable>()
                              ?? other.GetComponentInParent<IDamageable>();
        if (damageable == null)
            Destroy(other.gameObject);
        // IDamageable targets are only hit in OnTriggerEnter to avoid continuous damage per frame.
    }
}
