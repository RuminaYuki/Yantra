using UnityEngine;
using Kogetsu.Library.Core;
using NaughtyAttributes;
using UnityEngine.InputSystem;

public class RinAnimationController : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CameraAnimationController _cameraAnimationController;
    [SerializeField] private YantraInputObserverSO _inputObserver;
    [SerializeField] private DrawOn3DMesh _drawOn3DMesh;
    [SerializeField] private YantCaster _yantCaster;
    [SerializeField] private TPPMoveController3D _moveController;

    // ==========================================
    // ระบบเล็งปืน และ กล้อง
    // ==========================================
    [Header("Aiming & Camera System")]
    [Tooltip("ลาก Main Camera (ที่มีสคริปต์กล้อง) มาใส่ช่องนี้")]
    [SerializeField] private FatalFrameCameraController _cameraController;
    
    [Tooltip("ลากโฟลเดอร์ TargetTracking (Rig) มาใส่ช่องนี้")]
    [SerializeField] private UnityEngine.Animations.Rigging.Rig _aimRig; 
    
    [Tooltip("ลาก Object เป้าหมาย (AimTarget_Point) ที่สร้างไว้มาใส่ช่องนี้")]
    [SerializeField] private Transform _aimTarget;

    [Tooltip("เลเยอร์ที่ปืนจะเล็งเป้าไปหา (เอาเครื่องหมายถูกที่ Player ออก!)")]
    [SerializeField] private LayerMask _aimColliderMask = ~0;

    // ==========================================
    // ระบบตะเกียง
    // ==========================================
    [Header("Lamp System")]
    [Tooltip("ลากออบเจกต์ตะเกียง (Lamp) จากใน Hierarchy มาใส่ช่องนี้")]
    [SerializeField] private GameObject _lampObject;

    [Header("Animation Settings")]
    [SerializeField] private float _dampTime = 0.1f;
    [SerializeField] private float _timeBeforeScared = 5f;
    private float _idleTimer = 0f;

    [Header("States")]
    [SerializeField] private bool _isDrawing;
    [SerializeField] private bool _isLampOn;
    private bool _isRunning;
    private float _lastYRotation;

    private Vector3 _currentMoveInput;

    private readonly int _moveXHash = Animator.StringToHash("MoveX");
    private readonly int _moveZHash = Animator.StringToHash("MoveZ");
    private readonly int _jumpHash = Animator.StringToHash("Jump");
    private readonly int _drawHash = Animator.StringToHash("Draw");
    private readonly int _lampHash = Animator.StringToHash("Lamp");
    private readonly int _groundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _isScaredHash = Animator.StringToHash("IsScared");
    private readonly int _isGunAimingHash = Animator.StringToHash("IsGunAiming");

    private void OnValidate() { if (!_animator) TryGetComponent(out _animator); }
    private void Awake() { if (!_yantCaster) _yantCaster = FindFirstObjectByType<YantCaster>(); }

    private void OnEnable()
    {
        if (!_inputObserver) return;
        _inputObserver.OnMoveChannel += HandleMoveInput;
        _inputObserver.OnJumpChannel += HandleJumpInput;
        _inputObserver.OnRunChannel += HandleRunInput;
        _inputObserver.OnPressQ_ButtonChannel += HandlePressQInput;
        _inputObserver.OnPressF_ButtonChannel += HandlePressFInput;
        _inputObserver.OnPressE_ButtonChannel += HandlePressEInput;
        _inputObserver.OnLeftClickChannel += HandlePressLeftClickInput;
    }

    private void OnDisable()
    {
        if (!_inputObserver) return;
        _inputObserver.OnMoveChannel -= HandleMoveInput;
        _inputObserver.OnJumpChannel -= HandleJumpInput;
        _inputObserver.OnRunChannel -= HandleRunInput;
        _inputObserver.OnPressQ_ButtonChannel -= HandlePressQInput;
        _inputObserver.OnPressF_ButtonChannel -= HandlePressFInput;
        _inputObserver.OnPressE_ButtonChannel -= HandlePressEInput;
        _inputObserver.OnLeftClickChannel -= HandlePressLeftClickInput;
    }

    private void Update()
    {
        UpdateMovementAnimation();
        UpdateJumpAnimation();
        UpdateIdleTimer();
        UpdateGunAimingSystem(); // รันระบบเล็งปืน
    }

    private void UpdateIdleTimer()
{
    // เพิ่ม && !_isDrawing เข้าไป : แปลว่า ถ้ายืนนิ่งๆ "และไม่ได้กางสมุดอยู่" ค่อยนับเวลาสั่น
    if (_currentMoveInput.sqrMagnitude < 0.01f && !_isDrawing)
    {
        _idleTimer += Time.deltaTime;
        if (_idleTimer >= _timeBeforeScared) _animator.SetBool(_isScaredHash, true);
    }
    else
    {
        // ถ้ากำลังเดิน หรือ กำลังถือสมุดอยู่ ให้รีเซ็ตเวลา และปิดท่าสั่น
        _idleTimer = 0f;
        _animator.SetBool(_isScaredHash, false);
    }
}

    // ==========================================
    // ระบบจัดการตอนคลิกขวาเล็งปืน
    // ==========================================
    private void UpdateGunAimingSystem()
    {
        if (_cameraController == null) return;

        bool isGunAiming = _cameraController.IsGunAiming;
        _animator.SetBool(_isGunAimingHash, isGunAiming);

        // เปิด/ปิด การเล็งแบบสมูท (Lerp Weight)
        if (_aimRig != null)
        {
            float targetWeight = isGunAiming ? 1f : 0f;
            _aimRig.weight = Mathf.Lerp(_aimRig.weight, targetWeight, Time.deltaTime * 10f);
        }

        // ระบบเรดาร์ทะลุตัวละคร
        if (isGunAiming && _aimTarget != null && Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
            // ใช้ _aimColliderMask บังคับให้ทะลุ Player ไปโดนฉากหรือศัตรูแทน
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _aimColliderMask))
            {
                _aimTarget.position = hit.point;
            }
            else
            {
                _aimTarget.position = ray.GetPoint(100f);
            }
        }
    }

    private void HandleMoveInput(Vector3 moveInput) { _currentMoveInput = moveInput; }
    private void HandleRunInput(bool isRunning) { _isRunning = isRunning; }

    private void UpdateMovementAnimation()
    {
        Transform camTransform = Camera.main.transform;
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 worldMoveDir = (camForward * _currentMoveInput.z + camRight * _currentMoveInput.x).normalized;
        Vector3 localDir = transform.InverseTransformDirection(worldMoveDir);

        // === ✨ ระบบจำลองการก้าวเท้าตอนหันเป้าเล็ง ✨ ===
        // 1. เช็คว่าเฟรมนี้ตัวละครหมุนไปกี่องศา
        float currentY = transform.eulerAngles.y;
        float deltaY = Mathf.DeltaAngle(_lastYRotation, currentY);
        _lastYRotation = currentY;

        float turnShuffleX = 0f;

        // 2. ถ้ากำลังเล็งปืนอยู่ และ ไม่ได้กดปุ่มเดิน (ยืนอยู่กับที่)
        if (_cameraController != null && _cameraController.IsGunAiming && _currentMoveInput.sqrMagnitude < 0.01f)
        {
            // เอาองศาที่หมุนมาคำนวณเป็นความเร็ว (องศาต่อวินาที)
            float turnSpeed = deltaY / Time.deltaTime;

            // แปลงความเร็วหมุน ให้กลายเป็นค่าน้ำหนักเดินซ้าย-ขวา (-1 ถึง 1)
            // (เลข 120f คือตัวหารความสมูท: ถ้าอยากให้เท้าสับไวขึ้น ให้ลดเลขลง เช่น 90f)
            turnShuffleX = Mathf.Clamp(turnSpeed / 120f, -1f, 1f);
        }

        // 3. เอาค่าขยับเท้าตอนหมุน ไปบวกรวมกับค่าเดินปกติ
        float finalMoveX = Mathf.Clamp(localDir.x + turnShuffleX, -1f, 1f);
        // ===============================================

        float speedMultiplier = _isRunning ? 1f : 0.5f;

        // โยนค่า finalMoveX ที่ผสมเสร็จแล้ว ส่งให้ Animator ดึงท่าขยับเท้ามาเล่น
        _animator.SetFloat(_moveXHash, finalMoveX * speedMultiplier, _dampTime, Time.deltaTime);
        _animator.SetFloat(_moveZHash, localDir.z * speedMultiplier, _dampTime, Time.deltaTime);
    }

    private void HandleJumpInput() { _animator.SetTrigger(_jumpHash); }
    private void UpdateJumpAnimation() { if (_moveController) _animator.SetBool(_groundedHash, _moveController.IsGrounded); }

    public void HandlePressQInput()
    {
        _isLampOn = false;
        if (_lampObject != null) _lampObject.SetActive(false);

        bool wasDrawing = _isDrawing;
        _isDrawing = !wasDrawing;

        _animator.SetBool(_drawHash, _isDrawing);
        _animator.SetBool(_lampHash, _isLampOn);

        if (_cameraController != null) _cameraController.IsYantraAiming = _isDrawing;
        if (!wasDrawing) return;

        // ถ้ากำลังวาดอยู่แล้วกด Q อีกครั้งเพื่อหยุดวาด เราจะเคลียร์การวาดบน Mesh
        if (_drawOn3DMesh != null) _drawOn3DMesh.ClearDrawing();

        //ถ้ากำลังวาดอยู่แล้วกด Q อีกครั้งเพื่อหยุดวาด เราจะเคลียร์การวิเคราะห์ของ YantCaster ด้วย
        //bool castSucceeded = _yantCaster != null && _yantCaster.TryAnalyze();
        //if (!castSucceeded && _drawOn3DMesh) _drawOn3DMesh.ClearDrawing();
    }

    private void HandlePressFInput()
    {
        _isDrawing = false;
        if (_cameraController != null) _cameraController.IsYantraAiming = false;

        _isLampOn = !_isLampOn;
        _animator.SetBool(_lampHash, _isLampOn);
        _animator.SetBool(_drawHash, _isDrawing);

        if (_lampObject != null) _lampObject.SetActive(_isLampOn);
    }

    private void HandlePressEInput()
    {
        if (_yantCaster != null)
        {
            _yantCaster.Analyze();
        }
    }


    private void HandlePressLeftClickInput(Vector2 position, InputAction.CallbackContext context)
    {
        if(_yantCaster != null && context.started)
        {
            _yantCaster.tryCastYant();
        }
    }

}