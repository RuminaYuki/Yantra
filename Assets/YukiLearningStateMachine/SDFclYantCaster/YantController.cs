using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;
using UnityEngine.AdaptivePerformance;

public class YantController : MonoBehaviour
{
    [SerializeField] private YantCaster _yantCaster;

    [SerializeField] private GameObject _bookRef;
    [SerializeField] private YantraInputObserverSO _playerInput;

    [ReadOnly][SerializeField] private float _holdTime = 0;
    private bool _holding = false;

    private void Awake()
    {
        DisableInput();
        _bookRef.SetActive(false);
    }

    private void EnableInput()
    {
        _playerInput.OnPressE_ButtonChannel += HandlePressEInput;
        _playerInput.OnLeftClickChannel += HandlePressLeftClickInput;
        _playerInput.OnRightClickChannel += HandleHoldRightClickInput;
        _bookRef.SetActive(true);
    }

    private void DisableInput()
    {
        _playerInput.OnPressE_ButtonChannel -= HandlePressEInput;
        _playerInput.OnLeftClickChannel -= HandlePressLeftClickInput;
        _playerInput.OnRightClickChannel -= HandleHoldRightClickInput;
        _bookRef.SetActive(false);
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
        if (_yantCaster != null && context.started)
        {
            _yantCaster.tryCastYant(_holdTime);
            _holdTime = 0;
        }
    }

    private void HandleHoldRightClickInput(Vector2 position, InputAction.CallbackContext context)
    {
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
    public void SetEnableUseInputObserverSO(bool enable)
    {
       if (enable) EnableInput();
       else DisableInput();
    }


    #endregion
}
