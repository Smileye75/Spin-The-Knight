using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collectables handles the behavior of collectible items such as coins and food.
/// Coins and food can be attracted to the player when hit by a weapon,
/// and are collected when the player touches them, applying their effect.
/// </summary>
public class Collectables : MonoBehaviour
{
    private Transform targetPlayer;         // The player to move towards when attracted
    private bool moveToPlayer = false;      // Whether the collectable should move to the player
    public float moveSpeed = 15f;            // Speed at which the collectable moves to the player

    public int healAmount = 3;              // Amount of health restored by food

    /// <summary>
    /// Handles collision with weapon or player.
    /// If hit by a weapon, starts moving toward the player.
    /// If touched by the player, applies effect and destroys itself.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // If hit by a weapon, start moving toward the player who owns the weapon
        if (other.CompareTag("Weapon"))
        {
            PlayerStats stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                targetPlayer = stats.transform;
                moveToPlayer = true;
            }
        }
        // If touched by the player, apply effect and destroy
        else if (other.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            if (gameObject.CompareTag("Coins"))
            {
                stats.AddCoin(1);
                Debug.Log("Coins Collected: " + stats.coins);
            }
            if (gameObject.CompareTag("Food"))
            {
                stats.Heal(healAmount); // Heal the player
                Debug.Log("Player Healed! Current Health: " + stats.currentHealth);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Moves the collectable toward the player if attracted by weapon hit.
    /// </summary>
    private void Update()
    {
        if (moveToPlayer && targetPlayer != null)
        {
            Vector3 coinPos = transform.position;
            Vector3 playerPos = targetPlayer.position;

            Vector3 nextPos = Vector3.MoveTowards(coinPos, playerPos, moveSpeed * Time.deltaTime);
            nextPos.y = Mathf.Max(nextPos.y, 0.5f); // Clamp Y to at least 0.5

            transform.position = nextPos;
        }
    }

    /// <summary>
    /// Attracts the collectable to the specified player.
    /// </summary>
    /// <param name="player">The player to attract the collectable to.</param>
    public void AttractToPlayer(Transform player)
    {
        targetPlayer = player;
        moveToPlayer = true;
    }
}
