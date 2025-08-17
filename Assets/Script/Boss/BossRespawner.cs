using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRespawner : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform respawnPoint;

    private GameObject spawnedBoss;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spawnedBoss = Instantiate(bossPrefab, respawnPoint.position, respawnPoint.rotation);
        }
    }

}
