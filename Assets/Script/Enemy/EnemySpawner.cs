using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner is responsible for spawning a set number of enemies at random spawn points,
/// but only starts spawning after the player enters a trigger zone. The spawn interval decreases
/// every 5 enemies. When all enemies are defeated, the assigned wall object is deactivated to allow player progress.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private PlayerStateMachine stateMachine;
    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float intervalDecrease = 0.2f;
    [SerializeField] private float minInterval = 0.2f; // Minimum allowed interval

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;        // Array of spawning locations

    [Header("Limits")]
    [SerializeField] private int maxEnemiesLimit = 20;       // Total number of enemies to spawn before stopping

    [Header("Trigger Zone")]
    [SerializeField] private Collider triggerZone;           // Collider that starts spawning when the player enters

    [Header("Wall Object")]
    [SerializeField] private GameObject wallObject;          // Wall to deactivate after all enemies are defeated
    [SerializeField] private SkullGates skullGates;           // Reference to the SkullGates script

    [Header("VFX")]
    [SerializeField] private ParticleSystem rockVFX;

    [SerializeField] private Animation shieldTutorialAnim;

    private bool shieldTutorialPlayed = false;

    private float timeUntilSpawn;                            // Countdown timer for next enemy spawn
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List of currently alive enemies
    private bool spawningActive = false;                     // Whether spawning is currently active
    private int totalSpawned = 0;                            // Total number of enemies spawned so far
    public bool AllEnemiesDefeated => totalSpawned >= maxEnemiesLimit && spawnedEnemies.Count == 0;
    public bool HasAliveEnemies => spawnedEnemies.Count > 0;
        
    /// <summary>
    /// Initializes the spawner. Spawning is inactive until triggered.
    /// </summary>
    private void Awake()
    {
        // Do not start timer until triggered by player
        timeUntilSpawn = 0f;
    }

    /// <summary>
    /// Handles enemy spawning, spawn timer, and wall deactivation each frame.
    /// </summary>
    private void Update()
    {
        if (!spawningActive && totalSpawned < maxEnemiesLimit)
            return;

        spawnedEnemies.RemoveAll(e => e == null);

        timeUntilSpawn -= Time.deltaTime;

        if (spawningActive && totalSpawned >= maxEnemiesLimit)
        {
            spawningActive = false;
        }

        // Only check totalSpawned now
        if (spawningActive && timeUntilSpawn <= 0 && totalSpawned < maxEnemiesLimit)
        {
            SpawnEnemy();
            SetTimeUntilSpawn();
        }

        if (totalSpawned >= maxEnemiesLimit && spawnedEnemies.Count == 0 && wallObject != null)
        {
            stateMachine.playerStats.UnlockShield();
            if (!shieldTutorialPlayed)
            {
                shieldTutorialAnim?.Play();
                shieldTutorialPlayed = true;
            }
            skullGates.ActivateGate();

        }
    }

    /// <summary>
    /// Sets the timer for the next enemy spawn, with a random interval between min and max.
    /// </summary>
    private void SetTimeUntilSpawn()
    {
        timeUntilSpawn = spawnInterval;
    }

    
    /// <summary>
    /// Spawns a new enemy at a random assigned spawn point, and reduces spawn interval every 5 enenmy spawns.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Spawner missing references.");
            return;
        }
        if (totalSpawned >= maxEnemiesLimit) return;

        Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = chosenPoint.position;

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(enemy);
        totalSpawned++;

        // Decrease interval after each spawn, but clamp to minInterval
        spawnInterval = Mathf.Max(minInterval, spawnInterval - intervalDecrease);
    }

    /// <summary>
    /// Activates spawning when the player enters the trigger zone. Disables the trigger to prevent retriggering.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!spawningActive && other.CompareTag("Player"))
        {
            spawningActive = true;
            SetTimeUntilSpawn();
            if (triggerZone != null)
                triggerZone.enabled = false; // Prevent retriggering
        }
        if(other.CompareTag("Projectile"))
        {
            if (rockVFX != null)
            {
                rockVFX.Play();
            }
            skullGates.DestroyGate();
            wallObject.SetActive(false);
            wallObject = null;
        }
    }

    public void ResetSpawner()
    {
        if(AllEnemiesDefeated)
            return;
        // Destroy all spawned enemies
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();

        // Reset counters and flags
        totalSpawned = 0;
        spawningActive = false;
        timeUntilSpawn = 0f;
        shieldTutorialPlayed = false;

        // Optionally re-enable the trigger zone and wall
        if (triggerZone != null)
            triggerZone.enabled = true;
        if (wallObject != null)
            wallObject.SetActive(true);


    }

}
