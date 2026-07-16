using Kogetsu.Library.DesignPatternCore;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class YantraInputManager : MonoBehaviour
{
    [Header("ObserverChannels")]
    [Required]
    [SerializeField] protected YantraInputObserverSO InputObserver;

    private PlayerInput _playerInput;

    private void Awake()
    {
        TryGetComponent(out _playerInput);
    }

    private void OnEnable()
    {
        if (_playerInput == null)
        {
            TryGetComponent(out _playerInput);
        }

        if (_playerInput == null)
        {
            return;
        }

        _playerInput.ActivateInput();

        if (!string.IsNullOrEmpty(_playerInput.defaultActionMap))
        {
            _playerInput.SwitchCurrentActionMap(_playerInput.defaultActionMap);
        }
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("MoveInputChannelSO is not Assign");
#endif
            return;
        }

        InputObserver.SendMoveSignal(context.ReadValue<Vector3>());
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("JumpEventChannelSO is not Assign");
#endif
            return;
        }

        if (context.performed) InputObserver.SendJumpSignal();
    }

    public void OnRunInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("RunEventChannelSO is not Assign"); return;
#endif
        }

        InputObserver.SendRunSignal(context.performed);
    }

    public void OnLeftClickInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("LeftClickInputSO is not Assign");
#endif
            return;
        }

        var mousePosition = Mouse.current.position.ReadValue();
        var isPressed = context.ReadValueAsButton();

        Debug.Log($"Mouse Position: {mousePosition} | IsPressed: {isPressed}");

        InputObserver.SendLeftClickSignal(mousePosition, isPressed);
    }

    public void OnRightClickInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("RightClickInputSO is not Assign");
#endif
            return;
        }

        var mousePosition = Mouse.current.position.ReadValue();
        var isPressed = context.ReadValueAsButton();

        InputObserver.SendRightClickSignal(mousePosition, isPressed);
    }

    public void OnMiddleClickInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("MiddleClickInputSO is not Assign");
#endif
            return;
        }

        var mousePosition = Mouse.current.position.ReadValue();
        var isPressed = context.ReadValueAsButton();

        InputObserver.SendMiddleClickSignal(mousePosition, isPressed);
    }

    public void OnInteractionInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("InteractionInputSO is not Assign");
#endif
            return;
        }

        if (!context.performed) return;

        InputObserver.SendInteractionSignal();
    }

    public void OnPressQ_ButtonInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("PressQ_ButtonInputSO is not Assign");
#endif
            return;
        }
        if (!context.performed) return;

        InputObserver.SendPressQ_ButtonSignal();
    }

    public void OnPressF_ButtonInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("PressF_ButtonInputSO is not Assign");
#endif
            return;
        }
        if (!context.performed) return;

        InputObserver.SendPressF_ButtonSignal();
    }

    public void OnPressCtalR_ButtonInput(InputAction.CallbackContext context)
    {
        if (InputObserver == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("PressCtalR_ButtonInputSO is not Assign");
#endif
        }
        if (!context.performed) return;

        InputObserver.SendPressCtalR_ButtonSignal();
    }
}
