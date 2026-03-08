using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;

    void Start()
    {
        // Destroy the bullet after a few seconds so they don't clutter the scene
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move forward every frame
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle hitting the player or walls
        Debug.Log("Hit: " + other.name);
        Destroy(gameObject); 
    }
}