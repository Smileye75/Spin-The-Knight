using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages health, damage, and death logic for player and enemies.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 3; // Maximum health value
    private int health;       // Current health

    private void Start()
    {
        // Initialize health to maximum at start
        health = maxHealth;
    }

    /// <summary>
    /// Handles taking damage and checks for death.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void DamageManager(int damage)
    {
        if (health > 1)
        {
            // Reduce health but don't go below zero
            health = Mathf.Max(health - damage, 0);
            Debug.Log(health + " Health Remaining");
        }
        else
        {
            // Death logic
            Debug.Log("I'm Dead");
            if (CompareTag("Enemy"))
            {
                // Destroy enemy GameObject on death
                Destroy(gameObject);
            }
        }
    }
}
