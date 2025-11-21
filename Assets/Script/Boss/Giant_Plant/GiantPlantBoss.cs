using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantPlantBoss : MonoBehaviour
{
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private Transform[] minionSpawnPoints;
    [SerializeField] private GameObject[] minionPrefabs;
    [SerializeField] private Animator[] vineAnimators;

    [Header("Detection")]
    [SerializeField] private Collider awakeCollider;
    [SerializeField] private Collider detectionCollider;
    private Transform detectedPlayer;

    [Header("Attack")]
    [Tooltip("Minimum seconds between PlayerInRange triggers.")]
    [SerializeField] private float attackCooldown = 3f;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    private int damageAmount = 1;

    // --- NEW: Phase system (template) ---
    private enum Phase { Phase1, Phase2, Phase3 }

    [Header("Phase Settings (template)")]
    [Tooltip("Total HP for the boss. Phase thresholds use relative HP (you can change logic).")]
    [SerializeField] private int maxHitPoints = 3;
    private int currentHitPoints;

    // runtime tracking
    private Phase currentPhase = Phase.Phase1;
    private Phase lastPhase = Phase.Phase1;
    private Coroutine activePhaseRoutine;

    // ensure we only start phases once when boss awakens
    private bool hasAwoken = false;

    [Header("Minion Spawning (per-phase)")]
    [Tooltip("Number of minions to spawn for each phase (index 0 = Phase1, 1 = Phase2, 2 = Phase3)")]
    [SerializeField] private int[] phaseMinionCounts = new int[] { 2, 4, 6 };
    [Tooltip("Spawn interval (seconds) for each phase (index aligned with phaseMinionCounts)")]
    [SerializeField] private float[] phaseSpawnIntervals = new float[] { 3f, 2f, 1f };

    // runtime tracking of spawned minions for cleanup / checks
    private List<GameObject> spawnedMinions = new List<GameObject>();

    // hold a reference to the active spawn coroutine so it can be stopped on phase changes
    private Coroutine phaseSpawnRoutine;

    private void Awake()
    {
        // initialize HP / phase
        currentHitPoints = maxHitPoints;
        currentPhase = DeterminePhase();
        lastPhase = currentPhase;
    }

    private void Start()
    {
        // existing start logic can remain; phases will begin when the boss is triggered (OnTriggerEnter)
    }

    private void Update()
    {
        // detection existing logic...
        // keep current behavior but ensure phase changes are handled
        Phase newPhase = DeterminePhase();
        if (newPhase != currentPhase)
        {
            // phase changed
            ExitPhase(currentPhase);
            currentPhase = newPhase;
            EnterPhase(currentPhase);
        }

        // existing per-frame behavior (face player, etc.) can run here
    }

    // Call to reduce HP and potentially change phase
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHitPoints = Mathf.Max(0, currentHitPoints - amount);

        // optional: play hit animation/state
        if (bossAnimator != null) bossAnimator.SetTrigger("Hit");

        // check death
        if (currentHitPoints <= 0)
        {
            Die();
            return;
        }

        // check for teleport/hit reaction etc. (existing logic can go here)

        // phase change handled in Update, but you can call it immediately:
        Phase newPhase = DeterminePhase();
        if (newPhase != currentPhase)
        {
            ExitPhase(currentPhase);
            currentPhase = newPhase;
            EnterPhase(currentPhase);
        }
    }

    private Phase DeterminePhase()
    {
        // Simple template: phases by remaining HP percentage/thresholds
        if (currentHitPoints <= Mathf.Max(1, maxHitPoints / 3))
            return Phase.Phase3;
        if (currentHitPoints <= Mathf.Max(1, (maxHitPoints * 2) / 3))
            return Phase.Phase2;
        return Phase.Phase1;
    }

    private void EnterPhase(Phase p)
    {
        // stop any existing phase routine
        StopActivePhaseRoutine();

        // start phase behaviour coroutine (template)
        switch (p)
        {
            case Phase.Phase1:
                activePhaseRoutine = StartCoroutine(PhaseRoutine_Template("Phase1"));
                break;
            case Phase.Phase2:
                activePhaseRoutine = StartCoroutine(PhaseRoutine_Template("Phase2"));
                break;
            case Phase.Phase3:
                activePhaseRoutine = StartCoroutine(PhaseRoutine_Template("Phase3"));
                break;
        }

        // start minion spawning for this phase (separate coroutine)
        if (phaseSpawnRoutine != null) StopCoroutine(phaseSpawnRoutine);
        phaseSpawnRoutine = StartCoroutine(SpawnMinionsForPhase(p));

        // Trigger vine animators according to phase (first 2 for phase1, first 4 for phase2, etc.)
        TriggerVinesForPhase(p);

        lastPhase = p;
        // optional: notify animator or VFX about phase entry
        // bossAnimator?.SetInteger("Phase", (int)p);
    }

    private void ExitPhase(Phase p)
    {
        // Clean up anything specific to a phase (stop effects, reset timers)
        StopActivePhaseRoutine();

        // stop minion spawner for this phase if running
        if (phaseSpawnRoutine != null)
        {
            StopCoroutine(phaseSpawnRoutine);
            phaseSpawnRoutine = null;
        }
    }

    private void StopActivePhaseRoutine()
    {
        if (activePhaseRoutine != null)
        {
            StopCoroutine(activePhaseRoutine);
            activePhaseRoutine = null;
        }
    }

    // Minimal template coroutine that you can replace with real behavior for each phase
    private IEnumerator PhaseRoutine_Template(string phaseName)
    {
        // Example loop: run while boss alive and in this phase
        while (DeterminePhase().ToString() == phaseName && currentHitPoints > 0)
        {
            // placeholder: you can call phase-specific methods here,
            // spawn minions, change shoot intervals, enable new attacks, etc.
            // e.g. SpawnMinion(), StartVineAttack(), ChangeFireRate(), etc.

            // For template, just wait a short time so coroutine yields control
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator SpawnMinionsForPhase(Phase p)
    {
        int idx = (int)p;
        int count = (phaseMinionCounts != null && phaseMinionCounts.Length > idx) ? phaseMinionCounts[idx] : 0;
        float interval = (phaseSpawnIntervals != null && phaseSpawnIntervals.Length > idx) ? phaseSpawnIntervals[idx] : 2f;

        // Guard: nothing to spawn or missing spawn points/prefabs
        if (count <= 0 || minionPrefabs == null || minionPrefabs.Length == 0 || minionSpawnPoints == null || minionSpawnPoints.Length == 0)
            yield break;
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < count; i++)
        {
            // if phase changed while spawning, stop spawning for this (old) phase
            if (currentPhase != p) yield break;

            // choose spawn point and prefab (round-robin spawn point, random prefab)
            Transform spawnPoint = minionSpawnPoints[i % minionSpawnPoints.Length];
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];

            GameObject minion = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            spawnedMinions.Add(minion);

            // wait interval before next spawn
            yield return new WaitForSeconds(interval);
        }

        phaseSpawnRoutine = null;
    }

    // Optional helper to purge null entries and optionally destroy remaining minions
    private void CleanupSpawnedMinions(bool destroyAll = false)
    {
        spawnedMinions.RemoveAll(x => x == null);
        if (destroyAll)
        {
            foreach (var go in spawnedMinions)
                if (go != null) Destroy(go);
            spawnedMinions.Clear();
        }
    }

    private void Die()
    {
        // existing death logic
        if (bossAnimator != null) bossAnimator.SetTrigger("Dead");
        // stop routines
        StopActivePhaseRoutine();
        // destroy or play final death sequence
        Destroy(gameObject, 2f);
    }

    private void FaceDetectedPlayer()
    {
        if (detectedPlayer == null) return;
        Vector3 direction = detectedPlayer.position - transform.position;
        direction.y = 0f; // Only rotate horizontally
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (awakeCollider == null) return;

        // Wake / start phases when the detection trigger is hit (PlayerDetection tag)
        if (other.CompareTag("PlayerDetection"))
        {
            if (!hasAwoken)
            {
                hasAwoken = true;
                EnterPhase(currentPhase);
            }

            bossAnimator?.SetTrigger("Awake");
            awakeCollider.enabled = false;
        }

        // Only apply damage / knockback if the collider's tag is exactly "Player"
        if (!other.CompareTag("Player")) return;

        // Robust: get PlayerStats / ForceReceiver from the collider or its parents
        var playerStats = other.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damageAmount);

            var receiver = other.GetComponentInParent<ForceReceiver>();
            if (receiver != null)
                receiver.ApplyKnockback(transform.position);
        }

        // no player found or not tagged "Player" -> nothing else to do
    }

    private void OnTriggerStay(Collider other)
    {
        if (detectionCollider == null) return;

        if (!other.CompareTag("PlayerDetection")) return;

        detectedPlayer = other.transform;

        FaceDetectedPlayer();

        // Trigger attack animation (PlayerInRange) with cooldown
        if (bossAnimator != null && Time.time >= lastAttackTime + attackCooldown)
        {
            bossAnimator.SetTrigger("PlayerInRange");
            lastAttackTime = Time.time;
            isAttacking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (detectionCollider == null) return;
        if (!other.CompareTag("PlayerDetection")) return;
        if (detectedPlayer != other.transform) return;
        detectedPlayer = null;
        isAttacking = false;
    }

    public void ResetBoss()
    {
        currentHitPoints = maxHitPoints;
        currentPhase = DeterminePhase();
        lastPhase = currentPhase;
        StopActivePhaseRoutine();
        EnterPhase(currentPhase);

        // Destroy any spawned minions and clear list
        CleanupSpawnedMinions(true);
    }

    // Triggers 'Rise' on a subset of vineAnimators depending on the phase:
    // Phase1 => indices [0,1], Phase2 => [0..3], Phase3 => [0..5] (bounded by array length)
    private void TriggerVinesForPhase(Phase p)
    {
        if (vineAnimators == null || vineAnimators.Length == 0) return;
        int countToTrigger = Mathf.Min(vineAnimators.Length, ((int)p + 1) * 2);
        for (int i = 0; i < countToTrigger; i++)
        {
            var a = vineAnimators[i];
            if (a != null)
                a.SetTrigger("Rise");
        }
    }
}
