using UnityEngine;
using TMPro;

public class PlayerShooting : MonoBehaviour
{

    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] TMP_Text ammoCount;

    [SerializeField] private float bulletSpd = 50f;
    [SerializeField] private int spawnCount = 12;
    [SerializeField] private float coneAngle = 15f;
    [SerializeField] private int maxAmmo = 2;
    [SerializeField] private float reloadDelay = 0.2f;
    private int currentAmmo;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = maxAmmo;
        ammoCount.text = currentAmmo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        SpawnBurst();
        StaggeredReload();
    }

    private void SpawnBurst()
    {
        if(currentAmmo <= 0)
        {
            PlayerMovement._movement.setLaunchSpeed(0f);
            return;
        }
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
        // decrement ammo and update UI
        currentAmmo--;
        ammoCount.text = currentAmmo.ToString();
    }

    private void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentAmmo = maxAmmo;
            ammoCount.text = currentAmmo.ToString();
        }
    }

    private void StaggeredReload()
    {
        //future plans to make it cancelable
        if (Input.GetKeyDown(KeyCode.R))
        {
            while(currentAmmo < maxAmmo)
                StartCoroutine(startReloading(reloadDelay));
        }
    }

    private System.Collections.IEnumerator startReloading(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentAmmo++;
        ammoCount.text = currentAmmo.ToString();
    }
}
