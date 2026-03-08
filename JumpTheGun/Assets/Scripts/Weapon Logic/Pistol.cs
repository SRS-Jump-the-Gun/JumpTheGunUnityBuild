using UnityEngine;

public class Pistol : Gun
{
    [SerializeField] private int pistolAmmo = 6;
    [SerializeField] private float pistolReloadDelay = 0.2f;
    [SerializeField] private GameObject pistolCollision;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        //Pistol statistics
        maxAmmo = pistolAmmo;
        reloadDelay = pistolReloadDelay;
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
        collisionZone = pistolCollision;
    }

    // Update is called once per frame
    void Update()
    {
        ShootingLogic();
    }

    private void ShootingLogic()
    {
        if (Input.GetMouseButtonDown(0) && currentAmmo > 0 && !isReloading)
        {
            pistolCollision.SetActive(true);

        }

    }
}
