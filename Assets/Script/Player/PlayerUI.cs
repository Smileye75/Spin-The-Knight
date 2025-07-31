using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Health")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Lives")]
    public TMP_Text livesText; // Use Text if not using TextMeshPro

    [Header("Coins")]
    public TMP_Text coinText;

    // Example starting values (you can set these from another script if needed)
    [Header("Initial UI States")]
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int startingCoins = 0;

    private void Start()
    {
        // Initialize UI to starting values
        UpdateHearts(startingHealth);
        UpdateLives(startingLives);
        UpdateCoins(startingCoins);
    }

    /// <summary>
    /// Updates the heart icons based on current health.
    /// </summary>
    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }

    /// <summary>
    /// Updates the number of lives shown.
    /// </summary>
    public void UpdateLives(int currentLives)
    {
        if (livesText != null)
            livesText.text = "x " + currentLives;
    }

    /// <summary>
    /// Updates the coin counter.
    /// </summary>
    public void UpdateCoins(int coinCount)
    {
        if (coinText != null)
            coinText.text = coinCount.ToString("D2");
    }
}
