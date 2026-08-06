using UnityEngine;
using NaughtyAttributes;
[RequireComponent(typeof(CharacterController),typeof(Animator))]
public class PlayerLocomotion : BaseLocomotion
{
    private Vector3 _directionMove;

    
    [Header("Player Input")]
    [SerializeField] private YantraInputObserverSO _playerInput;
    private bool _enableUseInputObserverSO;

    //===================== Camera ============================
    [Header("ReferencePoint")]
    [SerializeField] private Transform _referencePoint;

    //Walk Run and Turn Setting
    [Header("Walk and Run Setting")]
    [SerializeField] private string _nameParameterMoveZ;
    [SerializeField] private string _nameParameterMoveX;
    
    //Turn
    [Header("Turn Animation Setting")]
    [SerializeField] private string _nameTurnAngle;
    [SerializeField] private string _nameStartTurn;
    [SerializeField] private float _angleTurn = 45;
    [SerializeField] private float _angleTurnbyCamera = 60;

    [Header("Turn Logic Setting")]
    [ReadOnly] [SerializeField] private bool _turnbyCamera;

    

    
    private bool _wasMoving;

    protected override void Awake()
    {
        base.Awake();

        //Set LocomotionAnim
        LocomotionAnim.SetMoveParameter(_nameParameterMoveX,_nameParameterMoveZ);
        LocomotionAnim.SetTurnParameter(_nameTurnAngle,_nameStartTurn);

        _enableUseInputObserverSO = true;
    }

    void OnEnable()
    {
        _playerInput.OnMoveChannel += HandleMoveInput;
    }

    void OnDisable()
    {
        _playerInput.OnMoveChannel -= HandleMoveInput;
    }
    void Update()
    {
        if (IsMovementLocked)
        {
            _wasMoving = false;

            if (ShouldResetMoveAnimation)
                LocomotionAnim.SetMove(0f, 0f);

            return;
        }

        // set Animation
        Vector3 direction = GetDirectionWithReferencePoint();
        if (_turnbyCamera)
            TurnByCamera(direction);
        else
            TurnAnimation(direction);

        MoveAnimation(direction);
    }

    void FixedUpdate()
    {
        if (IsMovementLocked)
            return;

        bool hasMoveInput = _directionMove.sqrMagnitude > 0.01f;
        if (!hasMoveInput && !_turnbyCamera) return;
        Rotation.Rotate(GetCameraForwardFlat());
    }

    private void HandleMoveInput(Vector3 moveInput)
    {
        if(!_enableUseInputObserverSO) return;
        _directionMove = moveInput;
    }

    #region SetAnimation
    private void MoveAnimation(Vector3 direction)
    {
        Vector3 localVelocity = transform.InverseTransformDirection(direction);
        float velocityX = Mathf.Clamp(localVelocity.x, -1, 1);
        float velocityZ = Mathf.Clamp(localVelocity.z, -1, 1);

        LocomotionAnim.SetMove(velocityX,velocityZ);
    }
    private void TurnAnimation(Vector3 direction)
    {
        bool isMoving = direction.sqrMagnitude > 0.01f;
        bool justStartedMoving = isMoving && !_wasMoving;

        if (justStartedMoving && Mathf.Abs(GetSignedAngleToCamera()) >= _angleTurn)
        {
            float angle = Vector3.SignedAngle(transform.forward,direction,Vector3.up);
            LocomotionAnim.SetTurn(angle);
        }

        _wasMoving = isMoving;
    }
    private void TurnByCamera(Vector3 direction)
    {
        float angle = GetSignedAngleToCamera();

        if (Mathf.Abs(angle) >= _angleTurnbyCamera && !(direction.sqrMagnitude > 0.01f))
        {
            LocomotionAnim.SetTurn(angle);
        }
    }

    private float GetSignedAngleToCamera()
    {
        Vector3 playerForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 cameraForward = Vector3.ProjectOnPlane(_referencePoint.forward, Vector3.up).normalized;

        return Vector3.SignedAngle(playerForward, cameraForward, Vector3.up );
    }

    #endregion

    #region HelperMethod
    private Vector3 GetWorldDirectionRelativeTo(
    Vector3 inputDirection,
    Transform referenceTransform)
    {
        if (_referencePoint== null)
            return _directionMove;

        Vector3 referenceForward = referenceTransform.forward;
        Vector3 referenceRight = referenceTransform.right;

        referenceForward.y = 0f;
        referenceRight.y = 0f;

        referenceForward.Normalize();
        referenceRight.Normalize();

        return referenceForward * inputDirection.z + referenceRight * inputDirection.x;
    }

    private Vector3 GetCameraForwardFlat()
    {
        if (_referencePoint == null)
            return transform.forward;

        Vector3 flatForward =
            _referencePoint.forward;

        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.0001f)
            return transform.forward;

        return flatForward.normalized;
    }
    #endregion

    #region //API

    // Enable function
    public void SetEnableUseInputObserverSO(bool enable) => _enableUseInputObserverSO = enable;
    public bool GetEnableUseInputObserverSO() => _enableUseInputObserverSO;

    // Direction
    public void SetDirection(Vector3 direction) => _directionMove = direction;
    public Vector3 GetDirection() => _directionMove;
    public Vector3 GetDirectionWithReferencePoint() => GetWorldDirectionRelativeTo(_directionMove,_referencePoint);

    // Get,Set TurnbyCamera
    public bool GetTurnByCamera() => _turnbyCamera;
    public void SetTurnByCamera(bool value) => _turnbyCamera = value;

    #endregion
}
