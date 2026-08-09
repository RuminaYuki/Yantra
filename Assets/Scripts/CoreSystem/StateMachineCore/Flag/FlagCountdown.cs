using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlagCDData
{
    public FlagSO flag;
    public float countdownTime;
    public float remainingTime;
    public Coroutine countdownCoroutine;

    public FlagCDData(FlagSO flag, float countdownTime)
    {
        this.flag = flag;
        this.countdownTime = countdownTime;
    }
}

public class FlagCountdown : MonoBehaviour
{
    [SerializeField] private List<FlagCDData> flagCountdowns = new();
    [SerializeField] private StateFlags stateFlags;

    private void Awake()
    {
        if (stateFlags == null)
            stateFlags = GetComponent<StateFlags>();
    }

    public void SetFlagCountdown(FlagSO flag, float countdownTime, bool flagValue = true)
    {
        foreach (var flagData in flagCountdowns)
        {
            if (flagData.flag == flag)
            {
                if (flagData.countdownTime < countdownTime)
                    SetStartCountdown(flagData, countdownTime, flagValue);
                return;
            }
        }
        
        FlagCDData newFlagData = new FlagCDData(flag, countdownTime);
        flagCountdowns.Add(newFlagData);

        SetStartCountdown(newFlagData, countdownTime, flagValue);
    }
    
    public float GetRemainingTime(FlagSO flag)
    {
        foreach (var flagData in flagCountdowns)
        {
            if (flagData.flag == flag)
                return flagData.remainingTime;
        }
        return 0f;
    }

    public bool Contains(FlagSO flag)
    {
        foreach (var flagData in flagCountdowns)
        {
            if (flagData.flag == flag)
                return true;
        }
        return false;
    }

    private void SetStartCountdown(FlagCDData flagData, float countdownTime, bool flagValue = false)
    {
        stateFlags.Set(flagData.flag, flagValue);
        if (flagData.countdownCoroutine != null)
        {
            StopCoroutine(flagData.countdownCoroutine);
        }
        flagData.countdownTime = countdownTime;
        flagData.remainingTime = countdownTime;
        flagData.countdownCoroutine = StartCoroutine(CountdownCoroutine(flagData, !flagValue));
    }

    private IEnumerator CountdownCoroutine(FlagCDData flagData, bool flagValue = false)
    {
        while (flagData.remainingTime > 0)
        {
            flagData.remainingTime -= Time.deltaTime;
            yield return null;
        }
        stateFlags.Set(flagData.flag, flagValue);
        flagCountdowns.Remove(flagData);
    }

    private void OnDestroy()
    {
        foreach (var flagData in flagCountdowns)
        {
            if (flagData.countdownCoroutine != null)
            {
                StopCoroutine(flagData.countdownCoroutine);
            }
        }
        flagCountdowns.Clear();
    }
}
