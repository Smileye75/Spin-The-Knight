using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject victoryUI;



    private bool isPaused = false;

    void Update()
    {
        // Only allow pause if player lives < 0
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (victoryUI != null && victoryUI.activeSelf)
        {
            if (stats != null && stats.lives < 0 && Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (thePlayer != null)
            thePlayer.SetActive(!isPaused);

        if (playerUI != null)
            playerUI.SetActive(!isPaused);

        if (isPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("Tutorial Level");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowVictoryUI()
    {
        if (victoryUI != null)
            victoryUI.SetActive(true);
        TogglePause();
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
