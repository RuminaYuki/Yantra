using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyToStart
{
    public KeyCode key;
    public List<waitAndTriger> ListwaitAndTrigers;
}

[System.Serializable]
public class waitAndTriger
{
    public string nameTrigger;
    public float waitTime;
    public KeyCode nextKeyRequired = KeyCode.None;

    [Tooltip("ถ้าติดลบ จะรอไปเรื่อยๆ / ถ้ามากกว่า 0 หากหมดเวลาจะทำงานตามเงื่อนไข Timeout")]
    public float inputTimeout = -1f;

    [Header("Loop Key Settings")]
    [Tooltip("ถ้าติ๊กถูก: จะกดปุ่มนี้ซ้ำรัวๆ ได้จนกว่าจะหมดเวลา โดยไม่ข้ามไปไอเทมถัดไป \nถ้าไม่ติ๊ก: กดติดครั้งเดียวจะข้ามไปไอเทมถัดไปทันที")]
    public bool loopKeyUntilTimeout = false; // ตัวเลือกเปิด-ปิดระบบวนลูปที่ต้องการ

    [Header("Timeout Settings")]
    public bool doElseIfTimeout = false;
    public string elseTriggerName;
}

public class TestGamePlayControl : MonoBehaviour
{
    public YantraInputObserverSO _inputObserver;
    public Animator _animator;

    [SerializeField] private List<KeyToStart> _KeyToStart;

    private Coroutine _currentSequenceCoroutine;
    private bool _isSequenceRunning = false;

    private void Update()
    {
        if (_isSequenceRunning) return;
        StartTest();
    }

    private void StartTest()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyToStart keyData in _KeyToStart)
            {
                if (Input.GetKeyDown(keyData.key))
                {
                    ResetActiveSequence();
                    _currentSequenceCoroutine = StartCoroutine(RunTriggersInSequence(keyData.ListwaitAndTrigers));
                    break;
                }
            }
        }
    }

    private IEnumerator RunTriggersInSequence(List<waitAndTriger> triggerList)
    {
        _isSequenceRunning = true;

        foreach (waitAndTriger trigger in triggerList)
        {
            bool isKeyMatched = false;
            bool isTimedOut = false;
            bool hasPressedAtLeastOnce = false;

            // 1. เงื่อนไขการรอตรวจจับปุ่มกด
            if (trigger.nextKeyRequired != KeyCode.None)
            {
                float timeRemaining = trigger.inputTimeout;

                // วนลูปเช็คจนกว่าจะหมดเวลา หรือ กดปุ่มติด (กรณีไม่ได้ตั้งให้วนลูปรัวๆ)
                while (!isTimedOut && !isKeyMatched)
                {
                    if (Input.GetKeyDown(trigger.nextKeyRequired))
                    {
                        hasPressedAtLeastOnce = true;

                        // สั่งรันแอนิเมชันทันทีที่กดปุ่มสำเร็จ
                        _animator.ResetTrigger(trigger.nameTrigger);
                        _animator.SetTrigger(trigger.nameTrigger);

                        // เช็คตัวเลือก bool ที่เพิ่มเข้ามา
                        if (trigger.loopKeyUntilTimeout)
                        {
                            Debug.Log($"<color=green>[Loop Key]</color> กดรัวปุ่ม {trigger.nextKeyRequired} สำเร็จ! รันแอนิเมชัน: {trigger.nameTrigger}");
                            // ไม่ตั้งให้ isKeyMatched = true เพื่อให้ลูปวิ่งต่อไปตรวจจับการกดปุ่มครั้งถัดไปได้อีก
                        }
                        else
                        {
                            Debug.Log($"<color=green>[Single Key]</color> กดปุ่ม {trigger.nextKeyRequired} ติดแล้ว! เตรียมข้ามไปไอเทมถัดไป");
                            isKeyMatched = true; // หลุดลูปทันทีเพื่อไปไอเทมถัดไป
                        }
                    }

                    // นับเวลาถอยหลัง Timeout
                    if (trigger.inputTimeout > 0)
                    {
                        timeRemaining -= Time.deltaTime;
                        if (timeRemaining <= 0)
                        {
                            isTimedOut = true;
                        }
                    }

                    yield return null;
                }
            }

            // 2. จัดการโลจิกหลังจากหลุดลูปออกมา (หมดเวลา หรือ กดผ่านสำเร็จ)
            if (isTimedOut)
            {
                Debug.Log($"<color=orange>[Timeout]</color> หมดเวลาสำหรับปุ่ม {trigger.nextKeyRequired}!");

                if (trigger.doElseIfTimeout && !string.IsNullOrEmpty(trigger.elseTriggerName))
                {
                    Debug.Log($"<color=yellow>[Action]</color> เล่นท่า else {trigger.elseTriggerName} แทน");
                    _animator.ResetTrigger(trigger.elseTriggerName);
                    _animator.SetTrigger(trigger.elseTriggerName);
                }
                else if (!trigger.loopKeyUntilTimeout && !hasPressedAtLeastOnce)
                {
                    // กรณีเป็นปุ่มปกติ แต่ผู้เล่นไม่กดเลยจนหมดเวลา ให้บังคับรันท่าหลักส่งท้าย
                    if (trigger.waitTime > 0) yield return new WaitForSeconds(trigger.waitTime);
                    _animator.ResetTrigger(trigger.nameTrigger);
                    _animator.SetTrigger(trigger.nameTrigger);
                }
            }
            else
            {
                // ถ้าไม่ใช่กรณี Timeout (เช่น เป็นไอเทมที่รอเวลาเฉยๆ ไม่มีปุ่มกด หรือกด Single Key ผ่านฉลุย)
                // ให้หน่วงเวลา WaitTime ก่อนรันท่าถัดไปตามปกติ
                if (!trigger.loopKeyUntilTimeout)
                {
                    if (trigger.waitTime > 0) yield return new WaitForSeconds(trigger.waitTime);

                    // ป้องกันการ SetTrigger ซ้ำซ้อนถ้าเป็นปุ่มเดี่ยวที่เพิ่งกดสั่งเล่นไปในลูปข้างบน
                    if (trigger.nextKeyRequired == KeyCode.None)
                    {
                        _animator.ResetTrigger(trigger.nameTrigger);
                        _animator.SetTrigger(trigger.nameTrigger);
                    }
                }
            }
        }

        _isSequenceRunning = false;
    }

    private void ResetActiveSequence()
    {
        if (_currentSequenceCoroutine != null)
        {
            StopCoroutine(_currentSequenceCoroutine);
            _currentSequenceCoroutine = null;
        }
        _isSequenceRunning = false;
    }
}
