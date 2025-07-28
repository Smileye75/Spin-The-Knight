using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers damage and knockback when the player enters the enemy's hit zone.
/// </summary>
public class EnemyDamageTrigger : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to the player.")]
    [SerializeField] private int damageAmount = 1;

    /// <summary>
    /// Called when another collider enters this trigger.
    /// Checks for player, applies damage and knockback.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Only proceed if the collider belongs to the player
        if (!other.CompareTag("Player")) return;

        // Try to get the player's health system and apply damage
        if (other.TryGetComponent<HealthSystem>(out HealthSystem playerHealth))
        {
            playerHealth.DamageManager(damageAmount);

            // Try to apply knockback to the player
            if (other.TryGetComponent(out ForceReceiver receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
        }
    }

}
