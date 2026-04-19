using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Spawns a randomized group of enemies when the player enters a trigger zone.
/// Enemies are selected from a weighted pool and placed at predefined Spawn Points.
/// Tracks when all enemies are defeated to unlock doors or activate rewards.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        [Tooltip("Enemy prefab to spawn.")]
        public GameObject prefab;

        [Tooltip("Spawn weight. Higher = more likely. (e.g. Sniper=70, Melee=20, Projectile=10)")]
        public float weight = 1f;
    }

    [Header("Enemy Pool")]
    [SerializeField] private List<EnemyEntry> enemyPool = new List<EnemyEntry>();

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Count")]
    [SerializeField] private int minEnemies = 2;
    [SerializeField] private int maxEnemies = 5;

    [Header("Settings")]
    [SerializeField] private bool spawnOnce = true;
    [SerializeField] private float navMeshSampleDistance = 3f;
    [SerializeField] private float enemySpeed = 3f;

    [Header("Clear Condition")]
    [Tooltip("Doors to open (deactivate) when all enemies are defeated.")]
    [SerializeField] private GameObject[] doorsToOpen;

    [Tooltip("Rewards to activate when all enemies are defeated.")]
    [SerializeField] private GameObject[] rewardsToActivate;

    private bool hasSpawned = false;
    private List<GameObject> activeEnemies = new List<GameObject>();

    /// <summary>
    /// Detects when the player enters the trigger volume attached to this GameObject.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (spawnOnce && hasSpawned) return;

        hasSpawned = true;
        SpawnEnemies();
    }

    /// <summary>
    /// Picks a random number of enemies, places them at shuffled Spawn Points,
    /// and configures their NavMeshAgents.
    /// </summary>
    private void SpawnEnemies()
    {
        if (enemyPool == null || enemyPool.Count == 0) { Debug.LogWarning("EnemySpawner: Enemy Pool is empty!"); return; }
        if (spawnPoints == null || spawnPoints.Length == 0) { Debug.LogWarning("EnemySpawner: No Spawn Points assigned!"); return; }

        activeEnemies.Clear();

        // Shuffle spawn points so enemies don't always appear in the same spots
        List<Transform> shuffled = new List<Transform>(spawnPoints);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int count = Random.Range(minEnemies, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            Transform point = shuffled[i % shuffled.Count];

            // NavMesh.SamplePosition finds the nearest valid point on the baked NavMesh.
            // This prevents enemies from spawning inside obstacles or off-cliffs.
            if (!NavMesh.SamplePosition(point.position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                Debug.LogWarning($"EnemySpawner: No NavMesh found near {point.name}, skipping.");
                continue;
            }

            EnemyEntry entry = PickWeightedRandom();
            if (entry == null || entry.prefab == null) continue;
 
            GameObject enemy = Instantiate(entry.prefab, hit.position, Quaternion.identity);
 
            NavMeshAgent nav = enemy.GetComponent<NavMeshAgent>();
            if (nav != null)
            {
                nav.speed = enemySpeed;
                // Warp immediately places the agent at the position (more reliable than transform.position)
                nav.Warp(hit.position);
                nav.ResetPath();
                nav.isStopped = false;
            }

            activeEnemies.Add(enemy);
            Debug.Log($"EnemySpawner: Spawned [{entry.prefab.name}] at {point.name} (NavMesh pos: {hit.position})");
        }

        Debug.Log($"EnemySpawner: Spawned {activeEnemies.Count} enemies.");
    }

    /// <summary>
    /// Checks every frame if all tracked enemies have been destroyed.
    /// </summary>
    private void Update()
    {
        if (!hasSpawned || activeEnemies.Count == 0) return;

        // Destroyed GameObjects become null — remove them from the list
        activeEnemies.RemoveAll(e => e == null);

        if (activeEnemies.Count == 0)
            OnRoomCleared();
    }

    /// <summary>
    /// Called once all enemies are defeated. Opens doors and activates rewards.
    /// </summary>
    private void OnRoomCleared()
    {
        Debug.Log("EnemySpawner: All enemies defeated! Room cleared.");

        if (doorsToOpen != null)
            foreach (var door in doorsToOpen)
                if (door != null) door.SetActive(false);

        if (rewardsToActivate != null)
            foreach (var reward in rewardsToActivate)
                if (reward != null) reward.SetActive(true);
    }

    /// <summary>
    /// Rolls a random number against the cumulative weights in the Enemy Pool
    /// and returns the selected prefab.
    /// </summary>
    private EnemyEntry PickWeightedRandom()
    {
        float total = 0f;
        foreach (var entry in enemyPool) total += entry.weight;
 
        float roll = Random.Range(0f, total);
        float cumulative = 0f;
 
        foreach (var entry in enemyPool)
        {
            cumulative += entry.weight;
            if (roll <= cumulative) return entry;
        }
 
        return enemyPool[enemyPool.Count - 1];
    }
}