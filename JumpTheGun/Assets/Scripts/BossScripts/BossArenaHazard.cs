using UnityEngine;

/// <summary>
/// Manages all environmental hazards in the boss arena.
/// BossEnemy calls TriggerRandomHazard() and TriggerShockwave() here.
/// Assign this component to an empty "Arena Hazard Manager" GameObject in the scene.
///
/// Setup in Inspector:
///   - shockwavePrefab: a flat disc with GroundShockwave + trigger Collider
///   - debrisPrefab:    a rigid-body object with FallingDebris script
///   - debrisAnchorPoints: empty Transforms placed at the ceiling where debris can fall from
///     (the debris is spawned above the player, but anchors restrict valid drop zones)
/// </summary>
public class BossArenaHazard : MonoBehaviour
{
    [Header("Hazard Prefabs")]
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private GameObject debrisPrefab;

    [Header("Debris Drop Zones")]
    [Tooltip("Ceiling anchor Transforms. Debris will fall from above one of these positions.")]
    [SerializeField] private Transform[] debrisAnchors;

    [Tooltip("How far the debris target can deviate from the player's position (adds unpredictability).")]
    [SerializeField] private float debrisTargetJitter = 2f;

    [Header("Phase Escalation")]
    [Tooltip("Phase 1: only debris. Phase 2: debris + shockwave. Phase 3: both, more often.")]
    private BossEnemy.BossPhase currentPhase = BossEnemy.BossPhase.Phase1_Projectile;

    // Called by BossEnemy when the phase changes
    public void OnPhaseChanged(BossEnemy.BossPhase newPhase)
    {
        currentPhase = newPhase;
    }

    /// <summary>
    /// Picks a random hazard appropriate for the current phase and triggers it.
    /// playerPos is used to aim debris near the player.
    /// </summary>
    public void TriggerRandomHazard(BossEnemy.BossPhase phase, Vector3 playerPos)
    {
        currentPhase = phase;

        switch (phase)
        {
            case BossEnemy.BossPhase.Phase1_Projectile:
                // Only falling debris in phase 1
                SpawnDebris(playerPos);
                break;

            case BossEnemy.BossPhase.Phase2_Sniper:
                // Mix: 60% debris, 40% shockwave (shockwave spawns at arena center)
                if (Random.value < 0.6f)
                    SpawnDebris(playerPos);
                else
                    SpawnShockwaveAtRandomAnchor();
                break;

            case BossEnemy.BossPhase.Phase3_Melee:
                // All hazards together in phase 3
                SpawnDebris(playerPos);
                if (Random.value < 0.5f)
                    SpawnShockwaveAtRandomAnchor();
                break;
        }
    }

    /// <summary>
    /// Directly triggers a shockwave at the given world position (called by boss melee slam).
    /// </summary>
    public void TriggerShockwave(Vector3 worldPos)
    {
        if (shockwavePrefab == null) return;
        Instantiate(shockwavePrefab, worldPos, Quaternion.Euler(0f, 0f, 0f));
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────
    private void SpawnDebris(Vector3 playerPos)
    {
        if (debrisPrefab == null) return;

        // Pick a drop anchor if available; otherwise drop straight above the player
        Vector3 targetPos = playerPos;

        if (debrisAnchors != null && debrisAnchors.Length > 0)
        {
            // Find the anchor closest to the player and use its XZ position
            Transform closest = debrisAnchors[0];
            float minDist = float.MaxValue;
            foreach (Transform anchor in debrisAnchors)
            {
                float d = Vector3.Distance(anchor.position, playerPos);
                if (d < minDist) { minDist = d; closest = anchor; }
            }
            targetPos = new Vector3(closest.position.x, playerPos.y, closest.position.z);
        }

        // Add jitter so it isn't a perfect always-avoidable pattern
        targetPos.x += Random.Range(-debrisTargetJitter, debrisTargetJitter);
        targetPos.z += Random.Range(-debrisTargetJitter, debrisTargetJitter);
        targetPos.y  = playerPos.y; // Keep Y at player's floor level

        GameObject obj = Instantiate(debrisPrefab);
        FallingDebris fd = obj.GetComponent<FallingDebris>();
        if (fd != null)
            fd.Initialize(targetPos);
        else
            Destroy(obj); // Safety — missing script
    }

    private void SpawnShockwaveAtRandomAnchor()
    {
        if (shockwavePrefab == null || debrisAnchors == null || debrisAnchors.Length == 0)
        {
            // Fall back to transform position if no anchors defined
            TriggerShockwave(transform.position);
            return;
        }

        Transform anchor = debrisAnchors[Random.Range(0, debrisAnchors.Length)];
        // Project anchor XZ to the floor (Y = 0 relative to scene floor)
        Vector3 pos = new Vector3(anchor.position.x, 0f, anchor.position.z);
        TriggerShockwave(pos);
    }
}
