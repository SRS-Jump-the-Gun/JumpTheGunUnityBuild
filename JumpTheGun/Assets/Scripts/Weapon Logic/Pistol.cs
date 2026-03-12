using UnityEngine;

public class Pistol : Gun
{
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

    private void OnDisable()
    {
        pistolAsset.SetActive(false);
    }
}
