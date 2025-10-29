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
        bool pause = CurrentState != GameState.Paused;
        CurrentState = pause ? GameState.Paused : GameState.Playing;
        Time.timeScale = pause ? 0 : 1; // Freezes/unfreezes game
        OnPauseToggled?.Invoke(pause);
    }

    /// <summary>
    /// Handles game over logic, pauses the game, shows UI, and notifies listeners.
    /// </summary>
    public void GameOver()
    {
        TogglePause();
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
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 spawnPos = position;
            if (spawnPos == Vector3.zero && defaultSpawnPoint != null)
                spawnPos = defaultSpawnPoint.position;

            player.transform.position = spawnPos;

            if (cc != null) cc.enabled = true;
        }
        OnRespawn?.Invoke(position);

        // --- Add this: ---
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
        Time.timeScale = 1; // Ensure game is unpaused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartCoroutine(WaitAndSetDefaultSpawn());
    }

    /// <summary>
    /// Loads the main game scene ("TheVillageOutskirt") and resets spawn.
    /// </summary>
    public void NewGame()
    {
        Time.timeScale = 1;
        ResetGameData();
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


    public void LoadGame()
    {
        Time.timeScale = 1; // Ensure game is unpaused
        pendingLoadData = SaveManager.Instance.LoadData();
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        yield return StartCoroutine(LoadSceneWithFade("TheVillageOutskirt"));

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

        // If no data was loaded, or after reset, ensure fade out
        if (!loadedData && loadingScreen)
            yield return StartCoroutine(loadingScreen.FadeOut(1f));
    }

    public void ResetGameData()
    {
        SaveManager.Instance.DeleteSave();

        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.ResetPlayerProgress();

        // Optionally fade out if loading screen is active
        if (loadingScreen) StartCoroutine(loadingScreen.FadeOut(1f));
    }

    public bool HasSaveData()
    {
        return SaveManager.Instance.HasSave();
    }
}
