using System.Collections;
using UnityEngine;

public class CameraAnimationController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private YantraInputObserverSO _inputObserver;

    [Header("Look At Settings")]
    [SerializeField] private Transform _targetToLook;
    [SerializeField] private float _rotationSpeed = 5f;

    [Tooltip("ปรับองศาหันเห (X, Y, Z) สามารถปรับแบบ Real-time ได้")]
    [SerializeField] private Vector3 _rotationOffset;

    [SerializeField] private bool _isDrawing;

    private Quaternion _cachedRotation;
    private Coroutine _lookRoutine;

    private void OnValidate()
    {
        if (!_animator) TryGetComponent(out _animator);
    }

    public void OnDrawEvent(bool isDrawing)
    {
        _isDrawing = isDrawing;
        _animator.SetBool("Draw", _isDrawing);

        if (_lookRoutine != null)
        {
            StopCoroutine(_lookRoutine);
        }

        if (_isDrawing)
        {
            // บันทึกมุมเริ่มต้นเก็บไว้เฉพาะตอนเริ่มวาด
            _cachedRotation = transform.rotation;
        }

        // เรียก Coroutine ชุดเดียวให้จัดการทั้งหมด
        _lookRoutine = StartCoroutine(SmoothRotateRoutine());
    }

    private IEnumerator SmoothRotateRoutine()
    {
        // วนลูปทำงานไปเรื่อยๆ เพื่อให้รับค่า Offset แบบ Real-time ได้
        while (true)
        {
            Quaternion targetRotation;

            if (_isDrawing)
            {
                // คำนวณทิศทางใหม่ทุกเฟรม (ทำให้ขยับเป้าหมาย หรือแก้ค่า Offset ใน Inspector แล้วเห็นผลทันที)
                Vector3 directionToTarget = _targetToLook.position - transform.position;
                targetRotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(_rotationOffset);
            }
            else
            {
                // ถ้าหยุดวาด ให้เป้าหมายคือมุมกล้องเดิม
                targetRotation = _cachedRotation;
            }

            // ค่อยๆ หมุนไปยังมุมเป้าหมาย
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            // เงื่อนไขการหยุด: ถ้าไม่ได้กำลังวาดอยู่ และหมุนกลับมาถึงจุดเดิมแล้ว ให้หยุด Coroutine เพื่อประหยัดทรัพยากร
            if (!_isDrawing && Quaternion.Angle(transform.rotation, targetRotation) <= 0.1f)
            {
                transform.rotation = targetRotation;
                break;
            }

            yield return null;
        }
    }
}