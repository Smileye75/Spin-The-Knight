using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Testing script: when the player enters this zone, they lose all health.
/// </summary>
public class DeathZone : MonoBehaviour
{
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Debug.Log("Player fell into DeathZone!");

        // Trigger life loss
        if (other.TryGetComponent<PlayerStats>(out var stats))
        {
            stats.TakeDamage(stats.maxHealth); // Force death
        }
    }
}
}
