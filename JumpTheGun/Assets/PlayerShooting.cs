using UnityEngine;

public class PlayerShooting : MonoBehaviour
{

    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpd = 50f;
    [SerializeField] private int spawnCount = 12;
    [SerializeField] private float coneAngle = 15f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SpawnBurst();   
    }

    private void SpawnBurst()
    {
        if (!Input.GetMouseButtonUp(0))
        {
            return;
        }
        for (int i = 0; i < spawnCount; i++)
        {
            Quaternion spread = Quaternion.Euler(
                Random.Range(-coneAngle, coneAngle),
                Random.Range(-coneAngle, coneAngle),
                Random.Range(-coneAngle, coneAngle)
            );

            Vector3 finalDirection = spread * playerCamera.transform.forward;

            GameObject bulletObj = Instantiate(bulletPrefab, playerCamera.transform.position + playerCamera.transform.forward, Quaternion.LookRotation(finalDirection));

            // 4. Apply Velocity
            Rigidbody rb = bulletObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = finalDirection * bulletSpd;
            }
        }
    }
}
