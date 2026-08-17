using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Objective : MonoBehaviour
{
    [SerializeField] private ObjectiveSO objectiveData;
    
    [Tooltip("Filter and add IObjectiveTarget from MonoBehaviour to objectiveTargets list.")]
    [SerializeField] private List<MonoBehaviour> SetobjectiveTargetsInspecter = new();
    private List<IObjectiveTarget> objectiveTargets = new();
    
    public Action<string> onObjectiveCompleted;
    public Action<int> onObjectiveChange;
    
    private int currentProgress = 0;
    private bool isCompleted = false;

    public int CurrentProgress => currentProgress;
    public int TargetCount => objectiveData != null ? objectiveData.TargetCount : 0;
    public bool IsCompleted => isCompleted;

    void Awake()
    {
        if (objectiveData == null)
        {
            Debug.LogError("ObjectiveSO is not assigned!", this);
            return;
        }

        filterIObjectiveTarget();
        SetupTargets();
    }

    void SetupTargets()
    {
        // Setup each target and pass objective reference
        foreach (var target in objectiveTargets)
        {
            if (target is InteractionPoint interactionPoint)
            {
                interactionPoint.Setup(this);
            }
        }
    }

    void filterIObjectiveTarget()
    {
        for (int i = SetobjectiveTargetsInspecter.Count - 1; i >= 0; i--)
        {
            if (SetobjectiveTargetsInspecter[i] is IObjectiveTarget target)
            {
                objectiveTargets.Add(target);
            }
        }
        SetobjectiveTargetsInspecter.Clear();
    }

    public void OnTargetCompleted()
    {
        if (isCompleted)
            return;

        currentProgress++;
        onObjectiveChange?.Invoke(currentProgress);
        Debug.Log($"Progress: {currentProgress}/{TargetCount}");

        if (currentProgress >= TargetCount)
        {
            CompleteObjective();
        }
    }

    void CompleteObjective()
    {
        isCompleted = true;
        Debug.Log($"🎉 Objective Completed: {objectiveData.ObjectiveName}");
        
        // Invoke UnityEvent
        onObjectiveCompleted?.Invoke(objectiveData.ObjectiveName);
    }
}

public interface IObjectiveTarget
{
    bool IsComplete { get; }
    void Complete();
}
