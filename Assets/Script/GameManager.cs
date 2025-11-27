using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is a singleton that manages the overall game state, including pausing, game over, victory,
/// player respawn, scene transitions, and boss events. It provides global access to important references,
/// handles UI menu logic, and broadcasts events for other systems to respond to changes in game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance for global access
    public static GameManager Instance { get; private set; }

    // Enum for tracking the current game state
    public enum GameState { Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("References")]
    public GameObject player; // Assign your player GameObject here
    public MenuUI menuUI;     // Assign your MenuUI script here

    [Header("Default Spawn")]
    [SerializeField] private Transform defaultSpawnPoint; // Assign in Inspector

    public LoadingScreen loadingScreen; // Reference to loading screen

    // Events for UI and other systems to subscribe to
    public event Action OnGameOver;
    public event Action OnVictory;
    public event Action<bool> OnPauseToggled;
    public event Action<Vector3> OnRespawn;
    public event Action OnBossDefeated;

    private PlayerSaveData pendingLoadData; // Add this field to GameManager

    // If true, unlock shield for the player once after the next scene load
    private bool unlockShieldOnNextLoad = false;

    private bool canPause = true;

    /// <summary>
    /// Ensures only one GameManager exists and persists across scenes.
    /// Assigns references to player and UI.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        AssignReferences();
    }

    /// <summary>
    /// Sets the player to the default spawn point at game start.
    /// </summary>
    private void Start()
    {
        SetPlayerToDefaultSpawn();
    }

    /// <summary>
    /// Toggles the pause state and notifies listeners.
    /// </summary>
    public void TogglePause()
    {
        if (!canPause) return;
        bool pause = CurrentState != GameState.Paused;
        CurrentState = pause ? GameState.Paused : GameState.Playing;
        Time.timeScale = pause ? 0 : 1;
        OnPauseToggled?.Invoke(pause);

        // Pause/unpause player using PausingState
        if (player != null)
        {
            var stateMachine = player.GetComponent<PlayerStateMachine>();
            if (stateMachine != null)
            {
                if (pause)
                    stateMachine.SwitchState(stateMachine.PausingState);
                else
                    stateMachine.SwitchState(new PlayerMoveState(stateMachine)); // or your default state
            }
        }
    }

    /// <summary>
    /// Handles game over logic, pauses the game, shows UI, and notifies listeners.
    /// </summary>
    public void GameOver()
    {
        TogglePause();
        canPause = false;
        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
        menuUI?.ShowGameOverUI();
    }

    /// <summary>
    /// Handles victory logic, pauses the game, shows UI, and notifies listeners.
    /// </summary>
    public void Victory()
    {
        TogglePause();
        canPause = false;
        CurrentState = GameState.Victory;
        OnVictory?.Invoke();
        menuUI?.ShowVictoryUI();
    }

    /// <summary>
    /// Respawns the player at a given position, or at the default spawn if position is Vector3.zero.
    /// Notifies listeners of the respawn event.
    /// </summary>
    /// <param name="position">The position to respawn the player at.</param>
    public void RespawnPlayer(Vector3 position)
    {
        if (player != null)
        {
            var stateMachine = player.GetComponent<PlayerStateMachine>();
            if (stateMachine != null)
                stateMachine.SwitchState(stateMachine.PausingState);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 spawnPos = position;
            if (spawnPos == Vector3.zero && defaultSpawnPoint != null)
                spawnPos = defaultSpawnPoint.position;

            player.transform.position = spawnPos;

            if (cc != null) cc.enabled = true;

            // After respawn, switch back to normal state
            StartCoroutine(ResumePlayerAfterRespawn(stateMachine));
        }
        OnRespawn?.Invoke(position);

        // --- Add this: ---
        var boss = FindObjectOfType<GiantPlantBoss>();
        if (boss != null)
        {
            boss.ResetBoss();
            Debug.Log("Boss has been reset upon player respawn.");
        }

        var bossSpawner = FindObjectOfType<BossSpawner>();
        if (bossSpawner != null)
        {
            bossSpawner.DespawnBoss();
        }
        var enemySpawners = FindObjectsOfType<EnemySpawner>();
        foreach (var spawner in enemySpawners)
        {
            spawner.ResetSpawner();
        }
    }

    private IEnumerator ResumePlayerAfterRespawn(PlayerStateMachine stateMachine)
    {
        yield return new WaitForSeconds(1.5f); // Adjust as needed
        if (stateMachine != null)
            stateMachine.SwitchState(new PlayerMoveState(stateMachine)); // or your default state
    }

    /// <summary>
    /// Sets the player to the default spawn point.
    /// </summary>
    private void SetPlayerToDefaultSpawn()
    {
        if (defaultSpawnPoint != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = defaultSpawnPoint.position;
            if (cc != null) cc.enabled = true;
        }
    }

    /// <summary>
    /// Reloads the current scene and resets UI and player spawn.
    /// </summary>
    public void ReloadScene()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(ReloadSceneRoutine());
    }

    private IEnumerator ReloadSceneRoutine()
    {
        // Fade in before reload (optional)
        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeIn(1f));
 
        // Reload the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Wait one frame for scene to finish loading
        yield return null;

        canPause = true;
        StartCoroutine(WaitAndSetDefaultSpawn());

        // Fade out after reload
        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeOut(1f));
    }

    /// <summary>
    /// Loads the main game scene ("TheVillageOutskirt") and resets spawn.
    /// </summary>
    public void NewGame()
    {
        Time.timeScale = 1;
        ResetGameData();
        canPause = true;
        StartCoroutine(LoadSceneWithFade("TheVillageOutskirt"));
    }

    public IEnumerator SceneTransition()
    {
        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeIn(1f));
        yield return new WaitForSeconds(1.5f);
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Loads the main menu scene and resets spawn/UI.
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(LoadSceneWithFade("StartMenu"));
        menuUI?.HideAllMenus();
    }

    /// <summary>
    /// Handles boss defeat logic, triggers victory, and notifies listeners.
    /// </summary>
    public void BossDefeated()
    {
        Debug.Log("Boss defeated!");
        SaveGame();
        OnBossDefeated?.Invoke();
        Victory(); // Triggers victory flow
    }

    /// <summary>
    /// Waits for the scene to load, then assigns references and sets player spawn/UI.
    /// </summary>
    private IEnumerator WaitAndSetDefaultSpawn()
    {
        yield return null; // Wait one frame for scene to load
        AssignReferences();
        SetPlayerToDefaultSpawn();
        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                stats.UpdateUI();
        }

        // Apply scene-start unlocks if requested
        if (unlockShieldOnNextLoad && player != null)
        {
            var ps = player.GetComponent<PlayerStats>();
            ps?.UnlockShield();
            unlockShieldOnNextLoad = false;
        }

        // --- UI update logic here ---
        menuUI?.HideAllMenus();
        menuUI?.playerUI?.SetActive(true);
        menuUI?.ReSubscribeEvents();

        // Enable/disable pause action based on scene
        string sceneName = SceneManager.GetActiveScene().name;
        bool enablePause = sceneName != "StartMenu";
        menuUI?.SetPauseActionEnabled(enablePause);
    }

    /// <summary>
    /// Finds and assigns references to player and menu UI if not already set.
    /// </summary>
    private void AssignReferences()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (menuUI == null)
            menuUI = FindObjectOfType<MenuUI>();
    }

    public IEnumerator LoadSceneWithFade(string sceneName, float fadeDuration = 1f)
    {
        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeIn(fadeDuration));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        yield return null;

        AssignReferences();
        SetPlayerToDefaultSpawn();

        // Apply scene-start unlocks if requested (also cover immediate load path)
        if (unlockShieldOnNextLoad && player != null)
        {
            var ps = player.GetComponent<PlayerStats>();
            ps?.UnlockShield();
            unlockShieldOnNextLoad = false;
        }

        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                stats.UpdateUI();
        }

        // Always fade out if no save data or after reset
        if (!HasSaveData() && loadingScreen)
            yield return StartCoroutine(loadingScreen.FadeOut(fadeDuration));

        // Always fade out at the end
        if (loadingScreen)
            yield return StartCoroutine(loadingScreen.FadeOut(1.5f));
    }

    public void SaveGame()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            SaveManager.Instance.SaveData(stats);
    }

        public void LoadVillageOutskirt()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(LoadSceneWithFade("TheVillageOutskirt"));
    }

    public void LoadMagicalForest()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(LoadSceneWithFade("MagicalForest"));
    }

    public void ResetartLevel()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(LoadSceneWithFade(SceneManager.GetActiveScene().name));
    }

    public void LoadGame()
    {
        Time.timeScale = 1; // Ensure game is unpaused
        pendingLoadData = SaveManager.Instance.LoadData();
        canPause = true;

        // Use saved checkpoint scene name if available
        string sceneToLoad = SceneManager.GetActiveScene().name;
        if (pendingLoadData != null && !string.IsNullOrEmpty(pendingLoadData.lastCheckpointSceneName))
            sceneToLoad = pendingLoadData.lastCheckpointSceneName;

        StartCoroutine(LoadGameRoutine(sceneToLoad));
    }

    private IEnumerator LoadGameRoutine(string sceneName)
    {
        yield return StartCoroutine(LoadSceneWithFade(sceneName));

        PlayerStats stats = null;
        float timeout = 5f;
        bool loadedData = false;

        while (stats == null && timeout > 0f)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                stats = player.GetComponent<PlayerStats>();

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (stats != null && pendingLoadData != null)
        {
            stats.coins = pendingLoadData.coins;
            stats.lives = pendingLoadData.lives;
            stats.shieldUnlocked = pendingLoadData.shieldUnlocked;
            stats.heavyAttackUnlocked = pendingLoadData.heavyAttackUnlocked;
            stats.jumpAttackUnlocked = pendingLoadData.jumpAttackUnlocked;
            stats.rollJumpUnlocked = pendingLoadData.rollJumpUnlocked;
            stats.lastCheckpointSceneName = pendingLoadData.lastCheckpointSceneName;

            stats.currentHealth = stats.maxHealth;
            stats.currentStamina = stats.maxStamina;

            if (stats.shieldUnlocked) stats.UnlockShield();
            else if (stats.shieldObject) stats.shieldObject.SetActive(false);

            stats.playerUI?.UpdateHearts(stats.currentHealth);
            stats.playerUI?.UpdateLives(stats.lives);
            stats.playerUI?.UpdateCoins(stats.coins);
            stats.playerUI?.UpdateStamina(stats.currentStamina, stats.maxStamina);

            Debug.Log("✅ Player progress loaded after scene transition!");
            loadedData = true;
        }

        if (!loadedData && loadingScreen)
            yield return StartCoroutine(loadingScreen.FadeOut(1f));
    }


    public void ResetGameData()
    {

        SaveManager.Instance.DeleteSave();
        canPause = true;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.ResetPlayerProgress();

        if (loadingScreen) StartCoroutine(loadingScreen.FadeOut(1f));
    }

    public bool HasSaveData()
    {
        return SaveManager.Instance.HasSave();
    }
}
