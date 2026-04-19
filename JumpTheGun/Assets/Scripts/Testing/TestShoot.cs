using UnityEngine;

public class TestShoot : MonoBehaviour
{
    public Camera cam;
    public float range = 100f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Shoot!");

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log("Hit: " + hit.collider.name);

                EnemyBase enemy = hit.collider.GetComponentInParent<EnemyBase>();
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                    Debug.Log("Enemy killed");
                }
            }
        }
    }
}