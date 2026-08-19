using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ตัวสั่ง Damage ผ่าน CollisionDetector
/// เคลื่อน GameObject ตามตำแหน่ง _originTransform (กล้อง) + offset
/// </summary>
public class ConeColliderYant : MonoBehaviour, IYantEffect, IYantAnimationTiming
{
    [Header("Collision Detection")]
    [Tooltip("CollisionDetector ที่จะใช้ตรวจจับ targets")]
    [SerializeField] private CollisionDetector _collisionDetector;

    [Header("Damage Settings")]
    [SerializeField] private float _damagePerTick = 5f;
    [SerializeField] private int _requiredTargetCount = 1;
    [SerializeField] private float _damageBoostPerExtraTarget = 0.5f;

    [Header("Position Tracking")]
    [Tooltip("Transform ที่ใช้ตำแหน่งอ้างอิง (เช่น กล้อง)")]
    [SerializeField] private Transform _originTransform;

    [Tooltip("เลื่อนตำแหน่งออกจาก origin")]
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;

    [Header("Beam Settings")]
    [SerializeField] private float _beamDuration = 2.5f;
    [SerializeField] private float _tickInterval = 0.25f;
    [SerializeField] private BoolEventChannelSO _beamEventChannel;

    [SerializeField] private GameObject _owner;

    private Coroutine _beamRoutine;
    private Coroutine _damageRoutine;
    private bool _isBeamActive = false;

    private void Awake()
    {
        if (_originTransform == null)
        {
            _originTransform = Camera.main != null
                ? Camera.main.transform
                : null;
        }
    }

    private void Update()
    {
        // ติดตามตำแหน่ง origin + offset ตลอดเวลา
        if (_isBeamActive && _originTransform != null)
        {
            UpdatePosition();
        }
    }

    /// <summary>
    /// อัปเดตตำแหน่ง object ตามตำแหน่ง origin
    /// </summary>
    private void UpdatePosition()
    {
        Vector3 targetPos = _originTransform.position +
                           _originTransform.TransformDirection(_positionOffset);
        transform.position = targetPos;
        transform.rotation = _originTransform.rotation;
    }

    public bool Initialize(GameObject playerRoot, bool holdLMB = false)
    {
        if (playerRoot == null)
        {
            Debug.LogWarning(
                "<color=#66CCFF>[ConeColliderYant]</color> playerRoot ว่าง"
            );

            return false;
        }

        if (_collisionDetector == null)
        {
            Debug.LogWarning(
                "<color=#66CCFF>[ConeColliderYant]</color> CollisionDetector ว่าง"
            );

            return false;
        }

        _owner = playerRoot;
        _collisionDetector.SetOwnerToIgnore(_owner);

        transform.rotation = _owner.transform.rotation;

        if (!holdLMB)
        {
            StopBeam();
        }

        return true;
    }

    public void TriggerAnimationTiming()
    {
        _isBeamActive = !_isBeamActive;
        _beamEventChannel.Raise(_isBeamActive);
        _beamRoutine = StartCoroutine(BeamRoutine());
        _damageRoutine = StartCoroutine(DamageRoutine());
        Debug.Log($"Beam active {_isBeamActive}");
    }

    private IEnumerator BeamRoutine()
    {
        float elapsedTime = 0f;

        while (_isBeamActive)
        {
            elapsedTime += _tickInterval;

            // หมดเวลาการยิง
            if (_beamDuration > 0f &&
                elapsedTime >= _beamDuration)
            {
                StopBeam();
                yield break;
            }

            yield return new WaitForSeconds(_tickInterval);
        }
    }

    /// <summary>
    /// Coroutine สำหรับใช้ damage ตามช่วงเวลา
    /// ตรวจสอบ cone angle และใช้ damage ที่ targets ที่เหลืออยู่
    /// </summary>
    private IEnumerator DamageRoutine()
    {
        while (_isBeamActive)
        {
            ApplyDamageToActiveTargets();
            yield return new WaitForSeconds(_tickInterval);
        }
    }

    private void StopBeam()
    {
        _isBeamActive = false;
        _collisionDetector.ClearTargets();

        if (_beamRoutine != null)
        {
            StopCoroutine(_beamRoutine);
            _beamRoutine = null;
        }

        if (_damageRoutine != null)
        {
            StopCoroutine(_damageRoutine);
            _damageRoutine = null;
        }
    }

    /// <summary>
    /// ใช้ damage กับ targets ที่ยังอยู่ใน collision
    /// </summary>
    private void ApplyDamageToActiveTargets()
    {
        List<IDamageable> targets =
            _collisionDetector.GetActiveTargets();

        if (targets.Count < _requiredTargetCount)
        {
            return;
        }

        // จำนวนเป้าหมายเพิ่ม → Damage เพิ่ม
        float extraDamage =
            Mathf.Max(0, targets.Count - 1) *
            _damageBoostPerExtraTarget;

        float finalDamage =
            _damagePerTick + extraDamage;

        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].TakeDamage(finalDamage);
        }
    }

    /// <summary>
    /// หาจุดเริ่มต้นของ effect
    /// </summary>
    private Vector3 GetOrigin()
    {
        return transform.position +
               transform.TransformDirection(_positionOffset);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.3f);
        Gizmos.DrawWireSphere(GetOrigin(), 0.3f);
    }
}
