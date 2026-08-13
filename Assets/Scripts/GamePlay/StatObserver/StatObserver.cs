using System;
using System.Collections.Generic;
using UnityEngine;

public class StatObserver : MonoBehaviour
{
    [Serializable]
    private class StatEntry
    {
        public StatSO Stat;
        public bool InitialValue;

#if UNITY_EDITOR
        [SerializeField] private bool runtimeValue;

        public bool RuntimeValue
        {
            get => runtimeValue;
            set => runtimeValue = value;
        }
#endif
    }

    [SerializeField] private List<StatEntry> stats = new();

    private readonly Dictionary<StatSO, bool> values = new();

    public event Action<StatSO, bool> OnStatChanged;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        values.Clear();

        foreach (StatEntry entry in stats)
        {
            if (entry.Stat == null)
                continue;

            if (values.ContainsKey(entry.Stat))
            {
                Debug.LogWarning(
                    $"Duplicate stat '{entry.Stat.name}' on {name}.",
                    this
                );

                continue;
            }

            values.Add(entry.Stat, entry.InitialValue);

#if UNITY_EDITOR
            entry.RuntimeValue = entry.InitialValue;
#endif
        }
    }

    public bool Get(StatSO stat)
    {
        if (stat == null)
            return false;

#if UNITY_EDITOR
        foreach (StatEntry entry in stats)
        {
            if (entry.Stat == stat)
                return entry.RuntimeValue;
        }
#endif

        return values.TryGetValue(stat, out bool value) && value;
    }

    public void Set(StatSO stat, bool value)
    {
        if (stat == null)
        {
            Debug.LogWarning(
                "Cannot set a null stat.",
                this
            );

            return;
        }

        if (!values.TryGetValue(stat, out bool currentValue))
        {
            Debug.LogWarning(
                $"Stat '{stat.name}' is not registered on {name}.",
                this
            );

            return;
        }

        // ไม่มีการเปลี่ยนแปลง ไม่ต้อง Invoke
        if (currentValue == value)
            return;

        values[stat] = value;

#if UNITY_EDITOR
        foreach (StatEntry entry in stats)
        {
            if (entry.Stat != stat)
                continue;

            entry.RuntimeValue = value;
            break;
        }
#endif

        OnStatChanged?.Invoke(stat, value);
    }

    public bool Contains(StatSO stat)
    {
        return stat != null && values.ContainsKey(stat);
    }

    public bool Add(StatSO stat, bool initialValue = false)
    {
        if (stat == null)
        {
            Debug.LogWarning(
                $"Cannot add a null stat on {name}.",
                this
            );

            return false;
        }

        if (values.ContainsKey(stat))
            return false;

        values.Add(stat, initialValue);

#if UNITY_EDITOR
        stats.Add(new StatEntry
        {
            Stat = stat,
            InitialValue = initialValue
        });
#endif

        return true;
    }

    public bool this[StatSO stat]
    {
        get => Get(stat);
        set => Set(stat, value);
    }

    public void ResetToInitialValues()
    {
        foreach (StatEntry entry in stats)
        {
            if (entry.Stat == null)
                continue;

            Set(entry.Stat, entry.InitialValue);
        }
    }
}