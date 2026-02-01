using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("References")]
    public GameObject player; 
    public MenuUI menuUI;     

    [Header("Default Spawn")]
    [SerializeField] private Transform defaultSpawnPoint;

    public LoadingScreen loadingScreen;

    public event Action OnGameOver;
    public event Action OnVictory;
    public event Action<bool> OnPauseToggled;
    public event Action<Vector3> OnRespawn;
    public event Action OnBossDefeated;

    private PlayerSaveData pendingLoadData;

    private bool canPause = true;

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

    private void Start()
    {
        SetPlayerToDefaultSpawn();
    }

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
                    stateMachine.SwitchState(new PlayerMoveState(stateMachine));
            }
        }
    }

    public void GameOver()
    {
        TogglePause();
        canPause = false;
        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
        menuUI?.ShowGameOverUI();
    }

    public void Victory()
    {
        TogglePause();
        canPause = false;
        CurrentState = GameState.Victory;
        OnVictory?.Invoke();

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        bool hasNextScene = (currentSceneIndex + 1 < totalScenes);

        if (hasNextScene)
        {
            menuUI?.ShowLevelCompleteUI();
        }
        else
        {
            menuUI?.ShowVictoryUI();
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(LoadSceneWithFade("TheVillageOutskirt"));
    }

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

            StartCoroutine(ResumePlayerAfterRespawn(stateMachine));
        }
        OnRespawn?.Invoke(position);

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
        yield return new WaitForSeconds(1.5f);
        if (stateMachine != null)
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
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

    public void ReloadScene()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(ReloadSceneRoutine());
    }

    private IEnumerator ReloadSceneRoutine()
    {
        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeIn(1f));
 
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

        yield return null;

        canPause = true;
        StartCoroutine(WaitAndSetDefaultSpawn());

        if (loadingScreen) yield return StartCoroutine(loadingScreen.FadeOut(1f));
    }

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

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        canPause = true;
        StartCoroutine(LoadSceneWithFade("StartMenu"));
        menuUI?.HideAllMenus();
    }

    public void PlayNextLevel()
    {
        Time.timeScale = 1;
        canPause = true;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string nextSceneName = System.IO.Path.GetFileNameWithoutExtension(
                SceneUtility.GetScenePathByBuildIndex(nextSceneIndex));
            pendingLoadData = SaveManager.Instance.LoadData();
            StartCoroutine(LoadSceneWithFade(nextSceneName));
        }
        else
        {
            Debug.LogWarning("No next level to load.");
        }
    }

    public void BossDefeated()
    {
        Debug.Log("Boss defeated!");
        SaveGame();
        OnBossDefeated?.Invoke();
        Victory(); 
    }

    private IEnumerator WaitAndSetDefaultSpawn()
    {
        yield return null; 
        AssignReferences();
        SetPlayerToDefaultSpawn();
        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                stats.UpdateUI();
        }

        menuUI?.HideAllMenus();
        menuUI?.playerUI?.SetActive(true);
        menuUI?.ReSubscribeEvents();

        string sceneName = SceneManager.GetActiveScene().name;
        bool enablePause = sceneName != "StartMenu";
        menuUI?.SetPauseActionEnabled(enablePause);
    }

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

        if (!HasSaveData() && loadingScreen)
            yield return StartCoroutine(loadingScreen.FadeOut(fadeDuration));

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
        pendingLoadData = SaveManager.Instance.LoadData();
        StartCoroutine(LoadSceneWithFade("TheVillageOutskirt"));
    }

    public void LoadMagicalForest()
    {
        Time.timeScale = 1;
        canPause = true;
        pendingLoadData = SaveManager.Instance.LoadData();
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
        Time.timeScale = 1; 
        pendingLoadData = SaveManager.Instance.LoadData();
        canPause = true;

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
            stats.heavyAttackUnlocked = pendingLoadData.heavyAttackUnlocked;
            stats.jumpAttackUnlocked = pendingLoadData.jumpAttackUnlocked;
            stats.rollJumpUnlocked = pendingLoadData.rollJumpUnlocked;
            stats.lastCheckpointSceneName = pendingLoadData.lastCheckpointSceneName;

            stats.currentHealth = stats.maxHealth;

            stats.playerUI?.UpdateHearts(stats.currentHealth);
            stats.playerUI?.UpdateLives(stats.lives);
            stats.playerUI?.UpdateCoins(stats.coins);

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
