using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(DestroyBullet(2.5f));
    }

    private System.Collections.IEnumerator DestroyBullet(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
