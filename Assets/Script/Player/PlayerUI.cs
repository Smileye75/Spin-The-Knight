using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerUI manages the player's HUD, including health hearts, lives, and coin count.
/// It provides methods to update the UI in response to changes in the player's stats,
/// such as taking damage, gaining lives, or collecting coins.
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [Header("Health")]
    public Image[] hearts;           // Array of heart icons representing player health
    public Sprite fullHeart;         // Sprite for a full (active) heart
    public Sprite emptyHeart;        // Sprite for an empty (lost) heart

    [Header("Lives")]
    public TMP_Text livesText;       // Text displaying the number of lives (TextMeshPro)

    [Header("Coins")]
    public TMP_Text coinText;        // Text displaying the number of coins (TextMeshPro)

    [Header("Stamina")]
    public Image[] staminaIcons;      // Array of stamina icons
    public Sprite staminaGem;        // Sprite for full stamina
    public GameObject staminaShieldUI; // Reference to the stamina/shield UI element

    // Example starting values (can be set from another script if needed)
    [Header("Initial UI States")]
    [SerializeField] private int startingHealth = 3;   // Initial health to display
    [SerializeField] private int startingLives = 3;    // Initial lives to display
    [SerializeField] private int startingCoins = 0;    // Initial coins to display

    /// <summary>
    /// Initializes the UI to the starting values on game start.
    /// </summary>
    private void Start()
    {
        UpdateHearts(startingHealth);
        UpdateLives(startingLives);
        UpdateCoins(startingCoins);
    }

    /// <summary>
    /// Updates the heart icons based on the player's current health.
    /// Sets each heart to full or empty depending on current health value.
    /// </summary>
    /// <param name="currentHealth">The player's current health.</param>
    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }

    /// <summary>
    /// Updates the number of lives shown in the UI.
    /// </summary>
    /// <param name="currentLives">The player's current number of lives.</param>
    public void UpdateLives(int currentLives)
    {
        if (livesText != null)
            livesText.text = "x " + currentLives;
    }

    /// <summary>
    /// Updates the coin counter in the UI.
    /// </summary>
    /// <param name="coinCount">The player's current coin count.</param>
    public void UpdateCoins(int coinCount)
    {
        if (coinText != null)
            coinText.text = coinCount.ToString("D2");
    }
    public void UpdateStamina(int currentStamina, int maxStamina)
    {
        for (int i = 0; i < staminaIcons.Length; i++)
        {
            if (i < maxStamina)
            {
                staminaIcons[i].gameObject.SetActive(true);
                staminaIcons[i].fillAmount = i < currentStamina ? 1f : 0f;
            }
            else
            {
                staminaIcons[i].gameObject.SetActive(false); // Hide unused icons
            }
        }
    }
    public void UpdateStaminaSmooth(float smoothStamina, int maxStamina)
    {
        for (int i = 0; i < staminaIcons.Length; i++)
        {
            if (i < maxStamina)
            {
                staminaIcons[i].gameObject.SetActive(true);

                if (i < Mathf.FloorToInt(smoothStamina))
                {
                    // Full stamina
                    staminaIcons[i].fillAmount = 1f;
                }
                else if (i == Mathf.FloorToInt(smoothStamina))
                {
                    // Partially filled stamina (the one currently refilling)
                    staminaIcons[i].fillAmount = smoothStamina - Mathf.Floor(smoothStamina);
                }
                else
                {
                    // Empty
                    staminaIcons[i].fillAmount = 0f;
                }
            }
            else
            {
                staminaIcons[i].gameObject.SetActive(false);
            }
        }
    }
}
