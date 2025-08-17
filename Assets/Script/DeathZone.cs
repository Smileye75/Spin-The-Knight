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
            var playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(playerStats.maxHealth);
            }
        }
    }
}
