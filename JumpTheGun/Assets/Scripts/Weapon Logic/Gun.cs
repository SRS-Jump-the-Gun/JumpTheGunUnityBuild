using System.Collections;
using TMPro;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [SerializeField] protected Camera playerCamera;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected TMP_Text ammoText;
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected float reloadDelay;
    public GameObject collisionZone;
    protected int currentAmmo;
    protected bool isReloading = false;


    //May need to be fixed ------------------------------------
    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
        ammoText.text = currentAmmo.ToString();
    }

    protected System.Collections.IEnumerator StartReloading()
    {
        isReloading = true;

        // Reset knockback to original value as soon as we start loading the first shell
        //PlayerMovement._movement.setLeftClickAllowed(true);

        while (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(reloadDelay);

            currentAmmo++;
            ammoText.text = currentAmmo.ToString();

            // Do later play sound or animation here
            if (Input.GetMouseButtonUp(0)) break;
        }

        isReloading = false;
    }

    public IEnumerator SetCollisionZoneActive(float seconds)
    {
        collisionZone.SetActive(true);
        Debug.Log("Shooting collider active");

        yield return new WaitForSeconds(seconds);

        collisionZone.SetActive(false);
        Debug.Log("Collider off");

    }

    protected void ReloadLogic()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(StartReloading());
        }
    }
}
