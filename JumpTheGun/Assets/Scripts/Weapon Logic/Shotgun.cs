using UnityEngine;
using TMPro;

public class Shotgun : Gun
{
    [SerializeField] private float bulletSpd = 50f;
    [SerializeField] private int bulletDamage = 15;
    [SerializeField] private int spawnCount = 12;
    [SerializeField] private float coneAngle = 15f;
    [SerializeField] private int shotgunAmmo = 2;
    [SerializeField] private float shotgunReloadDelay = 0.3f;
    [SerializeField] private float knockbackForce = 8f; // Recoil force applied to the player on fire
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
        if (currentAmmo <= 0)
        {
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
        // Play shotgun sound effect
        BulletSound();
        StartCoroutine(SetCollisionZoneActive(0.1f));

        // Cache colliders to ignore: player body + the shotgun cone zone itself
        Collider[] playerColliders = PlayerMovementForce.instance != null
            ? PlayerMovementForce.instance.GetComponents<Collider>()
            : new Collider[0];
        Collider coneCollider = shotgunCollisionZone != null
            ? shotgunCollisionZone.GetComponent<Collider>()
            : null;

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

            if (bulletObj.TryGetComponent<BulletLogic>(out var bl))
            {
                bl.isPlayerBullet = true;
                bl.damage = bulletDamage;
            }

            Collider bulletCol = bulletObj.GetComponent<Collider>();
            if (bulletCol != null)
            {
                foreach (Collider playerCol in playerColliders)
                    Physics.IgnoreCollision(bulletCol, playerCol);
                if (coneCollider != null)
                    Physics.IgnoreCollision(bulletCol, coneCollider);
            }

            Rigidbody rb = bulletObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = finalDirection * bulletSpd;
            }
        }
        // Push the player backwards opposite to the shoot direction
        if (PlayerMovementForce.instance != null)
        {
            Vector3 recoilDir = -playerCamera.transform.forward;
            PlayerMovementForce.instance.ApplyKnockback(recoilDir * knockbackForce);
        }

        // decrement ammo and update UI
        currentAmmo--;
        ammoText.text = currentAmmo.ToString();
    }

    protected override void OnReloadBullet()
    {
        SoundManager.PlaySound(SoundType.SHOTGUN_RELOAD);
    }
    protected override void BulletSound()
    {
        SoundManager.PlaySound(SoundType.SHOTGUN);
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
