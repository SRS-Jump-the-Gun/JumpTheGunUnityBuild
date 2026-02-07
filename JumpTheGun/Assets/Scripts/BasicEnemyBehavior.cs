using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyBehavior : MonoBehaviour
{

    //TLDR the way this script works is you need game objects with transforms to set (in editor) as destinations for the enemy to walk to. If the player
    //ever enters the enemies sphere collider, it constantly goes after them... this also requires the player to have a collider which I added :^)

    //This script also assumes that there is an active navmesh surface for the enemy to move across, to do this in scene, just add a navmesh surface to the "plane"
    //and bake the nav mesh surface component. The enemy prefab already has a navmesh agent, so it should work like that.

    NavMeshAgent agent;
    int currentDestination = 0;
    bool chasing = false;

    [SerializeField] GameObject player;
    private IDamageable playerDamageable;
    [Header("Patrol")]
    public Transform[] destinations;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange = 5f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int damage = 10;

    [SerializeField] private bool stopToAttack = true; // Whether to stop moving during an attack
    private float nextAttackTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CachePlayerDamageable();
        if (destinations != null && destinations.Length > 0)
            agent.destination = destinations[currentDestination].position;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!chasing)
        {
            if (agent.remainingDistance <= 0.5f && !chasing)
            {
                agent.destination = destinations[getNextDestination(currentDestination)].position;
            }
            return;
        }
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist <= meleeRange)
        {
            TryMeleeAttack();
            if (stopToAttack)
            {
                agent.isStopped = true;
                FacePlayer();
            }
        }
        else
        {
            agent.isStopped = false;
            chase();
        }
    }

    void CachePlayerDamageable()
    {
        playerDamageable = null;
        if (player == null) return;

        playerDamageable = player.GetComponent<IDamageable>();
        if (playerDamageable == null)
        {
            playerDamageable = player.GetComponentInChildren<IDamageable>();
        }
    }

    int getNextDestination(int destination)
    {
        if (destination + 1 >= destinations.Length)
        {
            destination = 0;
            currentDestination = destination;
            return destination;
        }
        destination += 1;
        currentDestination = destination;
        return destination;
    }

    void chase()
    {
        agent.SetDestination(player.transform.position);
    }

    void FacePlayer()
    {
        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    void TryMeleeAttack()
    {
        if (Time.time < nextAttackTime) return;
        if (playerDamageable == null) CachePlayerDamageable();
        if (playerDamageable != null)
        {
            playerDamageable.TakeDamage(damage);
            Debug.Log($"Enemy melee hit for {damage}!");
        }
        else
        {
            Debug.LogWarning("Enemy tried to attack, but player has no IDamageable component.");
        }

        nextAttackTime = Time.time + attackCooldown;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Found player!");
            player = other.gameObject;
            CachePlayerDamageable();
            chasing = true;
            agent.isStopped = false;
            chase();
        }
    }

}
