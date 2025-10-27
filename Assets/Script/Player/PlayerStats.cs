using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// PlayerStats manages the player's health, coins, lives, UI updates, damage, healing, death, and respawn logic.
/// It also handles invulnerability after taking damage, plays feedbacks, and interacts with the checkpoint and game manager systems.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 3;                  // Maximum health the player can have
    public int currentHealth;                  // Current health value
    public int coins = 0;                      // Number of coins collected
    public int lives = 3;                      // Number of lives remaining

    [SerializeField] private PlayerUI playerUI;        // Reference to the UI script for updating hearts, coins, lives

    // Event for other systems to listen for life loss
    public static event System.Action<GameObject> OnPlayerLostLife;

    [SerializeField] public Animator animator;         // Animator for player animations

    [Header("Feedback")]
    public MMF_Player damageFeedback;                  // Feedback to play when taking damage

    private bool isInvulnerable = false;               // Prevents taking damage repeatedly

    [Header("Render / Materials")]
    [SerializeField] private Renderer playerRenderer;  // Renderer for changing material on damage
    [SerializeField] private Material originalMaterial;// Original material to restore after invulnerability

    [Header("Control References")]
    [SerializeField] private PlayerStateMachine playerStateMachine; // Reference to the player's state machine
    [SerializeField] private CharacterController playerController;  // Reference to the character controller
    [SerializeField] private InputReader inputReader;               // Reference to the input reader

    [Header("Death Settings")]
    [Tooltip("How long to wait (unscaled seconds) before respawning after death.")]
    [SerializeField] private float deathDelay = 2f;                 // Delay before respawn
    [SerializeField] private LoadingScreen loadingScreen;           // Reference to loading screen for respawn
    [Tooltip("Animator bool used by the death state.")]
    [SerializeField] private string deathBoolName = "Dead";         // Animator parameter for death

    [SerializeField] private ParticleSystem healEffect;             // Effect to play when healing

    [Header("Stamina")]
    public int maxStamina = 3;
    public int currentStamina;
    public int staminaCost = 1; // How much stamina each attack costs

    // Prevents re-entrant death handling
    private bool isDying = false;

    /// <summary>
    /// Initializes health and references on Awake.
    /// </summary>
    public void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        playerUI?.UpdateStamina(currentStamina, maxStamina); // Update stamina UI

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (!playerStateMachine)
            playerStateMachine = GetComponent<PlayerStateMachine>();

        if (!playerController)
            playerController = GetComponent<CharacterController>();

        if (!inputReader)
            inputReader = GetComponent<InputReader>();

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            originalMaterial = playerRenderer.material;
    }

    public void Update()
    {
        if (playerUI == null)
            playerUI = FindObjectOfType<PlayerUI>(true);
        else
            enabled = false; // Disables Update() from running again
    }

    /// <summary>
    /// Applies damage to the player, triggers feedback, invulnerability, and handles death if health reaches zero.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        if (isInvulnerable || isDying) return;
        if (animator != null)
            animator.SetTrigger("Hit");

        if (playerStateMachine.CurrentState is PlayerShieldState && currentStamina > 0)
        {
            ConsumeStamina(staminaCost);
            Debug.Log("Attack blocked by shield!");
            return; // Nullify damage
        }
    

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        playerUI?.UpdateHearts(currentHealth);

        StartCoroutine(DamageFeedbackAndInvulnerability(1.5f, 0.2f));

        if (animator != null)
            animator.SetTrigger("Hit");

        // Call EndAttack if health reaches 0
        if (currentHealth <= 0)
        {
            if (playerStateMachine != null)
                playerStateMachine.EndAttack();

            StartCoroutine(HandleDeathAndRespawn());
        }
    }

    /// <summary>
    /// Handles player death, disables controls, waits, then respawns or triggers game over.
    /// </summary>
    private IEnumerator HandleDeathAndRespawn()
    {
        if (isDying) yield break;
        isDying = true;

        playerStateMachine?.EndAttack();
        if (animator) animator.SetBool(deathBoolName, true);
        if (playerStateMachine) playerStateMachine.enabled = false;
        if (playerController) playerController.enabled = false;
        if (inputReader) inputReader.enabled = false; // Disable input

        yield return new WaitForSecondsRealtime(deathDelay);

        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeIn(1f));

        LoseLife();
        yield return new WaitForSeconds(2f);

        Vector3 respawnPos = Checkpoint.HasActive ? Checkpoint.ActiveRespawnPosition : Vector3.zero;
        GameManager.Instance.RespawnPlayer(respawnPos);
        Rest();

        if (animator) animator.SetBool(deathBoolName, false);
        yield return new WaitForSeconds(2f);
        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeOut(1f));
        if (playerStateMachine) playerStateMachine.enabled = true;
        if (playerController) playerController.enabled = true;
        if (inputReader) inputReader.enabled = true; // Re-enable input

        isDying = false;

    }




    /// <summary>
    /// Plays damage feedback and makes the player invulnerable for a short duration.
    /// </summary>
    /// <param name="duration">Total invulnerability duration.</param>
    /// <param name="interval">Interval between feedback effects.</param>
    private IEnumerator DamageFeedbackAndInvulnerability(float duration, float interval)
    {
        isInvulnerable = true;
        float elapsed = 0f;
        float nextFeedback = 0f;

        while (elapsed < duration)
        {
            if (elapsed >= nextFeedback)
            {
                damageFeedback?.PlayFeedbacks();
                nextFeedback += interval;
            }
            yield return null;
            elapsed += Time.deltaTime;
        }

        isInvulnerable = false;

        // Restore original material
        if (playerRenderer != null && originalMaterial != null)
            playerRenderer.material = originalMaterial;
    }

    /// <summary>
    /// Restores the player's health and stamina to maximum, plays rest effect, and updates UI.
    /// </summary>
    public void Rest()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        healEffect.Play();
        playerUI?.UpdateHearts(currentHealth);
        playerUI?.UpdateStamina(currentStamina, maxStamina);
    }

    /// <summary>
    /// Adds coins to the player, grants extra life at 100 coins, and updates UI.
    /// </summary>
    /// <param name="amount">Number of coins to add.</param>
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

    /// <summary>
    /// Handles losing a life, updates UI, and triggers game over if no lives remain.
    /// </summary>
    public void LoseLife()
    {
        lives--;
        playerUI?.UpdateLives(lives);

        if (lives < 0)
        {
            Debug.Log("Game Over!");
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.Log("Player Lost a Life! Respawn...");
            currentHealth = maxHealth;
            playerUI?.UpdateHearts(currentHealth);
        }

        OnPlayerLostLife?.Invoke(gameObject);
    }
    
    /// <summary>
    /// Registers the PlayerUI instance to the PlayerStats.
    /// </summary>
    /// <param name="ui">The PlayerUI instance to register.</param>
    public void RegisterPlayerUI(PlayerUI ui)
    {
        playerUI = ui;
    }

    /// <summary>
    /// Updates the player UI elements for health, lives, and coins.
    /// </summary>
    public void UpdateUI()
    {
        var playerUI = FindObjectOfType<PlayerUI>();
        if (playerUI != null)
        {
            playerUI.UpdateHearts(currentHealth);
            playerUI.UpdateLives(lives);
            playerUI.UpdateCoins(coins);
        }
    }

    public bool ConsumeStamina(int amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            playerUI?.UpdateStamina(currentStamina, maxStamina); // Update stamina UI
            return true;
        }
        else
        {
            Debug.Log("Not enough stamina to attack!");
            return false;
        }
    }

}
