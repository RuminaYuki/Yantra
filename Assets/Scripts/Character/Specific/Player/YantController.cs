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
        if (_holding && _isCasting)
        {
            _holdTime += Time.deltaTime;
            Debug.Log($"<color=#00FFFF>[YantController]</color> Holding right click for {_holdTime} seconds.");
        }
        else
        {
            _holdTime = 0;
            _holding = false;
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
        if (_yantCaster != null && context.started && _isCasting)
        {
            _yantCaster.tryCastYant(_holdTime);
            _holdTime = 0;
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
