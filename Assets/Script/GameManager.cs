using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance for global access
    public static GameManager Instance { get; private set; }

    // Game state enum and current state
    public enum GameState { Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("References")]
    public GameObject player; // Assign your player GameObject here
    public MenuUI menuUI;     // Assign your MenuUI script here

    [Header("Default Spawn")]
    [SerializeField] private Transform defaultSpawnPoint; // Assign in Inspector

    // Events for UI and other systems to subscribe to
    public event Action OnGameOver;
    public event Action OnVictory;
    public event Action<bool> OnPauseToggled;
    public event Action<Vector3> OnRespawn;

    public event Action OnBossSpawned;
    public event Action OnBossDefeated;

    // Singleton setup and persistence
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keeps GameManager across scene loads
    }

    private void Start()
    {
        SetPlayerToDefaultSpawn();
    }

    // Handles pause input
    private void Update()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // Toggles pause state and notifies listeners
    public void TogglePause()
    {
        bool pause = CurrentState != GameState.Paused;
        CurrentState = pause ? GameState.Paused : GameState.Playing;
        Time.timeScale = pause ? 0 : 1; // Freezes/unfreezes game
        OnPauseToggled?.Invoke(pause);
    }

    // Handles game over logic
    public void GameOver()
    {
        TogglePause();
        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
        menuUI?.ShowGameOverUI();
    }

    // Handles victory logic
    public void Victory()
    {
        TogglePause();
        CurrentState = GameState.Victory;
        OnVictory?.Invoke();
        menuUI?.ShowVictoryUI();
    }

    // Respawns the player at a given position, or at default if position is Vector3.zero
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
        OnRespawn?.Invoke(position); // Other systems (e.g., RespawnManager) listen here
    }

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

    // Reloads the current scene
    public void ReloadScene()
    {
        Time.timeScale = 1; // Ensure game is unpaused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartCoroutine(WaitAndSetDefaultSpawn());
    }

    public void PlayGame()
    {
        Time.timeScale = 1; // Ensure game is unpaused
        SceneManager.LoadScene("Tutorial Level");
        StartCoroutine(WaitAndSetDefaultSpawn());
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Loads the main menu scene
    public void LoadMainMenu()
    {
        Time.timeScale = 1; // Ensure game is unpaused
        SceneManager.LoadScene("Main Menu");
        StartCoroutine(WaitAndSetDefaultSpawn());
    }

    // Handles boss defeat logic
    public void BossDefeated()
    {
        Debug.Log("Boss defeated!");
        OnBossDefeated?.Invoke();
        Victory(); // Triggers victory flow
    }

    private IEnumerator WaitAndSetDefaultSpawn()
    {
        yield return null; // Wait one frame for scene to load
        SetPlayerToDefaultSpawn();
    }
}
