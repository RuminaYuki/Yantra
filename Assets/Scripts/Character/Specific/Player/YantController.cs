using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;
using UnityEngine.AdaptivePerformance;

public class YantController : MonoBehaviour
{
    [SerializeField] private YantCaster _yantCaster;
    [SerializeField] private DrawOn3DMesh _drawOn3DMesh;
    [SerializeField] private GameObject _bookRef;
    [SerializeField] private YantraInputObserverSO _playerInput;

    [ReadOnly][SerializeField] private float _holdTime = 0;
    private bool _holding = false;
    private bool _isHoldingLeftMouse = false;

    [ReadOnly][SerializeField]private bool _isDrawing = false;
    private bool IsDrawing
    {
        get { return _isDrawing; }
        set 
        { 
            _isDrawing = value; 
            _bookRef.SetActive(value);
            if (value) _drawOn3DMesh.ClearDrawing();
        }
    }

    [ReadOnly][SerializeField] private bool _isCasting = false;

    private void Awake()
    {
        _bookRef.SetActive(false);
    }

    private void OnEnable()
    {
        _playerInput.OnPressE_ButtonChannel += HandlePressEInput;
        _playerInput.OnLeftClickChannel += HandlePressLeftClickInput;
        _playerInput.OnRightClickChannel += HandleHoldRightClickInput;
    }

    private void OnDisable()
    {
        _playerInput.OnPressE_ButtonChannel -= HandlePressEInput;
        _playerInput.OnLeftClickChannel -= HandlePressLeftClickInput;
        _playerInput.OnRightClickChannel -= HandleHoldRightClickInput;
        _bookRef.SetActive(false);
    }

    private void Update()
    {
        if (_holding)
        {
            _holdTime += Time.deltaTime;
        }

        if (_isHoldingLeftMouse && _isCasting)
        {
            _yantCaster?.tryCastYant(_holdTime, true);
        }
    }

    private void HandlePressEInput()
    {
        if (_yantCaster != null && _isDrawing)
        {
            _yantCaster.Analyze();
        }
    }

    private void HandlePressLeftClickInput(Vector2 position, InputAction.CallbackContext context)
    {
        if (!_isCasting) return;

        if (context.started)
        {
            _isHoldingLeftMouse = true;
            _holdTime = 0f;
            _yantCaster?.tryCastYant(_holdTime, true);
        }

        if (context.canceled)
        {
            _isHoldingLeftMouse = false;
            _holdTime = 0f;
            _yantCaster?.tryCastYant(0f, false);
        }
    }

    private void HandleHoldRightClickInput(Vector2 position, InputAction.CallbackContext context)
    {
        if (!_isCasting) return;

        if (context.started)
        {
            _holding = true;
            _holdTime = 0;
        }
        if (context.canceled)
        {
            _holding = false;
        }
    }

    #region API
    public void SetDrawInputObserverSO(bool enable) => IsDrawing = enable;
    public void SetCastInputObserverSO(bool enabled) => _isCasting = enabled;


    #endregion
}
