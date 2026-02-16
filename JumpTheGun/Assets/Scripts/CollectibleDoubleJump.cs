using UnityEngine;

public class CollectibleDoubleJump : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        // Try to get the DoubleJump component from the player
        DoubleJump doubleJump = other.GetComponent<DoubleJump>();
        
        if (doubleJump != null)
        {
            // Enable double jump ability
            doubleJump.SetDoubleJumpEnabled(true);
            
            // Optional: Spawn pickup effect

            
            // Destroy the collectible
            Destroy(gameObject);
        }
    }
}