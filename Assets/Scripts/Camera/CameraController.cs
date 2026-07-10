using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// คลาสสำหรับควบคุมมุมกล้องแบบมุมมองบุคคลที่หนึ่ง (FPP)
/// รองรับระบบ Free Look เมื่อกดเมาส์กลางค้างไว้ พร้อมจำกัดองศาการหัน
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private bool _executeAlways = true;

    [Header("FPP Camera Settings")]
    [SerializeField] private bool _enableMouseLook = true;
    [SerializeField] private float _mouseSensitivity = 0.1f;
    [SerializeField] private bool _invertY = false;

    [Header("Free Look Settings")]
    [SerializeField] private YantraInputObserverSO _inputObserverChannel;

    [SerializeField] private float _maxFreeLookAngle = 90f;

    private float _xRotation = 0f;
    private float _yRotation = 0f;

    private bool _isFreeLooking = false;
    private float _lockedYRotation; // เก็บค่าแกน Y เดิมตอนเริ่มกด Free Look

    public void ToggleMouseLook() => _enableMouseLook = !_enableMouseLook;
    public void SetMouseLook(bool enable) => _enableMouseLook = enable;

    private void Start()
    {
        if (Application.isPlaying)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Vector3 currentEuler = transform.localEulerAngles;
        _xRotation = currentEuler.x;
        _yRotation = currentEuler.y;

        if (_xRotation > 180) _xRotation -= 360f;
    }

    private void OnEnable()
    {
        if (_inputObserverChannel != null)
        {
            _inputObserverChannel.OnMiddleClickChannel += HandleFreeLook;
        }
    }

    private void OnDisable()
    {
        if (_inputObserverChannel != null)
        {
            _inputObserverChannel.OnMiddleClickChannel -= HandleFreeLook;
        }
    }

    private void HandleFreeLook(Vector2 mousePos, bool isPressed)
    {
        if (isPressed && !_isFreeLooking)
        {
            _lockedYRotation = _yRotation;
        }
        else if (!isPressed && _isFreeLooking)
        {
            _yRotation = _lockedYRotation;
        }

        _isFreeLooking = isPressed;
    }

    protected virtual void LateUpdate()
    {
        if (!_executeAlways && !Application.isPlaying) return;
        if (!_enableMouseLook) return;
        if (Mouse.current == null) return;

        Vector2 lookInput = Mouse.current.delta.ReadValue();

        float mouseX = lookInput.x * _mouseSensitivity;
        float mouseY = lookInput.y * _mouseSensitivity;

        if (_invertY) mouseY *= -1;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -85f, 85f);

        _yRotation += mouseX;

        if (_isFreeLooking)
        {
            _yRotation = Mathf.Clamp(_yRotation, _lockedYRotation - _maxFreeLookAngle, _lockedYRotation + _maxFreeLookAngle);

            float currentLookAngle = _yRotation - _lockedYRotation;
            Debug.Log($"[Free Look Debug] หันคอไปแล้ว: {currentLookAngle:F1} องศา");
        }

        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }
}