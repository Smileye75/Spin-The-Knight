using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint; // Assign this in the Inspector

    private GameObject spawnedBoss;
    private bool bossSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!bossSpawned && other.CompareTag("Player"))
        {
            SpawnBoss();
            bossSpawned = true;
            // Optionally, disable the trigger so it can't be used again
            GetComponent<Collider>().enabled = false;
        }
    }

    public void SpawnBoss()
    {
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            // Destroy any previous boss instance
            if (spawnedBoss != null)
                Destroy(spawnedBoss);

            spawnedBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }
    }

    public void DespawnBoss()
    {
        if (spawnedBoss != null)
        {
            Destroy(spawnedBoss);
            spawnedBoss = null;
        }
        bossSpawned = false;
        GetComponent<Collider>().enabled = true; // Allow retriggering if needed
    }
}
