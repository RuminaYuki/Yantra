using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FatalFrameCameraController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CameraAnimationController _cameraAnimationController;
    // หมายเหตุ: เอา InputObserver ออกไปไว้ที่ State Machine หรือ Player Controller แทน เพื่อรวมศูนย์การจัดการ Input

    [Header("Targets")]
    [SerializeField] private Transform _tppPivot;
    [SerializeField] private Transform _fppEyePosition;
    [SerializeField] private Transform _otsPivot;
    [SerializeField] private Transform _yantraPivot;

    [Tooltip("เพิ่มการก้มหน้า +ก้มลง -ก้มขึ้น")]
    [SerializeField] private int _yantraOffsetRotationY;

    [Header("TPP Settings")]
    [SerializeField] private Vector3 _tppOffset = new Vector3(0.6f, 0.2f, -2.5f);
    [SerializeField] private float _tppFOV = 60f;

    [Header("FPP / OTS Settings")]
    [SerializeField] private float _fppFOV = 40f;
    [SerializeField] private float _otsFOV = 50f;

    [Header("Controls")]
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _transitionSpeed = 12f;
    [SerializeField] private float _minPitch = -40f;
    [SerializeField] private float _maxPitch = 60f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _cameraRadius = 0.2f;
    [SerializeField] private float _minDistance = 0.5f;

    // --- State Variables (Private เท่านั้น ห้ามคลาสอื่นแก้ตรงๆ) ---
    private Camera _mainCamera;
    private float _pitch = 0f;
    private float _yaw = 0f;
    private Vector2 _currentLookDelta; // รับค่ามาจากภายนอก

    private bool _isGunAiming = false;
    private bool _isYantraAiming = false;
    private bool _isFreeLookingInBook = false;

    private void Start()
    {
        _mainCamera = GetComponent<Camera>();

        // ตรงนี้ถ้าให้คลีนสุดๆ ควรย้ายไปอยู่ใน GameStateManager 
        // แต่ถ้าอยากให้กล้องจัดการเองตอนเริ่มเกม ก็คงไว้ได้ครับ
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

    // เอา Update() ที่เช็ค Mouse.current ออกไปเลย โค้ดจะโล่งขึ้นและไม่รันตัวเอง 

    private void LateUpdate()
    {
        // เช็คแค่ Transform พื้นฐาน ไม่ต้องเช็ค Mouse.current แล้ว
        if (_tppPivot == null || _fppEyePosition == null) return;

        Quaternion targetRotation;

        if (!_isYantraAiming)
        {
            // คำนวณ Rotation จากค่า Delta ที่ถูกส่งเข้ามาผ่าน API
            _yaw += _currentLookDelta.x * _mouseSensitivity;
            _pitch -= _currentLookDelta.y * _mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
        else
        {
            targetRotation = Quaternion.LookRotation(_yantraPivot.position - transform.position);
        }

        Vector3 desiredPosition;
        float targetFOV;

        if (_isYantraAiming)
        {
            desiredPosition = _fppEyePosition.position;
            targetFOV = _fppFOV;
        }
        else if (_isGunAiming)
        {
            desiredPosition = _otsPivot != null
                ? _otsPivot.position
                : _tppPivot.position + (targetRotation * new Vector3(0.5f, 0.1f, -1f));
            targetFOV = _otsFOV;
        }
        else
        {
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

        // Apply Transforms
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _transitionSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _transitionSpeed);
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, Time.deltaTime * _transitionSpeed);

        // Reset Look Delta ทุกเฟรมเพื่อรอรับค่าใหม่
        _currentLookDelta = Vector2.zero;
    }

    #region Public API (Interface สำหรับให้ State Machine/Player Controller เรียกใช้)

    /// <summary>
    /// ส่งค่า Mouse Delta เข้ามาหมุนกล้อง
    /// </summary>
    public void FeedLookInput(Vector2 lookDelta)
    {
        _currentLookDelta = lookDelta;
    }

    /// <summary>
    /// เปิด/ปิด โหมดเล็งปืน (OTS)
    /// </summary>
    public void SetGunAimState(bool isAiming)
    {
        // Logic กันเหนียว: ถ้ายันต์กางอยู่ ห้ามเล็งปืน
        if (_isYantraAiming && isAiming) return;

        _isGunAiming = isAiming;
    }

    /// <summary>
    /// เปิด/ปิด โหมดกางยันต์ (FPP)
    /// </summary>
    public void SetYantraAimState(bool isAiming)
    {
        _isYantraAiming = isAiming;

        // บังคับปิดปืนเมื่อกางยันต์
        if (_isYantraAiming) _isGunAiming = false;
    }

    /// <summary>
    /// สลับโหมด Free Look ตอนเปิดสมุด
    /// </summary>
    public void ToggleFreeLookInBook()
    {
        _isFreeLookingInBook = !_isFreeLookingInBook;
        Debug.Log($"FreeLook Toggled. IsYantraAiming: {_isYantraAiming}, _isFreeLookingInBook: {_isFreeLookingInBook}");
    }

    public void ForceDisableFreeLookInBook()
    {
        _isFreeLookingInBook = false;
    }

    public Vector2 CameraRotation => new Vector2(_yaw, _pitch);
    public float MinPitch => _minPitch;
    public float MaxPitch => _maxPitch;
    public bool IsGunAiming => _isGunAiming;
    public bool IsYantraAiming => _isYantraAiming;

    #endregion
}