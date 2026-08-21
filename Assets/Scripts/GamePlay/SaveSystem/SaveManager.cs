using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [SerializeField] private SlotSaveSO SlotSaveSO;
    private List<ISave> ListISave = new();
    private List<ISaveLoad> ListISaveLoad = new();

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

    private void FindAllISave()
    {
        ListISave.Clear();

        MonoBehaviour[] monoBehaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (MonoBehaviour monoBehaviour in monoBehaviours)
        {
            if (monoBehaviour is ISave save)
            {
                ListISave.Add(save);
            }
        }
    }

    private void FindAllISaveLoad()
    {
        ListISaveLoad.Clear();

        MonoBehaviour[] monoBehaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (MonoBehaviour monoBehaviour in monoBehaviours)
        {
            if (monoBehaviour is ISaveLoad save)
            {
                ListISaveLoad.Add(save);
            }
        }
    }

    public void SaveAll()
    {
        if (SlotSaveSO == null)
        {
            Debug.LogWarning("Save ignored: no SlotSaveSO assigned.");
            return;
        }

        FindAllISave();

        if (ListISave.Count == 0) return;

        foreach (ISave save in ListISave)
        {
            save.Save(SlotSaveSO);
        }
    }

    public void LoadAll()
    {
        if (SlotSaveSO == null)
        {
            Debug.LogWarning("Load ignored: no SlotSaveSO assigned.");
            return;
        }

        FindAllISaveLoad();

        if (ListISaveLoad.Count == 0) return;

        foreach (ISaveLoad save in ListISaveLoad)
        {
            save.Load(SlotSaveSO);
        }
    }
}
