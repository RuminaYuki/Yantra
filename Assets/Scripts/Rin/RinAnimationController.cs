using UnityEngine;

public class RinAnimationController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CameraAnimationController _cameraAnimationController;
    [SerializeField] private YantraInputObserverSO _inputObserver;
    [SerializeField] private DrawOn3DMesh _drawOn3DMesh;
    [SerializeField] private YantCaster _yantCaster;

    [SerializeField] private bool _isDrawing;
    [SerializeField] private bool _isLampOn;

    private void OnValidate()
    {
        if (!_animator) TryGetComponent(out _animator);
    }

    private void Awake()
    {
        if (!_yantCaster) _yantCaster = FindFirstObjectByType<YantCaster>();
    }

    private void OnEnable()
    {
        if (!_inputObserver) return;

        _inputObserver.OnMoveChannel += HandleMoveInput;
        _inputObserver.OnJumpChannel += HandleJumpInput;
        _inputObserver.OnRunChannel += HandleRunInput;
        _inputObserver.OnPressQ_ButtonChannel += HandlePressQInput;
        _inputObserver.OnPressF_ButtonChannel += HandlePressFInput;
    }

    private void OnDisable()
    {
        if (!_inputObserver) return;

        _inputObserver.OnMoveChannel -= HandleMoveInput;
        _inputObserver.OnJumpChannel -= HandleJumpInput;
        _inputObserver.OnRunChannel -= HandleRunInput;
        _inputObserver.OnPressQ_ButtonChannel -= HandlePressQInput;
        _inputObserver.OnPressF_ButtonChannel -= HandlePressFInput;
    }

    private void HandleMoveInput(Vector3 moveInput)
    {
        _animator.SetBool("IsMoving", moveInput != Vector3.zero);
    }

    private void HandleJumpInput()
    {
        _animator.SetTrigger("Jump");
    }

    private void HandleRunInput(bool isRunning)
    {
        _animator.SetBool("IsRunning", isRunning);
    }

    private void HandlePressQInput()
    {
        _isLampOn = false;
        bool wasDrawing = _isDrawing;
        _isDrawing = !wasDrawing;

        _animator.SetBool("Draw", _isDrawing);
        _animator.SetBool("Lamp", _isLampOn);
        _cameraAnimationController.OnDrawEvent(_isDrawing);

        if (!wasDrawing) return;

        bool castSucceeded = _yantCaster != null && _yantCaster.TryAnalyzeAndCast();
        if (!castSucceeded && _drawOn3DMesh)
        {
            _drawOn3DMesh.ClearDrawing();
        }
    }

    private void HandlePressFInput()
    {
        _isDrawing = false;
        _isLampOn = !_isLampOn;

        _animator.SetBool("Lamp", _isLampOn);
        _animator.SetBool("Draw", _isDrawing);
        _cameraAnimationController.OnDrawEvent(_isDrawing);
    }
}
