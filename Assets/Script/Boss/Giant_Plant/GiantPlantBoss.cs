using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantPlantBoss : MonoBehaviour
{
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private Transform[] minionSpawnPoints;
    [SerializeField] private GameObject[] minionPrefabs;

    [Header("Detection")]
    [SerializeField] private Collider awakeCollider;
    [SerializeField] private Collider detectionCollider;
    [SerializeField] private BoxCollider hitboxCollider;
    [SerializeField] private SphereCollider damageCollider;
    private Transform detectedPlayer;

    [Header("Attack")]
    [Tooltip("Minimum seconds between PlayerInRange triggers.")]
    [SerializeField] private float attackCooldown = 3f;
    private float lastAttackTime = -Mathf.Infinity;
    private int damageAmount = 1;

    private enum Phase { Phase1, Phase2, Phase3 }

    [Header("Phase Settings")]
    [SerializeField] private int maxHitPoints = 3;
    private int currentHitPoints;
    private Phase currentPhase = Phase.Phase1;
    private Phase lastPhase = Phase.Phase1;
    private Coroutine activePhaseRoutine;
    private bool hasAwoken = false;

    [Header("Minion Spawning")]
    [SerializeField] private int[] phaseMinionCounts = new int[] { 2, 4, 6 };
    [SerializeField] private float[] phaseSpawnIntervals = new float[] { 3f, 2f, 1f };

    [Header("Resting")]
    [SerializeField] private float restDuration = 5f;

    private List<GameObject> spawnedMinions = new List<GameObject>();
    private Coroutine phaseSpawnRoutine;
    private Coroutine restRoutine;

    private void Awake()
    {
        currentHitPoints = maxHitPoints;
        currentPhase = DeterminePhase();
        lastPhase = currentPhase;
        if (hitboxCollider != null) hitboxCollider.enabled = false;
    }

    private void Update()
    {
        Phase newPhase = DeterminePhase();
        if (newPhase != currentPhase)
        {
            ChangePhase(newPhase);
        }
    }

    private Phase DeterminePhase()
    {
        if (currentHitPoints <= Mathf.Max(1, maxHitPoints / 3)) return Phase.Phase3;
        if (currentHitPoints <= Mathf.Max(1, (maxHitPoints * 2) / 3)) return Phase.Phase2;
        return Phase.Phase1;
    }

    private void ChangePhase(Phase newPhase)
    {
        ExitPhase(currentPhase);
        currentPhase = newPhase;
        EnterPhase(currentPhase);
    }

    private void EnterPhase(Phase p)
    {
        StopActivePhaseRoutine();
        SetHitbox(false);
        StartMinionSpawning(p);
        lastPhase = p;
    }

    private void ExitPhase(Phase p)
    {
        StopActivePhaseRoutine();
        StopCoroutineSafe(ref phaseSpawnRoutine);
        StopCoroutineSafe(ref restRoutine);
    }

    private void StopActivePhaseRoutine()
    {
        StopCoroutineSafe(ref activePhaseRoutine);
    }

    private void StartMinionSpawning(Phase p)
    {
        StopCoroutineSafe(ref phaseSpawnRoutine);
        phaseSpawnRoutine = StartCoroutine(SpawnMinionsForPhase(p));
    }

    private IEnumerator SpawnMinionsForPhase(Phase p)
    {
        int idx = (int)p;
        int count = GetPhaseValue(phaseMinionCounts, idx, 0);
        float interval = GetPhaseValue(phaseSpawnIntervals, idx, 2f);

        if (count <= 0 || minionPrefabs == null || minionPrefabs.Length == 0 || minionSpawnPoints == null || minionSpawnPoints.Length == 0)
            yield break;

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < count; i++)
        {
            if (currentPhase != p) yield break;
            Transform spawnPoint = minionSpawnPoints[i % minionSpawnPoints.Length];
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
            GameObject minion = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            spawnedMinions.Add(minion);
            yield return new WaitForSeconds(interval);
        }

        phaseSpawnRoutine = null;
        yield return StartCoroutine(WaitForMinionsAndRest(p));
    }

    private IEnumerator WaitForMinionsAndRest(Phase p)
    {
        
        while (true)
        {
            spawnedMinions.RemoveAll(x => x == null);
            if (spawnedMinions.Count == 0) break;
            yield return new WaitForSeconds(0.5f);
        }
        bossAnimator?.ResetTrigger("Awake");
        bossAnimator?.SetBool("Sleep", true);
        SetDetection(false);
        SetHitbox(true);
        SetDamageCollider(false);
        StartRestRoutine(p);
    }

    private void StartRestRoutine(Phase p)
    {
        StopCoroutineSafe(ref restRoutine);
        restRoutine = StartCoroutine(RestRoutine(p));
    }

    private IEnumerator RestRoutine(Phase p)
    {
        float elapsed = 0f;
        while (elapsed < restDuration)
        {
            if (DeterminePhase() != p)
            {
                SetHitbox(false);
                SetDetection(true);
                restRoutine = null;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetHitbox(false);
        SetDetection(true);
        SetDamageCollider(true);
        bossAnimator?.SetBool("Sleep", false);
        bossAnimator?.SetTrigger("Awake");
        yield return new WaitForSeconds(2.5f);
        StartMinionSpawning(p);
        restRoutine = null;
    }

    private void Die()
    {
        bossAnimator?.SetTrigger("Dead");
        StopActivePhaseRoutine();
        StopCoroutineSafe(ref phaseSpawnRoutine);
        StopCoroutineSafe(ref restRoutine);
        SetHitbox(false);
        SetDetection(false);
        Destroy(gameObject, 2f);
    }

    private void FaceDetectedPlayer()
    {
        if (detectedPlayer == null) return;
        Vector3 direction = detectedPlayer.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (awakeCollider == null) return;

        if (other.CompareTag("PlayerDetection"))
        {
            if (!hasAwoken)
            {
                hasAwoken = true;
                EnterPhase(currentPhase);
            }
            bossAnimator?.SetTrigger("Awake");
            if (awakeCollider != null) awakeCollider.enabled = false;
            SetHitbox(false);
        }

        if (!other.CompareTag("Player")) return;

        var playerStats = other.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damageAmount);
            var receiver = other.GetComponentInParent<ForceReceiver>();
            if (receiver != null)
                receiver.ApplyKnockback(transform.position);
        }
        if (other.CompareTag("Weapon"))
        {
            TakeDamage(1);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (detectionCollider == null) return;
        if (!other.CompareTag("PlayerDetection")) return;

        detectedPlayer = other.transform;
        FaceDetectedPlayer();

        if (bossAnimator != null && Time.time >= lastAttackTime + attackCooldown)
        {
            bossAnimator.SetTrigger("PlayerInRange");
            lastAttackTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (detectionCollider == null) return;
        if (!other.CompareTag("PlayerDetection")) return;
        if (detectedPlayer != other.transform) return;
        detectedPlayer = null;
    }

    public void ResetBoss()
    {
        currentHitPoints = maxHitPoints;
        currentPhase = DeterminePhase();
        lastPhase = currentPhase;
        StopActivePhaseRoutine();
        EnterPhase(currentPhase);
        CleanupSpawnedMinions(true);
        SetDamageCollider(false);
    }
    
    public void TakeDamage(int amount)
    {
        currentHitPoints -= amount;
        if (currentHitPoints <= 0)
        {
            Die();
        }
    }

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

    // Utility methods
    private void SetHitbox(bool enabled)
    {
        if (hitboxCollider != null) hitboxCollider.enabled = enabled;
    }

    private void SetDetection(bool enabled)
    {
        if (detectionCollider != null) detectionCollider.enabled = enabled;
    }
    private void SetDamageCollider(bool enabled)
    {
        if (damageCollider != null) damageCollider.enabled = enabled;
    }

    private void StopCoroutineSafe(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private T GetPhaseValue<T>(T[] array, int idx, T fallback)
    {
        return (array != null && array.Length > idx) ? array[idx] : fallback;
    }
}