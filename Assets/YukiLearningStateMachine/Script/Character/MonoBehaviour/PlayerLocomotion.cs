using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private YantraInputObserverSO _playerInput;
    private bool _enableUseInputObserverSO;

    private Rigidbody _rigidbody;
    private Vector3 _directionMove;

    [Header("ReferencePoint")]
    [SerializeField] private Transform _referencePoint;

    //movesystem
    private Movement _movement;

    //Rotate
    private Rotation _rotation;
    private bool _faceMoveDirection;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _enableUseInputObserverSO = true;
        _faceMoveDirection = false;

        _movement = new Movement(_rigidbody);
        _rotation = new Rotation(_rigidbody);
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
        _movement.Move(GetWorldDirectionRelativeTo(_directionMove,_referencePoint));

        _rotation.Rotate(_faceMoveDirection? 
            GetWorldDirectionRelativeTo(_directionMove,_referencePoint):
            GetCameraForwardFlat(), Time.fixedDeltaTime);
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

        return Vector3.ClampMagnitude(
            referenceForward * inputDirection.z +
            referenceRight * inputDirection.x,
            1f);
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
    // Locomotion Function
    public void StopMove()
    {
        _movement.Stop();
    }
    // Speed move and rotate
    public void SetSpeedMove(float speed) => _movement.Speed = speed;
    public float GetSpeedMove() => _movement.Speed;
    public void SetSpeedRotation(float speed) => _rotation.Speed = speed;
    public float GetSpeedRotation() => _rotation.Speed;

    // Enable function
    public void SetEnableUseInputObserverSO(bool enable) => _enableUseInputObserverSO = enable;
    public bool GetEnableUseInputObserverSO() => _enableUseInputObserverSO;
    public void SetEnableFaceMoveDirection(bool enable) => _faceMoveDirection = enable;
    public bool SetEnableFaceMoveDirection() => _faceMoveDirection;

    // Direction
    public void SetDirection(Vector3 direction) => _directionMove = direction;
    public Vector3 GetDirection() => _directionMove;

    #endregion
}
