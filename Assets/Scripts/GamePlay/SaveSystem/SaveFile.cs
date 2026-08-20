using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveFile : MonoBehaviour
{
    public static SaveFile Instance { get; private set; }

    private const string FileName = "save.json";

    public string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasSave() => File.Exists(SavePath);

    public bool LoadGame()
    {
        YantraStatsController stats = FindFirstObjectByType<YantraStatsController>();
        if (stats == null)
        {
            Debug.LogError("Load failed: no YantraStatsController exists in the scene.");
            return false;
        }

        return LoadGame(stats.transform.root, stats);
    }

    public bool SaveGame(Transform player, YantraStatsController stats)
    {
        if (player == null || stats == null)
        {
            Debug.LogError("Save failed: player or stats reference is missing.");
            return false;
        }

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            playerPosition = player.position,
            playerRotation = player.rotation,
            yantCount = stats.GetYantCount(),
            currentHp = stats.GetCurrentHp(),
            currentStamina = stats.GetCurrentStamina()
        };

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            Debug.Log($"Game saved to {SavePath}");
            return true;
        }
        catch (IOException exception)
        {
            Debug.LogError($"Save failed: {exception.Message}");
            return false;
        }
    }

    public bool LoadGame(Transform player, YantraStatsController stats)
    {
        if (player == null || stats == null || !HasSave())
            return false;

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            player.SetPositionAndRotation(data.playerPosition, data.playerRotation);
            stats.SetYantCount(data.yantCount);
            stats.ApplySavedValues(data.currentHp, data.currentStamina);
            return true;
        }
        catch (IOException exception)
        {
            Debug.LogError($"Load failed: {exception.Message}");
            return false;
       }
    }

    public void DeleteSave()
    {
        if (HasSave())
            File.Delete(SavePath);
    }
}
