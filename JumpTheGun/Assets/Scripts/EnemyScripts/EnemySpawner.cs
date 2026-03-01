using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private bool spawnOnce = true;
    [SerializeField] private float spEnemySpeed = 3f;

    [Header("NavMesh")]
    [SerializeField] private float sampleDistance = 10f;

    private bool hasSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (spawnOnce && hasSpawned) return;

        hasSpawned = true;
        SpawnAroundPlayer(other.transform);
    }

    private void SpawnAroundPlayer(Transform playerTf)
    {
        if (enemyPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 offset2D = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 desired = playerTf.position + new Vector3(offset2D.x, 0f, offset2D.y);

            if (!NavMesh.SamplePosition(desired, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
                continue;

            Vector3 spawnPos = hit.position;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            NavMeshAgent a = enemy.GetComponent<NavMeshAgent>();
            if (a != null)
            {
                a.speed = spEnemySpeed; 
                a.Warp(spawnPos);
                a.ResetPath();
                a.isStopped = false;
            }
        }
    }
}