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

    [SerializeField] public PlayerUI playerUI;

    public static event System.Action<GameObject> OnPlayerLostLife;

    [SerializeField] public Animator animator;        

    [Header("Feedback")]
    public MMF_Player damageFeedback;                  
    public MMF_Player cameraShakeFeedback;   

    [Header("Shield")]
    public GameObject[] shieldObject;
    public bool shieldUnlocked = false;
    private int currentShieldIndex = 0; // Add this line


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
    [SerializeField] private LoadingScreen loadingScreen;           
    [Tooltip("Animator bool used by the death state.")]
    [SerializeField] private string deathBoolName = "Dead";        

    [SerializeField] private ParticleSystem healEffect;             

    private bool isDying = false;

    [Header("Player Movement Upgrades")]
    public bool jumpAttackUnlocked = false;
    public GameObject jumpAttackFeather;
    public bool heavyAttackUnlocked = false;
    public bool rollJumpUnlocked = false;

    public string lastCheckpointSceneName;

    public static bool IsLoadingFromSave = false;

    public void Awake()
    {
        if (!IsLoadingFromSave)
        {
            currentHealth = maxHealth;
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
    }

    public void Update()
    {
        if (playerUI == null || loadingScreen == null)
        {
            var persistentRoot = FindObjectOfType<PersistentUIRoot>();
            if (persistentRoot != null)
            {
                if (playerUI == null)
                    playerUI = persistentRoot.GetComponentInChildren<PlayerUI>(true);
                if (loadingScreen == null)
                    loadingScreen = persistentRoot.GetComponentInChildren<LoadingScreen>(true);
            }
        }
        else
        {
            enabled = false;
        }
    }


    public void TakeDamage(int amount)
    {
        if (isInvulnerable || isDying) return;
        if (animator != null)
            animator.SetTrigger("Hit");
        cameraShakeFeedback?.PlayFeedbacks();
        StartCoroutine(DamageFeedbackAndInvulnerability(1.5f, 0.2f));
        
        if(shieldUnlocked && shieldObject != null && currentShieldIndex <= 0)
        {
            shieldObject[currentShieldIndex].SetActive(false);
            shieldUnlocked = false;
            return;
        }
        
        if (shieldUnlocked && shieldObject != null && shieldObject.Length > 0)
        {
            // Deactivate current shield
            if (shieldObject[currentShieldIndex] != null)
                shieldObject[currentShieldIndex].SetActive(false);

            // Move to previous shield (wrap around)
            currentShieldIndex = (currentShieldIndex - 1 + shieldObject.Length) % shieldObject.Length;

            // Activate previous shield
            if (shieldObject[currentShieldIndex] != null)
                shieldObject[currentShieldIndex].SetActive(true);
            return;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        playerUI?.UpdateHearts(currentHealth);     

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
    
    public void DeadZoneDeath()
    {
        if (isDying) return;

        if (playerStateMachine != null)
            playerStateMachine.EndAttack();

        StartCoroutine(HandleDeathAndRespawn());
    }

    private IEnumerator HandleDeathAndRespawn()
    {
        if (isDying) yield break;
        isDying = true;

        playerStateMachine?.EndAttack();

        if (playerStateMachine != null)
            playerStateMachine.SwitchState(playerStateMachine.PausingState);

        // Play death animation
        if (animator) animator.SetBool(deathBoolName, true);

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


        if (playerStateMachine != null)
            playerStateMachine.SwitchState(new PlayerMoveState(playerStateMachine));

        isDying = false;
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
    }

    public void UnlockRollJump()
    {
        rollJumpUnlocked = true;
    }

    public void UnlockShield()
    {
        if (shieldObject == null || shieldObject.Length == 0)
            return;

       
        if (currentShieldIndex == 2)
            return;

        
        if (shieldUnlocked)
        {
            
            if (shieldObject[currentShieldIndex] != null)
                shieldObject[currentShieldIndex].SetActive(false);

            
            currentShieldIndex = (currentShieldIndex + 1) % shieldObject.Length;

            
            if (shieldObject[currentShieldIndex] != null)
                shieldObject[currentShieldIndex].SetActive(true);

        }
        else
        {
            // Unlock first shield
            currentShieldIndex = 0;
            if (shieldObject[currentShieldIndex] != null)
                shieldObject[currentShieldIndex].SetActive(true);
            shieldUnlocked = true;
        }
    }


    public IEnumerator Invulnerability(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;


        if (playerRenderer != null && originalMaterial != null)
            playerRenderer.material = originalMaterial;
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


        if (playerRenderer != null && originalMaterial != null)
            playerRenderer.material = originalMaterial;
    }

    public void Rest()
    {
        currentHealth = maxHealth;

        healEffect?.Play();
        playerUI?.UpdateHearts(currentHealth);

    }

    public void AddCoin(int amount)
    {
        coins += amount;
        if (coins >= 100)
        {
            coins = coins - 100;
            lives += 1;
            playerUI?.UpdateLives(lives);
        }
        playerUI?.UpdateCoins(coins);
    }


    public void LoseLife()
    {
        lives--;
        playerUI?.UpdateLives(lives);

        if (lives < 0)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            currentHealth = maxHealth;
            playerUI?.UpdateHearts(currentHealth);
        }

        OnPlayerLostLife?.Invoke(gameObject);
    }

    public void RegisterPlayerUI(PlayerUI ui)
    {
        playerUI = ui;
    }

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

        heavyAttackUnlocked = data.heavyAttackUnlocked;
        jumpAttackUnlocked = data.jumpAttackUnlocked;
        rollJumpUnlocked = data.rollJumpUnlocked;


        currentHealth = maxHealth;

        playerUI?.UpdateHearts(currentHealth);
        playerUI?.UpdateLives(lives);
        playerUI?.UpdateCoins(coins);

    }

    public void ResetPlayerProgress()
    {
        coins = 0;
        lives = 3;

        heavyAttackUnlocked = false;
        jumpAttackUnlocked = false;
        rollJumpUnlocked = false;

        currentHealth = maxHealth;

        playerUI?.UpdateHearts(currentHealth);
        playerUI?.UpdateLives(lives);
        playerUI?.UpdateCoins(coins);

        SaveManager.Instance.SaveData(this);
    }

}
