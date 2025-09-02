using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] public GameObject playerUI;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;

    private InputReader inputReader;

    private void Start()
    {
        GameManager.Instance.OnPauseToggled += HandlePause;
        GameManager.Instance.OnVictory += ShowVictoryUI;
        GameManager.Instance.OnGameOver += ShowGameOverUI;

        inputReader = FindObjectOfType<InputReader>(true);
        if (inputReader != null)
            inputReader.pauseEvent += OnPausePressed;
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

    public void HideAllMenus()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (playerUI != null) playerUI.SetActive(false);
        if (victoryUI != null) victoryUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    private void OnPausePressed()
    {
        GameManager.Instance.TogglePause();
    }

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
    }

    public void SetPauseActionEnabled(bool enabled)
    {
        if (inputReader != null)
            inputReader.SetPauseEnabled(enabled);
    }
}
