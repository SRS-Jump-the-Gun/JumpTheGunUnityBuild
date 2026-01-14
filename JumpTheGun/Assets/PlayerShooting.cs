using UnityEngine;
using TMPro;

public class PlayerShooting : MonoBehaviour
{

    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] TMP_Text ammoText;

    [SerializeField] private float bulletSpd = 50f;
    [SerializeField] private int spawnCount = 12;
    [SerializeField] private float coneAngle = 15f;
    [SerializeField] private int maxAmmo = 2;
    [SerializeField] private float reloadDelay = 0.3f;
    private int currentAmmo;
    private bool isReloading = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        SpawnBurst();
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(StartReloading());
        }
        // idk why the launch speed resets weirdly without this
        if (currentAmmo > 0)
        { 
            PlayerMovement._movement.resetLaunchSpeed();
        }
    }

    private void SpawnBurst()
    {
        if(currentAmmo <= 0)
        {
            PlayerMovement._movement.noLaunchSpeed();
            return;
        }
        if (!Input.GetMouseButtonUp(0))
        {
            return;
        }
        Debug.Log(currentAmmo);
        Debug.Log(PlayerMovement._movement.getLaunchSpeed());
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
        ammoText.text = currentAmmo.ToString();
    }

    //not used yet - instant reload for testing
    private void InstantReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentAmmo = maxAmmo;
            ammoText.text = currentAmmo.ToString();
        }
    }

    private System.Collections.IEnumerator StartReloading()
    {
        isReloading = true;

        // Reset knockback to original value as soon as we start loading the first shell
        PlayerMovement._movement.resetLaunchSpeed();

        while (currentAmmo < maxAmmo)
        {
            // 1. Wait for the delay
            yield return new WaitForSeconds(reloadDelay);

            // 2. Add the ammo
            currentAmmo++;
            ammoText.text = currentAmmo.ToString();

            // 3. Play sound or animation here (optional)
            Debug.Log("Shell Inserted...");

            // FUTURE CANCEL LOGIC:
            // if (Input.GetButtonDown("Fire1")) break; 
        }

        isReloading = false;
    }
}
