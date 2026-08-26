using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "YantraInputObserverSO", menuName = "Observer/YantraInputObserverSO")]
public class YantraInputObserverSO : ScriptableObject
{

    // ─── Movement ──────────────────────────────────────────────────────
    public Action<Vector3> OnMoveChannel;
    public Action<bool> OnRunChannel;
    public Action<bool> OnCrouchChannel;
    public Action OnJumpChannel;

    // ─── Button ─────────────────────────────────────────────────────────
    
    public Action<Vector2, InputAction.CallbackContext> OnLeftClickChannel;
    public Action<Vector2, InputAction.CallbackContext> OnRightClickChannel;
    public Action<Vector2, InputAction.CallbackContext> OnMiddleClickChannel;

    public Action OnInteractionChannel;
    public Action OnPressQ_ButtonChannel;
    public Action OnPressF_ButtonChannel;
    public Action OnPressE_ButtonChannel;
    public Action OnPressCtalR_ButtonChannel;

    public Action OnPressAnyKeyChannel;

    // ─── Send helpers ──────────────────────────────────────────────────
    public void SendMoveSignal(Vector3 value) => OnMoveChannel?.Invoke(value);

    public void SendJumpSignal() => OnJumpChannel?.Invoke();

    public void SendRunSignal(bool isSprinting) => OnRunChannel?.Invoke(isSprinting);
    public void SendCrouchSignal(bool isCrouch) => OnCrouchChannel?.Invoke(isCrouch);

    public void SendInteractionSignal() => OnInteractionChannel?.Invoke();

    public void SendLeftClickSignal(Vector2 position, InputAction.CallbackContext context) => OnLeftClickChannel?.Invoke(position, context);

    public void SendRightClickSignal(Vector2 position, InputAction.CallbackContext context) => OnRightClickChannel?.Invoke(position, context);

    public void SendMiddleClickSignal(Vector2 position, InputAction.CallbackContext context) => OnMiddleClickChannel?.Invoke(position, context);

    public void SendPressQ_ButtonSignal() => OnPressQ_ButtonChannel?.Invoke();
    
    public void SendPressF_ButtonSignal() => OnPressF_ButtonChannel?.Invoke();

    public void SendPressE_ButtinSignal() => OnPressE_ButtonChannel?.Invoke();

    public void SendPressCtalR_ButtonSignal() => OnPressCtalR_ButtonChannel?.Invoke();

    public void SendPressAnyKeySignal() => OnPressAnyKeyChannel?.Invoke();

    public void ClearAllChannels()
    {
        OnPressAnyKeyChannel = null;
        OnMoveChannel = null;
        OnJumpChannel = null;
        OnRunChannel = null;
        OnCrouchChannel = null;
        OnInteractionChannel = null;
        OnLeftClickChannel = null;
        OnRightClickChannel = null;
        OnMiddleClickChannel = null;
        OnPressQ_ButtonChannel = null;
        OnPressF_ButtonChannel = null;
        OnPressE_ButtonChannel = null;
        OnPressCtalR_ButtonChannel = null;
    }
}
