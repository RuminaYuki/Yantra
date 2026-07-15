using System.Collections;
using UnityEngine;

public class CameraAnimationController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    [Header("Look At Settings")]
    [SerializeField] private Transform _targetToLook;
    [SerializeField] private float _rotationSpeed = 5f;

    [Tooltip("ปรับองศาหันเห (X, Y, Z) สามารถปรับแบบ Real-time ได้")]
    [SerializeField] private Vector3 _rotationOffset;

    [SerializeField] private bool _isDrawing;

    private Quaternion _cachedRotation;
    private Coroutine _lookRoutine;

    public bool IsAnimationActive => _isDrawing;
    public Quaternion DesiredRotation { get; private set; }

    private void OnValidate()
    {
        if (!_animator) TryGetComponent(out _animator);
    }

    public void OnDrawEvent(bool isDrawing)
    {
        Debug.Log($"OnDrawEvent called with isDrawing: {isDrawing}");
        if (!_isDrawing && Quaternion.Angle(transform.rotation, _targetToLook.rotation) <= 0.1f)
            return;

        _isDrawing = isDrawing;
        if (_animator != null)
        {
            _animator.SetBool("Draw", _isDrawing);
        }

        if (_lookRoutine != null)
        {
            StopCoroutine(_lookRoutine);
        }

        if (_isDrawing)
        {
            _cachedRotation = transform.rotation;
            DesiredRotation = _cachedRotation;
        }

        _lookRoutine = StartCoroutine(SmoothRotateRoutine());
    }

    private IEnumerator SmoothRotateRoutine()
    {
        // วนลูปทำงานไปเรื่อยๆ เพื่อให้รับค่า Offset แบบ Real-time ได้
        while (true)
        {
            Quaternion targetRotation;

            if (_isDrawing && _targetToLook != null)
            {
                Vector3 directionToTarget = _targetToLook.position - transform.position;
                targetRotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(_rotationOffset);
            }
            else
            {
                targetRotation = _cachedRotation;
            }

            DesiredRotation = targetRotation;

            if (!_isDrawing && Quaternion.Angle(transform.rotation, targetRotation) <= 0.1f)
            {
                DesiredRotation = targetRotation;
                break;
            }

            yield return null;
        }
    }
}