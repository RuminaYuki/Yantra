using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FatalFrameCameraController : MonoBehaviour
{
    [Header("Input Observer")]
    [SerializeField] private YantraInputObserverSO _inputObserverChannel;

    [Header("Camera States")]
    public bool IsGunAiming = false;    // คลิกขวา (ปืน)
    public bool IsYantraAiming = false; // กด Q (ยันต์)
    [SerializeField] private CameraAnimationController _cameraAnimationController;

    [Header("Targets")]
    [SerializeField] private Transform _tppPivot;
    [SerializeField] private Transform _fppEyePosition;
    [SerializeField] private Transform _otsPivot; // ✨ เพิ่มช่องสำหรับใส่จุดเล็งข้ามไหล่
    [SerializeField] private Transform _yantraPivot; // เพิ่มช่องสำหรับใส่จุดเล็งยันต์
    [Tooltip("เพิ่มการก้มหน้า +ก้มลง -ก้มขึ้น")]
    [SerializeField] private int _yantraOffsetRotationY;

    [Header("TPP Settings")]
    [SerializeField] private Vector3 _tppOffset = new Vector3(0.6f, 0.2f, -2.5f);
    [SerializeField] private float _tppFOV = 60f;

    [Header("FPP / OTS Settings")]
    [SerializeField] private float _fppFOV = 40f;
    [SerializeField] private float _otsFOV = 50f; // มุมกล้องตอนเล็งปืน

    [Header("Controls")]
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _transitionSpeed = 12f;
    [SerializeField] private float _minPitch = -40f;
    [SerializeField] private float _maxPitch = 60f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _cameraRadius = 0.2f;
    [SerializeField] private float _minDistance = 0.5f;

    private bool _ChangeCameraFinish = false;

    private Camera _mainCamera;
    private float _pitch = 0f;
    private float _yaw = 0f;

    public bool _isFreeLookingInBook = false;

    private void Start()
    {
        _mainCamera = GetComponent<Camera>();

        if (Application.isPlaying)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (_tppPivot != null)
        {
            _pitch = 10f;
            _yaw = _tppPivot.eulerAngles.y;
        }
    }

    private void OnEnable()
    {
        if (_inputObserverChannel != null)
        {
            _inputObserverChannel.OnPressCtalR_ButtonChannel += HandleLookAround;
            _inputObserverChannel.OnPressQ_ButtonChannel += HandleQ;
        }
    }

    private void OnDisable()
    {
        if (_inputObserverChannel != null)
        {
            _inputObserverChannel.OnPressCtalR_ButtonChannel -= HandleLookAround;
            _inputObserverChannel.OnPressQ_ButtonChannel -= HandleQ;
        }
    }

    private void Update()
    {
        // if (Mouse.current != null)
        // {
        //     // บังคับว่าถ้ากางยันต์อยู่ จะห้ามเล็งปืนซ้อน
        //     if (!IsYantraAiming)
        //     {
        //         if (Mouse.current.rightButton.wasPressedThisFrame) IsGunAiming = true;
        //         if (Mouse.current.rightButton.wasReleasedThisFrame) IsGunAiming = false;
        //     }
        // }
    }

    private void LateUpdate()
    {
        if (_tppPivot == null || _fppEyePosition == null || Mouse.current == null) return;

        if (!IsYantraAiming || _isFreeLookingInBook)
        {
            // ถ้าไม่ได้กางสมุด ให้ขยับกล้องด้วยเมาส์ได้ตามปกติ
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            _yaw += mouseDelta.x * _mouseSensitivity;
            _pitch -= mouseDelta.y * _mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }
        else
        {
            // ถ้ากางสมุดอยู่ (IsYantraAiming == true)
            // บังคับก้มหน้าลง 45 องศา (เปลี่ยนตัวเลข 45f ได้ตามความเหมาะสม)
            // ค่อยๆ ก้มลงอย่างสมูทด้วย Lerp
            Quaternion Rotation = Quaternion.LookRotation(_yantraPivot.position - transform.position);
            _pitch = Mathf.Lerp(_pitch, Rotation.y + _yantraOffsetRotationY, Time.deltaTime * 5f);
            // หมายเหตุ: เราไม่ยุ่งกับ _yaw ผู้เล่นจะหันไปทางเดิมก่อนเปิดสมุด
        }

        Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPosition;
        float targetFOV;

        if (IsYantraAiming)
        {
            // โหมด FPP (กางยันต์)
            desiredPosition = _fppEyePosition.position;
            targetFOV = _fppFOV;
        }
        else if (IsGunAiming)
        {
            // ✨ โหมด OTS (เล็งปืนข้ามไหล่)
            // ถ้ามีพิกัด _otsPivot ให้ใช้พิกัดนั้น ถ้าไม่มีให้กะระยะเอาจากจุดหมุนตัวละคร
            desiredPosition = _otsPivot != null ? _otsPivot.position : _tppPivot.position + (targetRotation * new Vector3(0.5f, 0.1f, -1f));
            targetFOV = _otsFOV;
        }
        else
        {
            // โหมด TPP (เดินสำรวจปกติ)
            Vector3 rotatedOffset = targetRotation * _tppOffset;
            desiredPosition = _tppPivot.position + rotatedOffset;
            targetFOV = _tppFOV;

            Vector3 direction = desiredPosition - _tppPivot.position;
            float desiredDistance = direction.magnitude;
            direction.Normalize();

            if (Physics.SphereCast(_tppPivot.position, _cameraRadius, direction, out RaycastHit hit, desiredDistance, _collisionMask))
            {
                float adjustedDistance = Mathf.Max(hit.distance, _minDistance);
                desiredPosition = _tppPivot.position + direction * adjustedDistance;
            }
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _transitionSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _transitionSpeed);
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, Time.deltaTime * _transitionSpeed);
        
    }
    
    private void HandleLookAround()
    {
        bool freelook = _isFreeLookingInBook;
        _isFreeLookingInBook = !freelook;
        Debug.Log($"HandleLookAround called. IsYantraAiming: {IsYantraAiming}, _isFreeLookingInBook: {_isFreeLookingInBook}");
    }

    private void HandleQ()
    {
        _isFreeLookingInBook = IsYantraAiming ? false : true; // ถ้ากำลังกางสมุดอยู่แล้วกด Q อีกครั้ง ให้ปิดโหมด Free Look
    }

    //API
    public Vector2 CameraRotation => new Vector2(_yaw, _pitch);
    public float MinPitch => _minPitch;
    public float MaxPitch => _maxPitch;
}
