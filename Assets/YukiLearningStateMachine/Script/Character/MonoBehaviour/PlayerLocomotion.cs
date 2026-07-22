using UnityEngine;
[RequireComponent(typeof(CharacterController),typeof(Animator))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private YantraInputObserverSO _playerInput;
    private bool _enableUseInputObserverSO;

    [Header("ReferencePoint")]
    [SerializeField] private Transform _referencePoint;


    [Header("Locomotion Animatiton Setting")]
    [SerializeField] private float _dampTime = 0.1f;
    private float _multiply;
    [SerializeField] private string _nameParameterMoveZ;
    [SerializeField] private string _nameParameterMoveX;
    [SerializeField] private string _nameParameterTurn;

    private int _moveZ;
    private int _moveParameterX;
    private int _turn;

    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _directionMove;


    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        //Hash Sring
        _moveZ = Animator.StringToHash(_nameParameterMoveZ);
        _moveParameterX = Animator.StringToHash(_nameParameterMoveX);
        _turn = Animator.StringToHash(_nameParameterTurn);

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
        Vector3 localVelocity = transform.InverseTransformDirection(direction);
        float velocityZ = Mathf.Clamp(localVelocity.z, -1, 1);
        float velocityX = Mathf.Clamp(localVelocity.x, -1, 1);
        float turn = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
            turn = Mathf.Clamp(angle / 90f, -1f, 1f);
        }



        _animator.SetFloat(_moveZ, velocityZ * _multiply, _dampTime, Time.deltaTime);
        _animator.SetFloat(_moveParameterX, velocityX * _multiply, _dampTime, Time.deltaTime);
        _animator.SetFloat(_turn, turn, _dampTime, Time.deltaTime);
    }

    void FixedUpdate()
    {
        bool hasMoveInput = _directionMove.sqrMagnitude > 0.01f;
        if (!hasMoveInput) return;
        //Rotate(GetCameraForwardFlat());
    }
    private void OnAnimatorMove()
    {
        _characterController.Move(_animator.deltaPosition);
        transform.rotation *= _animator.deltaRotation;
    }

    private void HandleMoveInput(Vector3 moveInput)
    {
        if(!_enableUseInputObserverSO) return;
        _directionMove = moveInput;
    }

    

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

    //Animaiton
    // Get,Set Mutiply
    public float GetMutiply() => _multiply;
    public void  SetMuitply(float multiply) => _multiply = multiply;

    #endregion
}
