using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private YantraInputObserverSO _playerInput;
    private bool _enableUseInputObserverSO;

    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _directionMove;

    [Header("ReferencePoint")]
    [SerializeField] private Transform _referencePoint;

    
    private bool _faceMoveDirection;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _enableUseInputObserverSO = true;
        _faceMoveDirection = false;
    }

    void OnEnable()
    {
        _playerInput.OnMoveChannel += HandleMoveInput;
    }

    void OnDisable()
    {
        _playerInput.OnMoveChannel -= HandleMoveInput;
    }

    void FixedUpdate()
    {

        bool hasMoveInput = _directionMove.sqrMagnitude > 0.01f;
        if(!hasMoveInput) return;
        
         //_rotation.Rotate(_faceMoveDirection? 
         //    GetWorldDirectionRelativeTo(_directionMove,_referencePoint):
         //    GetCameraForwardFlat(), Time.fixedDeltaTime);
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
    
    #region //API

    // Enable function
    public void SetEnableUseInputObserverSO(bool enable) => _enableUseInputObserverSO = enable;
    public bool GetEnableUseInputObserverSO() => _enableUseInputObserverSO;
    public void SetEnableFaceMoveDirection(bool enable) => _faceMoveDirection = enable;
    public bool SetEnableFaceMoveDirection() => _faceMoveDirection;

    // Direction
    public void SetDirection(Vector3 direction) => _directionMove = direction;
    public Vector3 GetDirection() => _directionMove;
    public Vector3 GetDirectionWithReferencePoint() => GetWorldDirectionRelativeTo(_directionMove,_referencePoint);

    #endregion
}
