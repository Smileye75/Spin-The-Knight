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
    public static event System.Action<GameObject> OnPlayerLostLife;
    [SerializeField] private Animator animator; // Add this field

    public MMF_Player damageFeedback;
    private bool isInvulnerable = false;

    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private PlayerStateMachine playerStateMachine;
    [SerializeField] private CharacterController playerController;

    public void Awake()
    {
        currentHealth = maxHealth;
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
            
    if (!playerStateMachine) playerStateMachine = GetComponent<PlayerStateMachine>();

    if (!playerController) playerController = GetComponent<CharacterController>();

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            originalMaterial = playerRenderer.material;
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return; // Ignore damage if invulnerable

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        playerUI?.UpdateHearts(currentHealth);

        StartCoroutine(DamageFeedbackAndInvulnerability(1.5f, 0.2f)); // 2 seconds, every 0.2s

        if (animator != null)
            animator.SetTrigger("Hit"); // Play hit animation

        if (currentHealth <= 0)
        {
            animator.SetBool("Dead", true);
            LoseLife();
        }
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

    private void LoseLife()
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
