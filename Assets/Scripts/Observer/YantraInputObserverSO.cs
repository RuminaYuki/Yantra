using System;
using UnityEngine;

[CreateAssetMenu(fileName = "YantraInputObserverSO", menuName = "Observer/YantraInputObserverSO")]
public class YantraInputObserverSO : ScriptableObject
{

    // ─── Movement ──────────────────────────────────────────────────────
    public Action<Vector3> OnMoveChannel;
    public Action<bool> OnRunChannel;
    public Action OnJumpChannel;

    // ─── Button ─────────────────────────────────────────────────────────
    
    public Action<Vector2, bool> OnLeftClickChannel;
    public Action<Vector2, bool> OnRightClickChannel;
    public Action<Vector2, bool> OnMiddleClickChannel;

    public Action OnInteractionChannel;
    public Action OnPressQ_ButtonChannel;
    public Action OnPressF_ButtonChannel;

    public Action OnPressAnyKeyChannel;

    // ─── Send helpers ──────────────────────────────────────────────────
    public void SendMoveSignal(Vector3 value) => OnMoveChannel?.Invoke(value);

    public void SendJumpSignal() => OnJumpChannel?.Invoke();

    public void SendRunSignal(bool isSprinting) => OnRunChannel?.Invoke(isSprinting);

    public void SendInteractionSignal() => OnInteractionChannel?.Invoke();

    public void SendLeftClickSignal(Vector2 position, bool isPressed) => OnLeftClickChannel?.Invoke(position, isPressed);

    public void SendRightClickSignal(Vector2 position, bool isPressed) => OnRightClickChannel?.Invoke(position, isPressed);

    public void SendMiddleClickSignal(Vector2 position, bool isPressed) => OnMiddleClickChannel?.Invoke(position, isPressed);

    public void SendPressQ_ButtonSignal() => OnPressQ_ButtonChannel?.Invoke();
    
    public void SendPressF_ButtonSignal() => OnPressF_ButtonChannel?.Invoke();

    public void SendPressAnyKeySignal() => OnPressAnyKeyChannel?.Invoke();

    public void ClearAllChannels()
    {
        OnPressAnyKeyChannel = null;
        OnMoveChannel = null;
        OnJumpChannel = null;
        OnRunChannel = null;
        OnInteractionChannel = null;
        OnLeftClickChannel = null;
        OnRightClickChannel = null;
        OnMiddleClickChannel = null;
    }
}
