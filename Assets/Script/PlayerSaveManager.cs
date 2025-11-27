using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "playerSave.json");
    }

    public void SaveData(PlayerStats stats)
    {
        PlayerSaveData data = new PlayerSaveData
        {
            coins = stats.coins,
            lives = stats.lives,
            shieldUnlocked = stats.shieldUnlocked,
            heavyAttackUnlocked = stats.heavyAttackUnlocked,
            jumpAttackUnlocked = stats.jumpAttackUnlocked,
            rollJumpUnlocked = stats.rollJumpUnlocked,
            lastCheckpointSceneName = stats.lastCheckpointSceneName
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"✅ Saved to: {savePath}");
    }

    public bool HasSave() => File.Exists(savePath);

    public PlayerSaveData LoadData()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("⚠️ No save file found, returning null.");
            return null;
        }

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
        Debug.Log($"📂 Loaded from: {savePath}");
        return data;
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑 Save file deleted.");
        }
    }
}
