using UnityEngine;
using Kogetsu.Library.Core;
using NaughtyAttributes;

/// <summary>
/// ควบคุมการเคลื่อนที่และหมุนตัวละคร รองรับระบบกล้องสไตล์ Fatal Frame
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TPPMoveController3D : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private YantraInputObserverSO _inputObserverChannel;
    [SerializeField] private YantraStatsController _statsController;
    [SerializeField] private Transform _cameraTransform;

    [Header("Camera & Rotation System")]
    [Tooltip("ใส่ Main Camera ที่มีสคริปต์ FatalFrameCameraController")]
    [SerializeField] private FatalFrameCameraController _cameraSystem;
    [Tooltip("ความเร็วในการหันหน้าของตัวละครในโหมด TPP")]
    [SerializeField] private float _turnSmoothSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField] private bool _enableJump = true;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _jumpCooldown = 0.5f;
    [SerializeField] private bool _startJumpCooldownOnLanding = true;

    [ReadOnly]
    [SerializeField] private float _currentJumpCooldown;

    [Header("Rotation Behavior")]
    [Tooltip("ถ้าเปิด: ตัวละครจะหันหน้าตามทิศทางที่เดิน (สไตล์ Fatal Frame). ถ้าปิด: หันตามกล้องเสมอเหมือนเดิม")]
    [SerializeField] private bool _faceMoveDirection = true;

    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _pendingJumpCooldown;
    private bool _isFreeLooking;
    private Vector3 _currentMoveInput;
    private Vector3 _lastMoveDirection; // ทิศทางการเดินล่าสุด (คำนวณจาก camera-relative input) ใช้หันตัวละครแบบ Fatal Frame
    public bool IsGrounded => _isGrounded;

    private void Awake()
    {
        if (_rb == null)
            TryGetComponent(out _rb);

        // ตั้งค่า Interpolate เพื่อให้การเคลื่อนที่และหมุนตัวด้วย Physics สมูทขึ้น
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable()
    {
        if (_inputObserverChannel != null)
        {
            _inputObserverChannel.OnMoveChannel += HandleMoveInput;
            _inputObserverChannel.OnJumpChannel += HandleJumpInput;
            _inputObserverChannel.OnRunChannel += HandleSprintInput;
            _inputObserverChannel.OnMiddleClickChannel += HandleFreeLookInput;
        }
    }

    private void OnDisable()
    {
        if (_inputObserverChannel != null)
        {
            _inputObserverChannel.OnMoveChannel -= HandleMoveInput;
            _inputObserverChannel.OnJumpChannel -= HandleJumpInput;
            _inputObserverChannel.OnRunChannel -= HandleSprintInput;
            _inputObserverChannel.OnMiddleClickChannel -= HandleFreeLookInput;
        }
    }

    private void Update()
    {
        UpdateJumpCooldown(); // นับเวลา Cooldown การกระโดด
    }

    private void FixedUpdate()
    {
        UpdateGroundCheck(); // เช็คการติดพื้น
        Move();              // เคลื่อนที่ด้วยความเร็ว[cite: 3]
        RotateBody();        // หมุนตัวละคร (ทำงานใน FixedUpdate เพื่อให้ซิงค์กับ Rigidbody)
    }

    private void HandleMoveInput(Vector3 moveInput)
    {
        _currentMoveInput = moveInput;
        _statsController.IsMoving = moveInput.sqrMagnitude > 0.01f;
    }

    private void HandleSprintInput(bool isRunning)
    {
        _statsController.Run(isRunning);
    }

    private void HandleFreeLookInput(Vector2 mousePos, bool isPressed)
    {
        _isFreeLooking = isPressed; // ล็อกการหันตัวละครเมื่อกด Free Look[cite: 3]
    }

    private void HandleJumpInput()
    {
        if (_statsController.IsHurt || _currentJumpCooldown > 0f)
            return;

        if (_enableJump && _isGrounded)
        {
            _statsController.UseStamina(_statsController.GetJumpStaminaCost()); // หัก Stamina[cite: 3]

            if (_startJumpCooldownOnLanding)
            {
                _pendingJumpCooldown = true;
            }
            else
            {
                _currentJumpCooldown = _jumpCooldown;
            }

            // รีเซ็ตความเร็วแกน Y แล้วใช้ AddForce แบบ Impulse เพื่อกระโดด[cite: 3]
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _statsController.GetJumpForce(), ForceMode.Impulse);

            _statsController.IsJumping = true;
        }
    }

    private void UpdateJumpCooldown()
    {
        if (_currentJumpCooldown > 0f)
            _currentJumpCooldown -= Time.deltaTime;

        if (_currentJumpCooldown <= 0f)
            _currentJumpCooldown = 0f;
    }

    private void Move()
    {
        if (_cameraTransform == null)
            return;

        // คำนวณทิศทางการเคลื่อนที่โดยอิงจากมุมกล้องปัจจุบัน[cite: 3]
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        // ตัดแกน Y ออกเพื่อไม่ให้ตัวละครบินขึ้นฟ้าหรือมุดดินตามมุมกล้อง[cite: 3]
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * _currentMoveInput.z + camRight * _currentMoveInput.x).normalized;
        Vector3 targetVelocity = moveDirection * _statsController.GetMoveSpeed();

        // ใส่ความเร็วในแนวระนาบ โดยรักษาความเร็วแกน Y เดิมไว้ (สำหรับการตกหรือกระโดด)[cite: 3]
        _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);

        // เก็บทิศทางการเดินไว้ใช้หมุนตัวใน RotateBody() (เฉพาะตอนมี input จริงๆ เท่านั้น กันไม่ให้ moveDirection กลายเป็น Vector3.zero ตอนยืนนิ่ง)
        if (moveDirection.sqrMagnitude > 0.0001f)
            _lastMoveDirection = moveDirection;
    }

    private void RotateBody()
    {
        if (_cameraTransform == null || _cameraSystem == null || _isFreeLooking)
            return;

        // แก้ไขบรรทัดนี้: ถ้ากำลังเล็งปืน หรือ กางยันต์ ให้หันหน้าตามกล้องทันที
        if (_cameraSystem.IsGunAiming || _cameraSystem.IsYantraAiming)
        {
            Vector3 aimEuler = transform.eulerAngles;
            aimEuler.y = _cameraTransform.eulerAngles.y;
            transform.eulerAngles = aimEuler;
            return;
        }

        bool hasMoveInput = _currentMoveInput.sqrMagnitude > 0.01f;

        if (!hasMoveInput)
            return; // ไม่มี input การเดิน -> ค้างทิศทางเดิมไว้ ไม่หมุนตาม

        Vector3 faceDirection = _faceMoveDirection ? _lastMoveDirection : GetCameraForwardFlat();

        if (faceDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(faceDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSmoothSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// คืนค่าทิศทางหน้ากล้อง โดยตัดแกน Y ออก (ใช้เป็น fallback เมื่อปิด _faceMoveDirection)
    /// </summary>
    private Vector3 GetCameraForwardFlat()
    {
        if (_cameraTransform == null)
            return transform.forward;

        Vector3 flatForward = _cameraTransform.forward;
        flatForward.y = 0f;
        return flatForward.normalized;
    }

    private void UpdateGroundCheck()
    {
        if (!_enableJump || _groundCheck == null)
            return;

        _wasGrounded = _isGrounded;

        // เช็คการชนพื้นด้วย Physics.CheckSphere[cite: 3]
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundRadius, _groundLayer);
        _statsController.IsGrounded = _isGrounded;

        if (!_wasGrounded && _isGrounded)
        {
            _statsController.IsJumping = false;

            if (_pendingJumpCooldown)
            {
                _currentJumpCooldown = _jumpCooldown;
                _pendingJumpCooldown = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundRadius); // วาด Sphere แสดงรัศมีเช็คพื้น[cite: 3]
    }
}