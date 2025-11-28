using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MenuUI manages all in-game UI menus, including pause, victory, game over, and the main player HUD.
/// It listens for game state events from the GameManager and input events from InputReader,
/// and shows/hides the appropriate UI panels. It also manages cursor visibility and locking,
/// and provides methods for other scripts to control UI state and re-subscribe to events after scene changes.
/// </summary>
public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;   // Pause menu panel
    [SerializeField] public GameObject playerUI;        // Main player HUD
    [SerializeField] private GameObject victoryUI;      // Victory screen panel
    [SerializeField] private GameObject gameOverUI;     // Game over screen panel
    [SerializeField] private GameObject mainMenuUI;      // Main menu screen panel
    [SerializeField] private GameObject loadButton;       // Load button in main menu
    [SerializeField] private GameObject levelSelectorUI;  // Level selector UI panel
    [SerializeField] private GameObject levelCompleteUI;  // Play again button in victory/game over UI
    [SerializeField] private GameObject newGameWarningUI;   // New game warning UI panel

    private InputReader inputReader;                    // Reference to input handler

    /// <summary>
    /// Subscribes to GameManager and InputReader events on awake.
    /// </summary>
    private void Awake()
    {
        GameManager.Instance.OnPauseToggled += HandlePause;
        GameManager.Instance.OnVictory += ShowVictoryUI;
        GameManager.Instance.OnGameOver += ShowGameOverUI;

        inputReader = FindObjectOfType<InputReader>(true);
        if (inputReader != null)
            inputReader.pauseEvent += OnPausePressed;

        UpdateMainMenuUIVisibility();

        // Optional: Listen for scene changes to update mainMenuUI
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
 
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseToggled -= HandlePause;
            GameManager.Instance.OnVictory -= ShowVictoryUI;
            GameManager.Instance.OnGameOver -= ShowGameOverUI;
        }
        if (inputReader != null)
            inputReader.pauseEvent -= OnPausePressed;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Updates the main menu UI visibility based on the current scene.
    /// Disables all other UI panels when main menu is active.
    /// </summary>
    private void UpdateMainMenuUIVisibility()
    {
        bool isMainMenu = SceneManager.GetActiveScene().name == "StartMenu";
        if (mainMenuUI != null)
            mainMenuUI.SetActive(isMainMenu);

        // Always hide pause, victory, and game over UI when main menu is active
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (victoryUI != null) victoryUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (levelCompleteUI != null) levelCompleteUI.SetActive(false);

        // Always hide player UI in main menu, show only if not in main menu
        if (playerUI != null)
            playerUI.SetActive(!isMainMenu);

        // Always hide level selector unless explicitly shown
        if (levelSelectorUI != null)
            levelSelectorUI.SetActive(false);
    }

    /// <summary>
    /// Called when a new scene is loaded, updates main menu UI visibility.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMainMenuUIVisibility();
        ReSubscribeEvents();
    }

    /// <summary>
    /// Handles showing/hiding the pause menu and player HUD, and manages cursor state.
    /// Also hides victory/game over UI when resuming from pause.
    /// </summary>
    private void HandlePause(bool paused)
    {
        pauseMenuUI.SetActive(paused);
        playerUI.SetActive(!paused);

        // Hide victory/game over UI when resuming from pause
        if (!paused)
        {
            HideVictoryUI();
            HideGameOverUI();
        }

        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ShowNewGameWarningUI()
    {
        if (newGameWarningUI != null)
            newGameWarningUI.SetActive(true);
        mainMenuUI.SetActive(false);
    }
    
    public void HideNewGameWarningUI()
    {
        if (newGameWarningUI != null)
            newGameWarningUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    /// <summary>
    /// Shows the victory UI and hides the pause menu.
    /// </summary>
    public void ShowVictoryUI()
    {
        if (victoryUI != null) victoryUI.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    public void ShowLevelCompleteUI()
    {
        if (levelCompleteUI != null) levelCompleteUI.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    /// <summary>
    /// Shows the game over UI and hides the pause menu.
    /// </summary>
    public void ShowGameOverUI()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    /// <summary>
    /// Hides the victory UI.
    /// </summary>
    public void HideVictoryUI()
    {
        if (victoryUI != null) victoryUI.SetActive(false);
    }

    /// <summary>
    /// Hides the game over UI.
    /// </summary>
    public void HideGameOverUI()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    /// <summary>
    /// Hides all UI menus (pause, player HUD, victory, game over).
    /// </summary>
    public void HideAllMenus()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (playerUI != null) playerUI.SetActive(false);
        if (victoryUI != null) victoryUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    /// <summary>
    /// Called when the pause button is pressed. Toggles the game's pause state.
    /// </summary>
    private void OnPausePressed()
    {
        GameManager.Instance.TogglePause();
    }

    /// <summary>
    /// Re-subscribes to GameManager and InputReader events (useful after scene reloads).
    /// </summary>
    public void ReSubscribeEvents()
    {
        GameManager.Instance.OnPauseToggled -= HandlePause;
        GameManager.Instance.OnVictory -= ShowVictoryUI;
        GameManager.Instance.OnGameOver -= ShowGameOverUI;

        GameManager.Instance.OnPauseToggled += HandlePause;
        GameManager.Instance.OnVictory += ShowVictoryUI;
        GameManager.Instance.OnGameOver += ShowGameOverUI;

        if (inputReader == null)
            inputReader = FindObjectOfType<InputReader>(true);
        if (inputReader != null)
        {
            inputReader.pauseEvent -= OnPausePressed;
            inputReader.pauseEvent += OnPausePressed;
        }

        UpdateMainMenuUIVisibility(); // Ensure correct state after scene reload
    }

    /// <summary>
    /// Enables or disables the pause action in the InputReader.
    /// </summary>
    /// <param name="enabled">Whether to enable the pause action.</param>
    public void SetPauseActionEnabled(bool enabled)
    {
        if (inputReader != null)
            inputReader.SetPauseEnabled(enabled);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "StartMenu")
        {
            // Check if the game has save data and show/hide the load button accordingly
            if (GameManager.Instance.HasSaveData())
            {
                // Show load button
                loadButton.SetActive(true);
            }
            else
            {
                // Hide load button
                loadButton.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Shows the level selector UI.
    /// </summary>
    public void ShowLevelSelector()
    {
        if (levelSelectorUI != null)
            levelSelectorUI.SetActive(true);
    }

    /// <summary>
    /// Hides the level selector UI.
    /// </summary>
    public void HideLevelSelector()
    {
        if (levelSelectorUI != null)
            levelSelectorUI.SetActive(false);
    }

    public void OnNewGamePressed()
    {
        if (GameManager.Instance.HasSaveData())
        {
            ShowNewGameWarningUI(); // Show warning if save data exists
        }
        else
        {
            GameManager.Instance.NewGame(); // Start new game immediately if no save data
        }
    }

    public void OnConfirmNewGame()
    {
        HideNewGameWarningUI(); // Hide the warning UI
        GameManager.Instance.NewGame(); // Start new game and overwrite save
    }
}
