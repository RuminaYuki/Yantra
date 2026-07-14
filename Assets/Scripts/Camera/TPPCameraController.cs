using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// คลาสควบคุมมุมกล้องบุคคลที่สาม (Orbit Camera)
/// กล้องจะหมุนรอบเป้าหมายและรักษาระยะห่างที่กำหนด
/// </summary>
public class TPPCameraController : MonoBehaviour
{
    [SerializeField] private bool _executeAlways = true;

    [Header("TPP Camera Settings")]
    [SerializeField] private Transform _target; // จุดที่กล้องจะโฟกัส (เช่น Empty Object ที่ตำแหน่งอกตัวละคร)
    [SerializeField] private float _distance = 5f; // ระยะห่างจากตัวละคร
    [SerializeField] private bool _enableMouseLook = true;
    [SerializeField] private float _mouseSensitivity = 0.1f;
    [SerializeField] private bool _invertY = false;

    [Header("Rotation Limits")]
    [SerializeField] private float _minPitch = -20f;
    [SerializeField] private float _maxPitch = 70f;

    private float _xRotation = 0f; // Pitch (ขึ้น/ลง)
    private float _yRotation = 0f; // Yaw (ซ้าย/ขวา)

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
    }

    protected virtual void LateUpdate()
    {
        if (!_executeAlways && !Application.isPlaying) return;
        if (!_enableMouseLook || _target == null) return;
        if (Mouse.current == null) return;

        Vector2 lookInput = Mouse.current.delta.ReadValue();

        float mouseX = lookInput.x * _mouseSensitivity;
        float mouseY = lookInput.y * _mouseSensitivity;

        if (_invertY) mouseY *= -1;

        // คำนวณองศาการหมุน
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, _minPitch, _maxPitch); // จำกัดมุมก้มเงย
        _yRotation += mouseX;

        // คำนวณ Rotation และ Position อิงจาก Target
        Quaternion rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        Vector3 position = _target.position - (rotation * Vector3.forward * _distance);

        transform.position = position;
        transform.rotation = rotation;
    }
}