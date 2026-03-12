using UnityEngine;
using TMPro;

public class Shotgun : Gun
{
    [SerializeField] private float bulletSpd = 50f;
    [SerializeField] private int spawnCount = 12;
    [SerializeField] private float coneAngle = 15f;
    [SerializeField] private int shotgunAmmo = 2;
    [SerializeField] private float shotgunReloadDelay = 0.3f;
    [SerializeField] public GameObject shotgunCollisionZone;
    [SerializeField] GameObject shotgunAsset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        //Shotgun statistics
        maxAmmo = shotgunAmmo;
        reloadDelay = shotgunReloadDelay;
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
        collisionZone = shotgunCollisionZone;
        shotgunAsset.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        SpawnBurst();
        ReloadLogic();
        //ShootAndReloadLogic();
    }

    //Shotgun projectile burst
    private void SpawnBurst()
    {
        if (currentAmmo <= 0) // If player doesnt have ammo, cant shoot
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
        StartCoroutine(SetCollisionZoneActive(0.1f));
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

    private void OnDisable()
    {
        shotgunCollisionZone.SetActive(false);
        shotgunAsset.SetActive(false);
    }
    private void OnEnable()
    {
        maxAmmo = shotgunAmmo;
        reloadDelay = shotgunReloadDelay;
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
        collisionZone = shotgunCollisionZone;
        shotgunAsset.SetActive(true);
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
