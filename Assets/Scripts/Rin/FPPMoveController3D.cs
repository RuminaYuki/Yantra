using UnityEngine;
using Kogetsu.Library.Core;
using NaughtyAttributes;

[RequireComponent(typeof(Rigidbody))]
public class FPPMoveController3D : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private YantraInputObserverSO _inputObserverChannel;

    [SerializeField] private YantraStatsController _statsController;

    [SerializeField] private Transform _cameraTransform;

    [SerializeField] private bool _enableJump = true;

    [SerializeField] private Transform _groundCheck;

    [SerializeField] private float _groundRadius = 0.15f;

    [SerializeField] private LayerMask _groundLayer;

    [SerializeField] private float _jumpCooldown = 0.5f;

    [ReadOnly]
    [SerializeField] private float _currentJumpCooldown;

    [SerializeField] private bool _startJumpCooldownOnLanding = true;

    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _pendingJumpCooldown;

    private bool _isFreeLooking;

    private Vector3 _currentMoveInput;

    private void Awake()
    {
        if (_rb == null)
            TryGetComponent(out _rb);

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
        UpdateJumpCooldown();
    }

    private void FixedUpdate()
    {
        UpdateGroundCheck();
        Move();
    }

    private void LateUpdate()
    {
        RotateBody();
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
        _isFreeLooking = isPressed;
    }

    private void HandleJumpInput()
    {
        if (_statsController.IsHurt || _currentJumpCooldown > 0f)
            return;

        if (_enableJump && _isGrounded)
        {
            _statsController.UseStamina(_statsController.GetJumpStaminaCost());

            if (_startJumpCooldownOnLanding)
            {
                _pendingJumpCooldown = true;
            }
            else
            {
                _currentJumpCooldown = _jumpCooldown;
            }

            _rb.linearVelocity = new Vector3(
                _rb.linearVelocity.x,
                0f,
                _rb.linearVelocity.z);

            _rb.AddForce(
                Vector3.up * _statsController.GetJumpForce(),
                ForceMode.Impulse);

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

        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection =
            (camForward * _currentMoveInput.z +
             camRight * _currentMoveInput.x).normalized;

        Vector3 targetVelocity =
            moveDirection * _statsController.GetMoveSpeed();

        _rb.linearVelocity = new Vector3(
            targetVelocity.x,
            _rb.linearVelocity.y,
            targetVelocity.z);
    }

    private void RotateBody()
    {
        if (_cameraTransform == null || _isFreeLooking)
            return;

        Vector3 euler = transform.eulerAngles;
        euler.y = _cameraTransform.eulerAngles.y;

        transform.eulerAngles = euler;
    }

    private void UpdateGroundCheck()
    {
        if (!_enableJump || _groundCheck == null)
            return;

        _wasGrounded = _isGrounded;

        _isGrounded = Physics.CheckSphere(
            _groundCheck.position,
            _groundRadius,
            _groundLayer);

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
        Gizmos.DrawWireSphere(
            _groundCheck.position,
            _groundRadius);
    }
}
