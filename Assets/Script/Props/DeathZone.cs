using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DeathZone is a trigger zone that instantly kills the player when entered.
/// When the player enters this zone, they lose all health, triggering the death and respawn logic.
/// Useful for pits, hazards, or out-of-bounds areas.
/// </summary>
public class DeathZone : MonoBehaviour
{
    /// <summary>
    /// Called when another collider enters the trigger zone.
    /// If the collider is the player, forces them to lose all health.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player fell into DeathZone!");

            // Trigger life loss by dealing max health as damage
            if (other.TryGetComponent<PlayerStats>(out var stats))
            {
                stats.DeadZoneDeath(); // Force death
            }
        }
    }
}
