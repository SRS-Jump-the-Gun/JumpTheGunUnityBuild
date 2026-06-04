using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Boss enemy combining all three enemy archetypes.
/// Phase 1 (>66% HP):  Projectile barrage — rapid bursts from medium range.
/// Phase 2 (33-66%):   Sniper lock-on + occasional projectiles from long range.
/// Phase 3 (<33%):     Berserk melee — charge attacks + all previous attacks combined.
/// Environmental hazards escalate each phase via BossArenaHazard.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BossEnemy : MonoBehaviour, IDamageable
{
    public enum BossPhase { Phase1_Projectile, Phase2_Sniper, Phase3_Melee }

    // ─────────────────────────────────────────
    //  Health & Phase
    // ─────────────────────────────────────────
    [Header("Health")]
    [SerializeField] private int maxHP = 600;
    private int currentHP;
    private BossPhase currentPhase = BossPhase.Phase1_Projectile;

    // ─────────────────────────────────────────
    //  References
    // ─────────────────────────────────────────
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private BossArenaHazard arenaHazard;
    private IDamageable playerDamageable;
    private NavMeshAgent agent;

    // ─────────────────────────────────────────
    //  Movement
    // ─────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float normalSpeed = 4f;
    [SerializeField] private float berserkerSpeed = 7f;
    [SerializeField] private float chaseRange = 30f;
    // Preferred combat distances per phase
    [SerializeField] private float phase1PreferredDist = 10f;
    [SerializeField] private float phase2PreferredDist = 18f;
    [SerializeField] private float meleeSlamRange = 3f;

    // ─────────────────────────────────────────
    //  Phase 1 – Projectile
    // ─────────────────────────────────────────
    [Header("Projectile Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileBurstCooldown = 4f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.25f;
    [SerializeField] private LayerMask obstacleLayer;
    private float nextProjectileTime;

    // ─────────────────────────────────────────
    //  Phase 2 – Sniper
    // ─────────────────────────────────────────
    [Header("Sniper Attack")]
    [SerializeField] public LineRenderer laserLine;
    [SerializeField] private float timeToLock = 2.5f;
    [SerializeField] private int sniperDamage = 40;
    private float lockOnTimer;

    // ─────────────────────────────────────────
    //  Phase 3 – Melee
    // ─────────────────────────────────────────
    [Header("Melee Attack")]
    [SerializeField] private int meleeDamage = 25;
    [SerializeField] private int chargeDamage = 50;
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float chargeWindup = 0.8f;
    [SerializeField] private float chargeImpactRadius = 3f;
    private float nextMeleeTime;
    private float nextChargeTime;
    private bool isCharging;

    // ─────────────────────────────────────────
    //  Environmental Hazard timing
    // ─────────────────────────────────────────
    [Header("Environmental Hazards")]
    [SerializeField] private float phase1HazardCooldown = 10f;
    [SerializeField] private float phase2HazardCooldown = 7f;
    [SerializeField] private float phase3HazardCooldown = 4f;
    private float nextHazardTime;

    // ─────────────────────────────────────────
    //  Events — subscribe in BossHealthBar / BossManager
    // ─────────────────────────────────────────
    public System.Action<int, int> OnHealthChanged;   // (currentHP, maxHP)
    public System.Action<BossPhase> OnPhaseChanged;
    public System.Action OnBossDefeated;

    // ─────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────
    private void Awake() => agent = GetComponent<NavMeshAgent>();

    private void Start()
    {
        currentHP = maxHP;

        if (player == null) player = GameObject.FindWithTag("Player");
        if (player != null) playerDamageable = player.GetComponentInParent<IDamageable>();

        agent.speed = normalSpeed;
        if (laserLine != null) laserLine.enabled = false;

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    // ─────────────────────────────────────────
    //  Main Loop
    // ─────────────────────────────────────────
    private void Update()
    {
        if (player == null || !agent.isOnNavMesh || isCharging) return;

        EvaluatePhaseTransition();

        float dist = Vector3.Distance(transform.position, player.transform.position);
        FacePlayer();

        switch (currentPhase)
        {
            case BossPhase.Phase1_Projectile: UpdatePhase1(dist); break;
            case BossPhase.Phase2_Sniper:     UpdatePhase2(dist); break;
            case BossPhase.Phase3_Melee:      UpdatePhase3(dist); break;
        }

        TickHazard();
    }

    // ─────────────────────────────────────────
    //  Phase Logic
    // ─────────────────────────────────────────
    private void UpdatePhase1(float dist)
    {
        // Maintain preferred distance from the player
        MaintainDistance(phase1PreferredDist, 4f);

        if (Time.time >= nextProjectileTime)
        {
            StartCoroutine(FireBurst(burstCount));
            nextProjectileTime = Time.time + projectileBurstCooldown;
        }
    }

    private void UpdatePhase2(float dist)
    {
        // Stay at long range so the laser shot matters
        MaintainDistance(phase2PreferredDist, 5f);

        HandleSniperLaser();

        // Pepper the player with projectiles while the laser isn't locking on
        if (Time.time >= nextProjectileTime && lockOnTimer < 0.2f)
        {
            StartCoroutine(FireBurst(1));
            nextProjectileTime = Time.time + projectileBurstCooldown * 0.75f;
        }
    }

    private void UpdatePhase3(float dist)
    {
        // Charge first, then close-range slam
        if (dist > meleeSlamRange + 2f && dist < 18f && Time.time >= nextChargeTime)
        {
            StartCoroutine(ChargeAttack());
            nextChargeTime = Time.time + 5f;
            return;
        }

        // Close enough — slam
        if (dist <= meleeSlamRange && Time.time >= nextMeleeTime)
        {
            MeleeSlam();
            nextMeleeTime = Time.time + 1.2f;
        }
        else if (dist > meleeSlamRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }

        // Also fires projectiles and laser in phase 3
        if (Time.time >= nextProjectileTime)
        {
            StartCoroutine(FireBurst(2));
            nextProjectileTime = Time.time + projectileBurstCooldown * 0.5f;
        }

        HandleSniperLaser();
    }

    // ─────────────────────────────────────────
    //  Projectile Attack
    // ─────────────────────────────────────────
    private IEnumerator FireBurst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            FireProjectile();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        Vector3 dir = (GetPlayerCenter() - firePoint.position).normalized;
        // Small spread so bursts feel dangerous but dodgeable
        dir += Random.insideUnitSphere * 0.04f;
        dir.Normalize();

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        // Prevent projectile from hitting the boss itself
        if (proj.TryGetComponent<Collider>(out var projCol))
            foreach (Collider myCol in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projCol, myCol);
    }

    // ─────────────────────────────────────────
    //  Sniper Laser
    // ─────────────────────────────────────────
    private void HandleSniperLaser()
    {
        if (laserLine == null || firePoint == null || player == null) return;

        Vector3 dir = (GetPlayerCenter() - firePoint.position).normalized;
        float dot = Vector3.Dot(transform.forward, dir);

        if (dot < 0.6f)
        {
            DisableLaser();
            return;
        }

        laserLine.enabled = true;
        laserLine.SetPosition(0, firePoint.position);

        if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, 40f, obstacleLayer))
        {
            laserLine.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Player"))
            {
                lockOnTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(lockOnTimer / timeToLock);
                laserLine.startWidth = Mathf.Lerp(0.04f, 0.45f, progress);
                laserLine.endWidth   = Mathf.Lerp(0.04f, 0.45f, progress);

                if (lockOnTimer >= timeToLock)
                {
                    playerDamageable?.TakeDamage(sniperDamage);
                    DisableLaser();
                }
            }
            else
            {
                DisableLaser();
            }
        }
        else
        {
            laserLine.SetPosition(1, firePoint.position + dir * 40f);
            DisableLaser();
        }
    }

    private void DisableLaser()
    {
        lockOnTimer = 0f;
        if (laserLine != null)
        {
            laserLine.startWidth = 0.04f;
            laserLine.endWidth   = 0.04f;
            laserLine.enabled    = false;
        }
    }

    // ─────────────────────────────────────────
    //  Melee Attacks
    // ─────────────────────────────────────────
    private void MeleeSlam()
    {
        agent.isStopped = true;
        if (playerDamageable != null)
            playerDamageable.TakeDamage(meleeDamage);

        // Trigger shockwave through the arena hazard system
        arenaHazard?.TriggerShockwave(transform.position);
    }

    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        agent.isStopped = true;

        // Telegraph — hold still for windup
        yield return new WaitForSeconds(chargeWindup);

        if (player == null) { isCharging = false; yield break; }

        Vector3 chargeTarget = player.transform.position;
        agent.speed = chargeSpeed;
        agent.isStopped = false;
        agent.SetDestination(chargeTarget);

        float timeout = 2.5f;
        while (timeout > 0f)
        {
            timeout -= Time.deltaTime;
            if (Vector3.Distance(transform.position, chargeTarget) < 2.5f)
            {
                // Deal impact damage if player is still nearby
                if (player != null && Vector3.Distance(transform.position, player.transform.position) < chargeImpactRadius)
                    playerDamageable?.TakeDamage(chargeDamage);
                break;
            }
            yield return null;
        }

        agent.speed = berserkerSpeed;
        isCharging = false;
    }

    // ─────────────────────────────────────────
    //  Environmental Hazards
    // ─────────────────────────────────────────
    private void TickHazard()
    {
        if (arenaHazard == null || Time.time < nextHazardTime) return;

        float cooldown = currentPhase switch
        {
            BossPhase.Phase1_Projectile => phase1HazardCooldown,
            BossPhase.Phase2_Sniper     => phase2HazardCooldown,
            _                           => phase3HazardCooldown,
        };

        nextHazardTime = Time.time + cooldown;
        arenaHazard.TriggerRandomHazard(currentPhase, player != null ? player.transform.position : transform.position);
    }

    // ─────────────────────────────────────────
    //  Phase Transition
    // ─────────────────────────────────────────
    private void EvaluatePhaseTransition()
    {
        float pct = (float)currentHP / maxHP;
        BossPhase target = pct > 0.66f ? BossPhase.Phase1_Projectile
                         : pct > 0.33f ? BossPhase.Phase2_Sniper
                                       : BossPhase.Phase3_Melee;

        if (target == currentPhase) return;

        currentPhase = target;

        if (currentPhase == BossPhase.Phase3_Melee)
            agent.speed = berserkerSpeed;

        if (laserLine != null) laserLine.enabled = false;
        lockOnTimer = 0f;

        arenaHazard?.OnPhaseChanged(currentPhase);
        OnPhaseChanged?.Invoke(currentPhase);
    }

    // ─────────────────────────────────────────
    //  IDamageable — player weapons call this
    // ─────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHP <= 0) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP == 0)
        {
            OnBossDefeated?.Invoke();
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────
    private void MaintainDistance(float preferred, float tolerance)
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist < preferred - tolerance)
        {
            // Back away
            Vector3 awayDir = (transform.position - player.transform.position).normalized;
            agent.isStopped = false;
            agent.SetDestination(transform.position + awayDir * 4f);
        }
        else if (dist > chaseRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void FacePlayer()
    {
        if (isCharging || player == null) return;
        Vector3 dir = (player.transform.position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 8f);
    }

    private Vector3 GetPlayerCenter()
    {
        if (player == null) return Vector3.zero;
        CapsuleCollider cap = player.GetComponent<CapsuleCollider>()
                           ?? player.GetComponentInChildren<CapsuleCollider>()
                           ?? player.GetComponentInParent<CapsuleCollider>();
        return cap != null ? player.transform.position + cap.center
                           : player.transform.position + Vector3.up * 0.9f;
    }
}
