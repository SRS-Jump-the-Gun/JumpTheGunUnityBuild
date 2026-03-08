using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawns a group of enemies around the player when the player enters a trigger zone.
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("The enemy object to be spawned.")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Number of enemies to create per trigger.")]
    [SerializeField] private int spawnCount = 3;

    [Tooltip("How far from the player the enemies should appear.")]
    [SerializeField] private float spawnRadius = 5f;

    [Tooltip("If true, this spawner only works once and then deactivates.")]
    [SerializeField] private bool spawnOnce = true;

    [Tooltip("Sets the movement speed of the spawned enemies' NavMeshAgents.")]
    [SerializeField] private float spEnemySpeed = 3f;

    [Header("NavMesh")]
    [Tooltip("How far to look for a valid NavMesh point if the desired spawn spot is blocked.")]
    [SerializeField] private float sampleDistance = 10f;

    private bool hasSpawned = false;

    /// <summary>
    /// Detects when the player enters the trigger volume attached to this GameObject.
    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if the object has the "Player" tag
        if (!other.CompareTag("Player")) return;
        
        // Stop if we've already spawned and 'spawnOnce' is enabled
        if (spawnOnce && hasSpawned) return;

        hasSpawned = true;
        SpawnAroundPlayer(other.transform);
    }

    /// <summary>
    /// Calculates random positions around the player and places enemies on the NavMesh.
    private void SpawnAroundPlayer(Transform playerTf)
    {
        if (enemyPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            // Create a random point on a 2D circle and convert it to 3D space
            Vector2 offset2D = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 desired = playerTf.position + new Vector3(offset2D.x, 0f, offset2D.y);

            // NavMesh.SamplePosition finds the nearest valid point on the baked NavMesh.
            // This prevents enemies from spawning inside static obstacles or off-cliffs.
            if (!NavMesh.SamplePosition(desired, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
                continue; // Skip this enemy if no valid ground is found nearby

            Vector3 spawnPos = hit.position;

            // Create the enemy instance
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Configure the NavMeshAgent component on the new enemy
            NavMeshAgent a = enemy.GetComponent<NavMeshAgent>();
            if (a != null)
            {
                a.speed = spEnemySpeed; 
                
                // Warp immediately places the agent at the position (more reliable than transform.position)
                a.Warp(spawnPos);
                
                // Clear any old paths and ensure the agent is active
                a.ResetPath();
                a.isStopped = false;
            }
        }
    }
}