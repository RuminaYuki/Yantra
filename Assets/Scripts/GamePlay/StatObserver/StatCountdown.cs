using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatCountdownData
{
    public StatSO stat;
    public float countdownTime;
    public float remainingTime;

    [NonSerialized]
    public Coroutine countdownCoroutine;

    public StatCountdownData(StatSO stat, float countdownTime)
    {
        this.stat = stat;
        this.countdownTime = countdownTime;
        remainingTime = countdownTime;
    }
}

public class StatCountdown : MonoBehaviour
{
    [SerializeField] private List<StatCountdownData> countdowns = new();
    [SerializeField] private StatObserver statObserver;

    private void Awake()
    {
        if (statObserver == null)
            statObserver = GetComponent<StatObserver>();

        if (statObserver == null)
        {
            Debug.LogError(
                $"StatCountdown on {name} requires a StatObserver.",
                this
            );
        }
    }

    /// <summary>
    /// Sets a stat to the specified value for a duration.
    /// If the stat already has a countdown, the countdown will only
    /// be extended when the new duration is greater than the remaining time.
    /// </summary>
    public void SetStatCountdown(
        StatSO stat,
        float countdownTime,
        bool statValue = true)
    {
        if (stat == null)
        {
            Debug.LogWarning(
                $"Cannot start countdown with null stat on {name}.",
                this
            );
            return;
        }

        if (statObserver == null)
            return;

        if (!statObserver.Contains(stat))
        {
            Debug.LogWarning(
                $"Stat '{stat.name}' is not registered on {name}.",
                this
            );
            if (!statObserver.Add(stat))
                return;
        }

        if (countdownTime <= 0f)
        {
            statObserver.Set(stat, statValue);
            StopCountdown(stat);
            return;
        }

        StatCountdownData existingData = FindCountdown(stat);

        if (existingData != null)
        {
            // ถ้าเวลาที่ขอมาไม่มากกว่าเวลาที่เหลือ
            // ไม่ต้อง reset countdown
            if (countdownTime <= existingData.remainingTime)
                return;

            StartCountdown(
                existingData,
                countdownTime,
                statValue
            );

            return;
        }

        StatCountdownData newData =
            new StatCountdownData(stat, countdownTime);

        countdowns.Add(newData);

        StartCountdown(
            newData,
            countdownTime,
            statValue
        );
    }

    public float GetRemainingTime(StatSO stat)
    {
        StatCountdownData data = FindCountdown(stat);

        return data != null
            ? data.remainingTime
            : 0f;
    }

    public bool Contains(StatSO stat)
    {
        return FindCountdown(stat) != null;
    }

    private void StartCountdown(
        StatCountdownData data,
        float countdownTime,
        bool statValue)
    {
        if (data.countdownCoroutine != null)
        {
            StopCoroutine(data.countdownCoroutine);
        }

        data.countdownTime = countdownTime;
        data.remainingTime = countdownTime;

        statObserver.Set(data.stat, statValue);

        data.countdownCoroutine =
            StartCoroutine(
                CountdownCoroutine(
                    data,
                    !statValue
                )
            );
    }

    private IEnumerator CountdownCoroutine(
        StatCountdownData data,
        bool finalValue)
    {
        while (data.remainingTime > 0f)
        {
            data.remainingTime -= Time.deltaTime;

            yield return null;
        }

        data.remainingTime = 0f;

        if (statObserver != null)
        {
            statObserver.Set(
                data.stat,
                finalValue
            );
        }

        RemoveCountdown(data);
    }

    public void StopCountdown(StatSO stat)
    {
        StatCountdownData data = FindCountdown(stat);

        if (data == null)
            return;

        if (data.countdownCoroutine != null)
        {
            StopCoroutine(data.countdownCoroutine);
            data.countdownCoroutine = null;
        }

        countdowns.Remove(data);
    }

    private StatCountdownData FindCountdown(StatSO stat)
    {
        if (stat == null)
            return null;

        foreach (StatCountdownData data in countdowns)
        {
            if (data.stat == stat)
                return data;
        }

        return null;
    }

    private void RemoveCountdown(StatCountdownData data)
    {
        if (data == null)
            return;

        data.countdownCoroutine = null;
        countdowns.Remove(data);
    }

    private void OnDestroy()
    {
        countdowns.Clear();
    }
}