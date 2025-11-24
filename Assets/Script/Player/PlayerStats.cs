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

    [SerializeField] public PlayerUI playerUI;        // Reference to the UI script for updating hearts, coins, lives

    // Event for other systems to listen for life loss
    public static event System.Action<GameObject> OnPlayerLostLife;

    [SerializeField] public Animator animator;         // Animator for player animations

    [Header("Feedback")]
    public MMF_Player damageFeedback;                  // Feedback to play when taking damage
    public MMF_Player cameraShakeFeedback;            // Camera shake feedback on damage

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

    [Header("Stamina Regen")]
    [SerializeField] private float staminaRegenDelay = 0.5f; // Delay before regen starts
    [SerializeField] private float staminaRegenRate = 0.2f;  // Stamina points per second (for smooth fill)

    private float smoothStamina;
    private Coroutine staminaRegenCoroutine;
    private Coroutine staminaDrainCoroutine;

    // Prevents re-entrant death handling
    private bool isDying = false;

    // Track if we're actively draining (so we don't start regen while draining)
    private bool isDraining = false;

    [Header("Player Movement Upgrades")]
    public bool shieldUnlocked = false;
    public GameObject shieldObject;
    public GameObject staminaUI;
    public bool jumpAttackUnlocked = false;
    public GameObject jumpAttackFeather;
    public bool heavyAttackUnlocked = false;
    public bool rollJumpUnlocked = false;



    public static bool IsLoadingFromSave = false;

    /// <summary>
    /// Initializes health and references on Awake.
    /// </summary>
    public void Awake()
    {
        if (!IsLoadingFromSave)
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            // Keep smooth float in sync at start
            smoothStamina = currentStamina;
            playerUI?.UpdateStamina(currentStamina, maxStamina); // Update stamina UI
        }

        if(jumpAttackFeather != null)
            jumpAttackFeather.SetActive(jumpAttackUnlocked);

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
        if (!staminaUI && playerUI != null)
            staminaUI = playerUI.staminaShieldUI;
    }

    public void Update()
    {
        if (playerUI == null || loadingScreen == null || staminaUI == null)
        {
            var persistentRoot = FindObjectOfType<PersistentUIRoot>();
            if (persistentRoot != null)
            {
                if (playerUI == null)
                    playerUI = persistentRoot.GetComponentInChildren<PlayerUI>(true);
                if (loadingScreen == null)
                    loadingScreen = persistentRoot.GetComponentInChildren<LoadingScreen>(true);
                // Assign staminaUI after playerUI is found
                if (staminaUI == null && playerUI != null)
                    staminaUI = playerUI.staminaShieldUI;
                if(!shieldUnlocked && shieldObject != null)
                {
                    shieldObject.SetActive(false);
                    if (staminaUI != null)
                        staminaUI.SetActive(false);
                }
            }
        }
        else
        {
            enabled = false; // Disables Update() from running again
        }
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
        cameraShakeFeedback?.PlayFeedbacks();
        // Shield block: costs stamina, nullifies damage
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

    public void UnlockShield()
    {
        shieldUnlocked = true;
        if (shieldObject != null)
            shieldObject.SetActive(true);
        if (staminaUI != null)
            staminaUI.SetActive(true);
    }

    public void UnlockJumpAttack()
    {
        jumpAttackUnlocked = true;
        if(jumpAttackFeather != null)
            jumpAttackFeather.SetActive(true);
    }

    public void UnlockHeavyAttack()
    {
        heavyAttackUnlocked = true;
        // Optional: play VFX / update UI here if you have assets to show the unlock
        Debug.Log("Heavy Attack unlocked");
    }

    public void UnlockRollJump()
    {
        rollJumpUnlocked = true;
        // Optional: play VFX / update UI here if you have assets to show the unlock
        Debug.Log("Roll Jump unlocked");
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
        smoothStamina = currentStamina; // keep in sync
        healEffect?.Play();
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
        // Only allow if smoothStamina is greater than 1
        if (smoothStamina > 1f)
        {
            smoothStamina -= amount;
            currentStamina = Mathf.FloorToInt(smoothStamina);
            playerUI?.UpdateStaminaSmooth(smoothStamina, maxStamina);
            playerUI?.UpdateStamina(currentStamina, maxStamina);

            // Only start regen if we're not actively draining
            if (!isDraining)
            {
                StopSmoothStaminaRegen();
                staminaRegenCoroutine = StartCoroutine(SmoothStaminaRegen());
            }

            return true;
        }
        else
        {
            Debug.Log("Not enough stamina to attack!");
            return false;
        }
    }

    private IEnumerator SmoothStaminaRegen()
    {
        yield return new WaitForSeconds(staminaRegenDelay);

        while (smoothStamina < maxStamina)
        {
            // Smoothly move stamina towards max
            smoothStamina = Mathf.MoveTowards(smoothStamina, maxStamina, staminaRegenRate * Time.deltaTime);

            // Update the UI with the smooth value every frame
            playerUI?.UpdateStaminaSmooth(smoothStamina, maxStamina);

            // Only update int stamina when we cross an integer value
            int newStamina = Mathf.FloorToInt(smoothStamina);
            if (newStamina != currentStamina)
            {
                currentStamina = newStamina;
            }

            yield return null;
        }

        // At the end, ensure the UI is fully filled and synced
        currentStamina = maxStamina;
        playerUI?.UpdateStamina(currentStamina, maxStamina);

        staminaRegenCoroutine = null;
    }

    // Helper: stop regen safely
    public void StopSmoothStaminaRegen()
    {
        if (staminaRegenCoroutine != null)
        {
            StopCoroutine(staminaRegenCoroutine);
            staminaRegenCoroutine = null;
        }
    }

    public void StartSmoothStaminaDrain(float drainRate)
    {
        if (isDraining || currentStamina <= 1) return;

        isDraining = true;

        // Stop regen to avoid tug-of-war
        StopSmoothStaminaRegen();
        // DO NOT reset smoothStamina here! Let it continue smoothly from its current value.

        if (staminaDrainCoroutine != null)
            StopCoroutine(staminaDrainCoroutine);

        staminaDrainCoroutine = StartCoroutine(SmoothStaminaDrain(drainRate));
    }

    public void StopSmoothStaminaDrain()
    {
        if (!isDraining) return;

        if (staminaDrainCoroutine != null)
        {
            StopCoroutine(staminaDrainCoroutine);
            staminaDrainCoroutine = null;
        }

        isDraining = false;

        // Kick off regen (respects delay) if not full
        if (currentStamina < maxStamina && staminaRegenCoroutine == null)
        {
            staminaRegenCoroutine = StartCoroutine(SmoothStaminaRegen());
        }
    }

    private IEnumerator SmoothStaminaDrain(float drainRate)
    {
        while (smoothStamina > 1f)
        {
            smoothStamina = Mathf.MoveTowards(smoothStamina, 1f, drainRate * Time.deltaTime);
            playerUI?.UpdateStaminaSmooth(smoothStamina, maxStamina);

            int newStamina = Mathf.FloorToInt(smoothStamina);
            if (newStamina != currentStamina)
            {
                currentStamina = newStamina;
            }

            yield return null;
        }

        // Clamp & finalize at 1
        smoothStamina = 1f;
        currentStamina = 1;
        playerUI?.UpdateStaminaSmooth(smoothStamina, maxStamina);
        playerUI?.UpdateStamina(currentStamina, maxStamina);

        // Stop draining and allow regen to start
        StopSmoothStaminaDrain();
    }

    public void SavePlayerProgress()
    {
        SaveManager.Instance.SaveData(this);
    }

    public void LoadPlayerProgress()
    {
        PlayerSaveData data = SaveManager.Instance.LoadData();
        if (data == null) return;

        // Apply saved fields
        coins = data.coins;
        lives = data.lives;
        shieldUnlocked = data.shieldUnlocked;
        heavyAttackUnlocked = data.heavyAttackUnlocked;
        jumpAttackUnlocked = data.jumpAttackUnlocked;
        rollJumpUnlocked = data.rollJumpUnlocked;

        // Reset health & stamina to max
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        smoothStamina = currentStamina;

        // Apply unlock visuals/UI if needed
        if (shieldUnlocked) UnlockShield();
        else if (shieldObject) shieldObject.SetActive(false);

        playerUI?.UpdateHearts(currentHealth);
        playerUI?.UpdateLives(lives);
        playerUI?.UpdateCoins(coins);
        playerUI?.UpdateStamina(currentStamina, maxStamina);

        Debug.Log("✅ Player progress loaded!");
    }

    public void ResetPlayerProgress()
    {
        coins = 0;
        lives = 3;

        shieldUnlocked = false;
        heavyAttackUnlocked = false;
        jumpAttackUnlocked = false;
        rollJumpUnlocked = false;

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        smoothStamina = currentStamina;

        // Update the UI and visuals
        if (shieldObject) shieldObject.SetActive(false);
        if (staminaUI) staminaUI.SetActive(false);

        playerUI?.UpdateHearts(currentHealth);
        playerUI?.UpdateLives(lives);
        playerUI?.UpdateCoins(coins);
        playerUI?.UpdateStamina(currentStamina, maxStamina);

        // Save this default state so the file is updated
        SaveManager.Instance.SaveData(this);

        Debug.Log("🔁 Player progress reset to default and saved!");
    }

    public void StartSmoothStaminaRegen()
    {
        if (staminaRegenCoroutine != null) return;
        staminaRegenCoroutine = StartCoroutine(SmoothStaminaRegen());
    }
}
