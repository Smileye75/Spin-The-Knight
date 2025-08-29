using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 3;
    public int currentHealth;
    public int coins = 0;
    public int lives = 3;

    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private GameObject deathMenuUI;

    // Other systems can still listen if needed
    public static event System.Action<GameObject> OnPlayerLostLife;

    [SerializeField] public Animator animator;

    [Header("Feedback")]
    public MMF_Player damageFeedback;

    private bool isInvulnerable = false;

    [Header("Render / Materials")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Material originalMaterial;

    [Header("Control References")]
    [SerializeField] private PlayerStateMachine playerStateMachine;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private InputReader inputReader;

    [Header("Death Settings")]
    [Tooltip("How long to wait (unscaled seconds) before respawning after death.")]
    [SerializeField] private float deathDelay = 2f;
    [Tooltip("Animator bool used by the death state.")]
    [SerializeField] private string deathBoolName = "Dead";

    // Prevents re-entrant death handling
    private bool isDying = false;

    public void Awake()
    {
        currentHealth = maxHealth;

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

    public void TakeDamage(int amount)
    {
        if (isInvulnerable || isDying) return;

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

    private IEnumerator HandleDeathAndRespawn()
    {
        if (isDying) yield break;
        isDying = true;

        if (playerStateMachine != null)
            playerStateMachine.EndAttack();


        if (animator) animator.SetBool(deathBoolName, true);
        if (playerStateMachine) playerStateMachine.enabled = false;
        if (playerController)  playerController.enabled = false;

        yield return new WaitForSecondsRealtime(deathDelay);
        LoseLife();

        if (animator) animator.SetBool(deathBoolName, false);
        if (playerStateMachine) playerStateMachine.enabled = true;
        if (playerController)  playerController.enabled = true;

        // FIX: Always call RespawnPlayer, fallback to default if no checkpoint
        Vector3 respawnPos = Checkpoint.HasActive ? Checkpoint.ActiveRespawnPosition : Vector3.zero;
        GameManager.Instance.RespawnPlayer(respawnPos);

        isDying = false;
    }

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

    // 👇 Made public so we can call it internally and other systems can too if needed
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
}
