using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;

    private void Start()
    {
        GameManager.Instance.OnPauseToggled += HandlePause;
        GameManager.Instance.OnVictory += ShowVictoryUI;
        GameManager.Instance.OnGameOver += ShowGameOverUI;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPauseToggled -= HandlePause;
        GameManager.Instance.OnVictory -= ShowVictoryUI;
        GameManager.Instance.OnGameOver -= ShowGameOverUI;
    }

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

    public void ShowVictoryUI()
    {
        if (victoryUI != null) victoryUI.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    public void HideVictoryUI()
    {
        if (victoryUI != null) victoryUI.SetActive(false);
    }

    public void HideGameOverUI()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    // Buttons now delegate to GameManager
    public void Play() => GameManager.Instance.ReloadScene();
    public void TryAgain() => GameManager.Instance.ReloadScene();
    public void Quit() => Application.Quit();
    public void MainMenu() => GameManager.Instance.LoadMainMenu();
}
