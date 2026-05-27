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

    // Override in subclasses to play a sound each time one bullet/shell is loaded
    protected virtual void OnReloadBullet() { }

    // Override in subclasses to play a sound when the full reload sequence finishes
    protected virtual void OnReloadComplete() { }
    protected virtual void BulletSound() { }

    protected System.Collections.IEnumerator StartReloading()
    {
        isReloading = true;
        OnReloadBullet();

        while (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(reloadDelay);

            currentAmmo++;
            ammoText.text = currentAmmo.ToString();
            if (Input.GetMouseButtonUp(0)) break;
        }
       
        while(SoundManager.IsPlayingSound(SoundType.REVOLVER_RELOAD)) // Wait until the reload sound finishes before playing the chamber sound
        {
            Debug.Log("Pistol reload complete");
            yield return null;
        }
        // If we finish reloading the GUN, stop the reload sound effect
        
        OnReloadComplete();
        
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
