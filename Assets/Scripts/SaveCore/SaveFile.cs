using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveFile : MonoBehaviour
{
    [SerializeField] private SaveSO _saveSO;

    private GameObject Players;

    private void Start()
    {
        Players = GameObject.FindGameObjectWithTag("Player");

        if (Players == null)
        {
            Debug.LogWarning("SaveFile: Player object with tag 'Player' was not found.");
        }
    }

    private void Awake()
    {
        if (_saveSO == null)
        {
            Debug.LogError("SaveFile: SaveSO is not assigned in the inspector.");
        }
    }


    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player")) {
            Save();
            Debug.Log("Game Saved!");
        }

    }
    public void Save()
    {
        if (_saveSO == null)
        {
            Debug.LogError("SaveFile: SaveSO is not assigned.");
            return;
        }

        if (Players == null)
        {
            Players = GameObject.FindGameObjectWithTag("Player");
        }

        if (Players == null)
        {
            Debug.LogError("SaveFile: Player object was not found, cannot save.");
            return;
        }

        YantraStatsController stats = Players.GetComponent<YantraStatsController>();
        if (stats == null)
        {
            Debug.LogError("SaveFile: YantraStatsController was not found on the player.");
            return;
        }

        _saveSO.SceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        _saveSO.PlayerPosition = Players.transform.position;
        _saveSO.PlayerRotation = Players.transform.eulerAngles;
        _saveSO.PlayerHealth = Mathf.RoundToInt(stats.GetCurrentHp());
        _saveSO.YantCount = stats.GetYantCount();

#if UNITY_EDITOR
        EditorUtility.SetDirty(_saveSO);
        AssetDatabase.SaveAssets();
#endif

        Debug.Log("SaveFile: Game saved successfully.");
    }
}
