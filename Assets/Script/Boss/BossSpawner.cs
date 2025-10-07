using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;

    private GameObject spawnedBoss;
    private bool bossSpawned = false;

    private void Awake()
    {
        // Disable the boss at the start
        if (bossPrefab != null)
        {
            bossPrefab.SetActive(false);
            spawnedBoss = bossPrefab;
        }
    }

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

    private void SpawnBoss()
    {
        if (spawnedBoss != null)
        {
            spawnedBoss.SetActive(true);
        }
    }
}
