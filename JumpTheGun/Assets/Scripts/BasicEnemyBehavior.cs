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
    public Transform[] destinations;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent.destination = destinations[currentDestination].position;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance <= 0.5f && !chasing)
        {
            agent.destination = destinations[getNextDestination(currentDestination)].position;
        }
        if (chasing)
        {
            chase();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Found player!");
            chase();
            chasing = true;
        }
    }

}
