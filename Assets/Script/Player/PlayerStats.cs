using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 3;
    public int currentHealth;
    public int coins = 0;
    public int lives = 3;
    [SerializeField] private PlayerUI playerUI;

    public void Awake()
    {
        currentHealth = maxHealth;
    }

public void TakeDamage(int amount)
{
    currentHealth -= amount;
    currentHealth = Mathf.Max(currentHealth, 0);

    playerUI?.UpdateHearts(currentHealth);

    if (currentHealth <= 0)
    {
        LoseLife();
    }
}

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        playerUI?.UpdateHearts(currentHealth);
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        if (coins >= 100)
        {
            coins = 0; // Reset coins after reaching 100
            lives += 1;
            playerUI?.UpdateLives(lives);
            Debug.Log("Extra Life Gained! Total Lives: " + lives);
        }
        playerUI?.UpdateCoins(coins);
    }

    public void LoseLife()
    {
        lives--;
        playerUI?.UpdateLives(lives);
        if (lives <= 0)
        {
            Debug.Log("Game Over!");
            // Respawn or end game
        }
        else
        {
            Debug.Log("Player Lost a Life! Respawn...");
            currentHealth = maxHealth;
        }
    }
}
