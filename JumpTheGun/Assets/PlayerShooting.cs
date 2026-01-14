using UnityEngine;

public class PlayerShooting : MonoBehaviour
{

    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpd = 50f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject bulletObj = Instantiate(bulletPrefab, playerCamera.transform.position + playerCamera.transform.forward, transform.rotation);
            Rigidbody rb = bulletObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = playerCamera.transform.forward * bulletSpd; // Shoot it forward
            }
        }
    }
}
