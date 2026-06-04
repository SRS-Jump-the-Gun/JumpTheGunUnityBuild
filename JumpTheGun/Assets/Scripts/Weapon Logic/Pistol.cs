using UnityEngine;

public class Pistol : Gun
{
    [SerializeField] private float bulletSpd = 50f;
    [SerializeField] private int bulletDamage = 20;
    [SerializeField] private int pistolAmmo = 6;
    [SerializeField] private float pistolReloadDelay = 0.2f;
    [SerializeField] public GameObject pistolCollision;
    [SerializeField] GameObject pistolAsset;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        //Pistol statistics
        maxAmmo = pistolAmmo;
        reloadDelay = pistolReloadDelay;
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
        collisionZone = pistolCollision;
        pistolAsset.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        ShootingLogic();
        ReloadLogic();
    }

    private void ShootingLogic()
    {
        if (Input.GetMouseButtonDown(0) && currentAmmo > 0 && !isReloading)
        {
            StartCoroutine(SetCollisionZoneActive(0.1f));
            Debug.Log("Shooting!");
            currentAmmo--;
            ammoText.text = currentAmmo.ToString();

            spawnBullet();
        }


    }

    private void OnEnable()
    {
        maxAmmo = pistolAmmo;
        reloadDelay = pistolReloadDelay;
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
        collisionZone = pistolCollision;
        pistolAsset.SetActive(true);

    }

    protected override void OnReloadBullet()
    {
        SoundManager.PlaySound(SoundType.REVOLVER_RELOAD);
    }

    protected override void OnReloadComplete()
    {
       SoundManager.PlaySound(SoundType.REVOLVER_CHAMBER);
        Debug.Log("Pistol relOAD OTHER COMPLETE");
        
    }

    private void OnDisable()
    {
        pistolAsset.SetActive(false);
    }

    private void spawnBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, playerCamera.transform.position + playerCamera.transform.forward, Quaternion.LookRotation(playerCamera.transform.forward));

        if (bullet.TryGetComponent<BulletLogic>(out var bl))
        {
            bl.isPlayerBullet = true;
            bl.damage = bulletDamage;
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = playerCamera.transform.forward * bulletSpd;
    }
}
