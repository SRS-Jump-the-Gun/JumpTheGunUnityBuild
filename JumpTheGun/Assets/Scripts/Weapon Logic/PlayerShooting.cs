using UnityEngine;
using TMPro;

public class PlayerShooting : Gun
{

    [SerializeField] private float bulletSpd = 50f;
    [SerializeField] private int spawnCount = 12;
    [SerializeField] private float coneAngle = 15f;

    //Figure out how to fix these and make it so we SET them here!
    
    //[SerializeField] protected int maxAmmo = 2;
    //[SerializeField] protected float reloadDelay = 0.3f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // IF you shotgun equped 
        SpawnBurst();
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(StartReloading());
        }
        // idk why the launch speed resets weirdly without this
        if (currentAmmo > 0)
        { 
            PlayerMovement._movement.setLeftClickAllowed(true);
        }

        // if pistol equipped, just shoot one bullet with no spread and instant reload
    }

    //Shotgun projectile burst
    private void SpawnBurst()
    {
        if(currentAmmo <= 0) // If player doesnt have ammo, cant shoot
        {
            PlayerMovement._movement.setLeftClickAllowed(false);
            return;
        }
        if (isReloading)    // If player is currently reloading, cant shoot
        {
            return;
        }
        if (!Input.GetMouseButtonDown(0)) // If player is not pressing left click, dont shoot
        {
            return;
        }
        for (int i = 0; i < spawnCount; i++)
        {
            // random rotation within cone angle for shotgun spread
            Quaternion spread = Quaternion.Euler(
                Random.Range(-coneAngle, coneAngle),
                Random.Range(-coneAngle, coneAngle),
                Random.Range(-coneAngle, coneAngle)
            );

            Vector3 finalDirection = spread * playerCamera.transform.forward;

            GameObject bulletObj = Instantiate(bulletPrefab, playerCamera.transform.position + playerCamera.transform.forward, Quaternion.LookRotation(finalDirection));

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

}
