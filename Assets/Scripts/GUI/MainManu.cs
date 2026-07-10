using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MainManu : MonoBehaviour
{
    [SerializeField] private YantraInputObserverSO inputObserver;
    [SerializeField] private bool sendOnlyOnce = true;
    [SerializeField] private float inputDelay = 0.25f;
    [SerializeField] private UnityEvent<string> onAnyKeyPressed;

    private bool _hasSent;
    private float _readyTime;

    public event System.Action<string> AnyKeyPressed;
    public bool HasSent => _hasSent;
    public string LastInputName { get; private set; }

    private void OnEnable()
    {
        _hasSent = false;
        LastInputName = string.Empty;
        _readyTime = Time.unscaledTime + inputDelay;
    }

    private void Update()
    {
        if (sendOnlyOnce && _hasSent)
        {
            return;
        }

        if (Time.unscaledTime < _readyTime)
        {
            return;
        }

        if (TryGetAnyInputName(out string inputName))
        {
            SendAnyKeyData(inputName);
        }
    }

    public void SendAnyKeyData(string inputName)
    {
        if (sendOnlyOnce && _hasSent)
        {
            return;
        }

        _hasSent = true;
        LastInputName = inputName;

        inputObserver?.SendPressAnyKeySignal();
        onAnyKeyPressed?.Invoke(inputName);
        AnyKeyPressed?.Invoke(inputName);
    }

    public void ResetInputGate()
    {
        _hasSent = false;
        LastInputName = string.Empty;
        _readyTime = Time.unscaledTime + inputDelay;
    }

    private static bool TryGetAnyInputName(out string inputName)
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    inputName = key.displayName;
                    return true;
                }
            }

            inputName = "Keyboard";
            return true;
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                inputName = "MouseLeft";
                return true;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                inputName = "MouseRight";
                return true;
            }

            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                inputName = "MouseMiddle";
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            inputName = "MouseLeft";
            return true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            inputName = "MouseRight";
            return true;
        }

        if (Input.GetMouseButtonDown(2))
        {
            inputName = "MouseMiddle";
            return true;
        }

        if (!Input.anyKeyDown)
        {
            inputName = string.Empty;
            return false;
        }

        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                inputName = keyCode.ToString();
                return true;
            }
        }

        inputName = "AnyKey";
        return true;
    }
}
