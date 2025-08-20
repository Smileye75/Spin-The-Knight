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
        // You can add more pause logic here (e.g., show/hide UI)
    }

    // Handles game over logic
    public void GameOver()
    {
        TogglePause();
        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
        menuUI?.ShowGameOverUI(); // Show game over UI
        // You can add more game over logic here
    }

    // Handles victory logic
    public void Victory()
    {
        TogglePause();
        CurrentState = GameState.Victory;
        OnVictory?.Invoke();
        menuUI?.ShowVictoryUI();
        // You can add more victory logic here
    }

    // Respawns the player at a given position
    public void RespawnPlayer(Vector3 position)
    {
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = position;

            if (cc != null) cc.enabled = true;
        }
        OnRespawn?.Invoke(position);
        // You can add more respawn logic here (e.g., reset health)
    }

    // Reloads the current scene
    public void ReloadScene()
    {
        Time.timeScale = 1; // Ensure game is unpaused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // You can add logic to reset game state here
    }

    // Loads the main menu scene
    public void LoadMainMenu()
    {
        Time.timeScale = 1; // Ensure game is unpaused
        SceneManager.LoadScene("Main Menu");
        // You can add logic to clean up game state here
    }

    // Handles boss defeat logic
    public void BossDefeated()
    {
        Debug.Log("Boss defeated!");
        OnBossDefeated?.Invoke();
        Victory(); // Triggers victory flow
        // You can add more boss defeat logic here (e.g., drop loot, play cutscene)
    }
}
