using UnityEngine;

public class SniperBehavior : MonoBehaviour
{   
    [SerializeField] Transform player;
    public float viewDistance = 1000f;
    public LineRenderer laserLine; 
    public LayerMask obstacleLayer;
    
    [Range(0, 1)] 
    public float dotThreshold = 0.5f; // 0.5 is roughly a 60-degree cone

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 startPos = transform.position + Vector3.up * 1.0f;
        Vector3 endPos = player.position + Vector3.up * 0.5f;
        Vector3 dirToPlayer = (endPos - startPos).normalized;

        // --- NEW: Front-Facing Check ---
        // Compare Sniper's Forward vector with the Direction to Player
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (dot > dotThreshold) 
        {
            // Player is in front! Enable laser and do logic
            laserLine.enabled = true;
            UpdateLaser(startPos, endPos, dirToPlayer);
        }
        else 
        {
            // Player is behind or to the side! Hide the laser
            laserLine.enabled = false;
        }
    }

    void UpdateLaser(Vector3 startPos, Vector3 endPos, Vector3 dir)
    {
        laserLine.SetPosition(0, startPos);
        RaycastHit hit;
        float dist = Vector3.Distance(startPos, endPos);

        if (Physics.Raycast(startPos, dir, out hit, viewDistance, obstacleLayer))
        {
            if (hit.transform == player)
            {
                laserLine.SetPosition(1, endPos);
            }
            else
            {
                laserLine.SetPosition(1, hit.point);
            }
        }
        else
        {
            laserLine.SetPosition(1, endPos);
        }
    }
}