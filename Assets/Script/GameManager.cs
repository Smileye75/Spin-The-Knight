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
    public void PlayGame()
    {
        Time.timeScale = 1;
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

        // Start loading scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is loaded
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                // Scene is ready, activate it
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Wait one frame for scene objects to initialize
        yield return null;

        // Assign references and set up player/UI as before
        AssignReferences();
        SetPlayerToDefaultSpawn();

        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeOut(fadeDuration));
    }
}
