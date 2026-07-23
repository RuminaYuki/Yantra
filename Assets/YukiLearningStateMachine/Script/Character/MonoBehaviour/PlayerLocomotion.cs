using UnityEngine;
[RequireComponent(typeof(CharacterController),typeof(Animator))]
public class PlayerLocomotion : MonoBehaviour
{
    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _directionMove;

    
    [Header("Player Input")]
    [SerializeField] private YantraInputObserverSO _playerInput;
    private bool _enableUseInputObserverSO;

    //===================== Camera ============================
    [Header("ReferencePoint")]
    [SerializeField] private Transform _referencePoint;

    //================Locomotion Animatiton====================
    [Header("Locomotion Animatiton Setting")]
    [SerializeField] private float _dampTime = 0.1f;
    //====================Walk and Run=========================
    [Header("Walk and Run Setting")]
    private float _multiply;
    [SerializeField] private string _nameParameterMoveZ;
    [SerializeField] private string _nameParameterMoveX;

    private int _moveZ; //Set parameter
    private int _moveX; //Set parameter

    [Header("Walk and Run Setting")]
    [SerializeField] private string _nameTurnAngle;
    [SerializeField] private string _nameStartTurn;
    [SerializeField] private float _angleTurn = 45;
    private int _startTurnAngle;
    private int _startTurnTrigger;
    private bool _wasMoving;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        //Hash Sring
        _moveZ = Animator.StringToHash(_nameParameterMoveZ);
        _moveX = Animator.StringToHash(_nameParameterMoveX);

        _startTurnAngle = Animator.StringToHash(_nameTurnAngle);
        _startTurnTrigger = Animator.StringToHash(_nameStartTurn); 

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
        // set Animation
        Vector3 direction = GetDirectionWithReferencePoint();
        MoveAnimation(direction);
        TurnAnimation(direction);
    }

    void FixedUpdate()
    {
        bool hasMoveInput = _directionMove.sqrMagnitude > 0.01f;
        if (!hasMoveInput) return;
        Rotate(GetCameraForwardFlat());
    }
    private void OnAnimatorMove()
    {
        _characterController.Move(_animator.deltaPosition);
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
        float velocityZ = Mathf.Clamp(localVelocity.z, -1, 1);
        float velocityX = Mathf.Clamp(localVelocity.x, -1, 1);

        _animator.SetFloat(_moveZ, velocityZ * _multiply, _dampTime, Time.deltaTime);
        _animator.SetFloat(_moveX, velocityX * _multiply, _dampTime, Time.deltaTime);
    }
    private void TurnAnimation(Vector3 direction)
    {
        bool isMoving = direction.sqrMagnitude > 0.01f;
        bool justStartedMoving = isMoving && !_wasMoving;

        if (justStartedMoving)
        {
            float angle = Vector3.SignedAngle(transform.forward,direction,Vector3.up);

            if (Mathf.Abs(angle) >= _angleTurn)
            {
                _animator.SetFloat(_startTurnAngle, angle);
                _animator.SetTrigger(_startTurnTrigger);
            }
        }

        _wasMoving = isMoving;
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

    public void Rotate(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        Quaternion nextRotation = Quaternion.Slerp(transform.rotation,
            targetRotation, Time.fixedDeltaTime);

        transform.rotation = nextRotation;
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

    //Animaiton
    // Get,Set Mutiply
    public float GetMutiply() => _multiply;
    public void  SetMuitply(float multiply) => _multiply = multiply;

    #endregion
}
