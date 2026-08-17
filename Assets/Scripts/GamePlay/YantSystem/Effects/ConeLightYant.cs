using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ฉายแสงรูป cone ด้านหน้า เพื่อเช็ค enemy ที่เข้าไปใน cone
/// แล้วโจมตีแบบต่อเนื่องตาม interval และจำนวน enemy ที่ถูกครอบคลุม
/// </summary>
public class ConeLightYant : MonoBehaviour, IYantEffect
{
    [Header("Cone Settings")]
    [SerializeField] private float _coneAngle = 35f;
    [SerializeField] private float _range = 8f;
    [SerializeField] private float _beamDuration = 2.5f;
    [SerializeField] private float _tickInterval = 0.25f;

    [Header("Damage Settings")]
    [SerializeField] private float _damagePerTick = 5f;
    [SerializeField] private int _requiredTargetCount = 1;
    [SerializeField] private float _damageBoostPerExtraTarget = 0.5f;

    [Header("Target Filter")]
    [SerializeField] private LayerMask _targetMask = ~0;

    [Tooltip("ถ้าเปิด จะใช้แกน Y ของ Obj เป็นทิศด้านหน้า")]
    [SerializeField] private bool _frontAxisIsY = false;

    [Tooltip("เลื่อนจุดเริ่มต้นแสงออกจาก source")]
    [SerializeField] private Vector3 _originOffset = Vector3.zero;

    [Tooltip("Transform ที่ใช้เป็นจุดเริ่มและทิศทางของ Cone")]
    [SerializeField] private Transform _originTransform;

    [Header("Transform Settings")]
    [Tooltip("Rotation เพิ่มเติมของ Cone Object")]
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

    [Header("Beam Rotation")]
    [Tooltip("หมุน cone ไปพร้อมกับ effect หรือหมุนเองขณะใช้งาน")]
    [SerializeField] private bool _rotateWithBeam = false;

    [Tooltip("ความเร็วในการหมุน cone (องศาต่อวินาที)")]
    [SerializeField] private float _spinSpeed = 90f;

    [Tooltip("แกนที่ใช้หมุน cone")]
    [SerializeField] private Vector3 _spinAxis = Vector3.up;

    [Header("Debug")]
    [SerializeField] private bool _drawDebugCone = true;

    [SerializeField] private GameObject _owner;

    private Coroutine _beamRoutine;
    private bool _isBeamActive;

    private void Awake()
    {
        _originTransform = Camera.main != null
            ? Camera.main.transform
            : null;
    }

    public bool Initialize(GameObject playerRoot, bool holdLMB = false)
    {
        if (playerRoot == null)
        {
            Debug.LogWarning(
                "<color=#66CCFF>[ConeLightYant]</color> playerRoot ว่าง"
            );

            return false;
        }

        _owner = playerRoot;

        transform.rotation = _owner.transform.rotation;

        if (!holdLMB)
        {
            StopBeam();
            return true;
        }

        if (_isBeamActive)
        {
            return true;
        }

        if (_beamRoutine != null)
        {
            StopCoroutine(_beamRoutine);
        }

        _isBeamActive = true;
        _beamRoutine = StartCoroutine(BeamRoutine());

        return true;
    }

    private IEnumerator BeamRoutine()
    {
        float elapsedTime = 0f;

        while (_isBeamActive)
        {
            // หมุน Effect ขณะทำงาน
            if (_rotateWithBeam)
            {
                Vector3 axis = _spinAxis.sqrMagnitude > 0.0001f
                    ? _spinAxis.normalized
                    : Vector3.up;

                transform.Rotate(
                    axis,
                    _spinSpeed * Time.deltaTime,
                    Space.World
                );
            }

            ApplyDamage();

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

    private void StopBeam()
    {
        _isBeamActive = false;

        if (_beamRoutine != null)
        {
            StopCoroutine(_beamRoutine);
            _beamRoutine = null;
        }
    }

    /// <summary>
    /// หาจุดเริ่มต้นของ Cone
    /// ถ้ามี Origin Transform จะใช้ตำแหน่งของมัน
    /// ถ้าไม่มีจะใช้ตำแหน่งของ Effect ตัวเอง
    /// </summary>
    private Vector3 GetOrigin()
    {
        if (_originTransform != null)
        {
            return _originTransform.position +
                   _originTransform.TransformDirection(_originOffset);
        }

        return transform.position +
               transform.TransformDirection(_originOffset);
    }

    /// <summary>
    /// หาทิศทางการยิงของ Cone
    /// ถ้ามี Origin Transform จะใช้ Rotation ของมัน
    /// ถ้าไม่มีจะใช้ Rotation ของ Effect ตัวเอง
    /// </summary>
    private Vector3 GetForwardDirection()
    {
        if (_originTransform != null)
        {
            if (_frontAxisIsY)
            {
                return _originTransform.up.normalized;
            }

            return _originTransform.forward.normalized;
        }

        if (_frontAxisIsY)
        {
            return transform.up.normalized;
        }

        return transform.forward.normalized;
    }

    private void ApplyDamage()
    {
        Vector3 origin = GetOrigin();
        Vector3 forward = GetForwardDirection();

        Collider[] hits = Physics.OverlapSphere(
            origin,
            _range,
            _targetMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits == null || hits.Length == 0)
        {
            return;
        }

        List<IDamageable> validTargets =
            new List<IDamageable>();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];

            if (hit == null)
            {
                continue;
            }

            // ไม่โจมตี Owner
            if (_owner != null &&
                hit.transform.root == _owner.transform.root)
            {
                continue;
            }

            Vector3 targetDirection =
                (hit.bounds.center - origin).normalized;

            if (targetDirection.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            float angle =
                Vector3.Angle(forward, targetDirection);

            if (angle > _coneAngle * 0.5f)
            {
                continue;
            }

            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
            {
                continue;
            }

            // ป้องกัน Enemy ตัวเดียวโดนหลาย Collider
            if (!validTargets.Contains(damageable))
            {
                validTargets.Add(damageable);
            }
        }

        if (validTargets.Count < _requiredTargetCount)
        {
            return;
        }

        // จำนวนเป้าหมายเพิ่ม → Damage เพิ่ม
        float extraDamage =
            Mathf.Max(0, validTargets.Count - 1) *
            _damageBoostPerExtraTarget;

        float finalDamage =
            _damagePerTick + extraDamage;

        for (int i = 0; i < validTargets.Count; i++)
        {
            validTargets[i].TakeDamage(finalDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawDebugCone)
        {
            return;
        }

        Gizmos.color = new Color(
            0.2f,
            0.7f,
            1f,
            0.18f
        );

        Matrix4x4 originalMatrix = Gizmos.matrix;

        Vector3 debugOrigin = GetOrigin();

        // ใช้ Rotation ของ Origin Transform
        Quaternion debugRotation = _originTransform != null
            ? _originTransform.rotation
            : transform.rotation;

        Gizmos.matrix = Matrix4x4.TRS(
            debugOrigin,
            debugRotation,
            Vector3.one
        );

        Vector3 arcStart = Vector3.zero;

        float halfAngle = _coneAngle * 0.5f;

        int segments = 18;

        if (_frontAxisIsY)
        {
            // Cone ใช้แกน Y เป็น Forward
            for (int i = 0; i <= segments; i++)
            {
                float a =
                    Mathf.Deg2Rad *
                    (halfAngle * (2f * i / segments - 1f));

                Vector3 dir =
                    new Vector3(
                        Mathf.Sin(a),
                        Mathf.Cos(a),
                        0f
                    ).normalized;

                Vector3 end = dir * _range;

                Gizmos.DrawLine(
                    arcStart,
                    end
                );
            }

            // Center
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(
                    0f,
                    _range,
                    0f
                )
            );

            // ขอบซ้าย
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(
                    Mathf.Sin(Mathf.Deg2Rad * halfAngle),
                    Mathf.Cos(Mathf.Deg2Rad * halfAngle),
                    0f
                ) * _range
            );

            // ขอบขวา
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(
                    -Mathf.Sin(Mathf.Deg2Rad * halfAngle),
                    Mathf.Cos(Mathf.Deg2Rad * halfAngle),
                    0f
                ) * _range
            );
        }
        else
        {
            // Cone ใช้แกน Z เป็น Forward
            for (int i = 0; i <= segments; i++)
            {
                float a =
                    Mathf.Deg2Rad *
                    (halfAngle * (2f * i / segments - 1f));

                Vector3 dir =
                    new Vector3(
                        Mathf.Sin(a),
                        0f,
                        Mathf.Cos(a)
                    ).normalized;

                Vector3 end = dir * _range;

                Gizmos.DrawLine(
                    arcStart,
                    end
                );
            }

            // Center
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(
                    0f,
                    0f,
                    _range
                )
            );

            // ขอบซ้าย
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(
                    Mathf.Sin(Mathf.Deg2Rad * halfAngle),
                    0f,
                    Mathf.Cos(Mathf.Deg2Rad * halfAngle)
                ) * _range
            );

            // ขอบขวา
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(
                    -Mathf.Sin(Mathf.Deg2Rad * halfAngle),
                    0f,
                    Mathf.Cos(Mathf.Deg2Rad * halfAngle)
                ) * _range
            );
        }

        Gizmos.matrix = originalMatrix;
    }
}