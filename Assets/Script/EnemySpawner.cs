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
    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;         // Prefab for enemies that would spawn
    [SerializeField] private float minimumSpawnTime = 1f;    // Minimum time between spawns (seconds)
    [SerializeField] private float maximumSpawnTime = 3f;    // Maximum time between spawns (seconds)

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;        // Array of spawning locations

    [Header("Limits")]
    [SerializeField] private int maxEnemies = 10;            // Maximum number of enemies alive at once
    [SerializeField] private int maxEnemiesLimit = 20;       // Total number of enemies to spawn before stopping

    [Header("Trigger Zone")]
    [SerializeField] private Collider triggerZone;           // Collider that starts spawning when the player enters

    [Header("Wall Object")]
    [SerializeField] private GameObject wallObject;          // Wall to deactivate after all enemies are defeated

    private float timeUntilSpawn;                            // Countdown timer for next enemy spawn
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List of currently alive enemies
    private bool spawningActive = false;                     // Whether spawning is currently active
    private int totalSpawned = 0;                            // Total number of enemies spawned so far

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
        // If not triggered and not finished, do nothing
        if (!spawningActive && totalSpawned < maxEnemiesLimit)
            return;

        // Remove any destroyed enemies from the list to keep it accurate
        spawnedEnemies.RemoveAll(e => e == null);

        // Countdown to next spawn
        timeUntilSpawn -= Time.deltaTime;

        // Stop spawning if we've reached the absolute max
        if (spawningActive && totalSpawned >= maxEnemiesLimit)
        {
            spawningActive = false;
        }

        // Spawn a new enemy if allowed (spawning is active, timer expired, under limits)
        if (spawningActive && timeUntilSpawn <= 0 && spawnedEnemies.Count < maxEnemies && totalSpawned < maxEnemiesLimit)
        {
            SpawnEnemy();
            SetTimeUntilSpawn();
        }

        // Check if all enemies have been spawned and defeated then disable the wall to allow the player to progress
        if (totalSpawned >= maxEnemiesLimit && spawnedEnemies.Count == 0 && wallObject != null)
        {
            wallObject.SetActive(false);
            wallObject = null; // Prevents repeated calls
        }
    }

    /// <summary>
    /// Sets the timer for the next enemy spawn, with a random interval between min and max.
    /// </summary>
    private void SetTimeUntilSpawn()
    {
        timeUntilSpawn = Random.Range(minimumSpawnTime, maximumSpawnTime);
    }

    /// <summary>
    /// Allows external scripts to increase the max enemies (up to the absolute limit).
    /// </summary>
    public void SetMaxEnemies(int newMax)
    {
        if (maxEnemies > maxEnemiesLimit)
        {
            maxEnemies = maxEnemiesLimit;
        }
        else
        {
            maxEnemies += newMax;
        }
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
        if (spawnedEnemies.Count >= maxEnemies || totalSpawned >= maxEnemiesLimit) return;

        // Pick a random spawn point
        Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = chosenPoint.position;

        // Instantiate the enemy and track it
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(enemy);
        totalSpawned++; // Increment total spawned count

        // Reduce spawn time after every 5 enemies spawned
        if (totalSpawned % 5 == 0)
        {
            // Clamp to minimum values to avoid zero or negative intervals
            minimumSpawnTime = Mathf.Max(0.2f, minimumSpawnTime - 0.2f);
            maximumSpawnTime = Mathf.Max(0.5f, maximumSpawnTime - 0.3f);
        }
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
    }
}
