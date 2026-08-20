using UnityEngine;

public class VoidEventChannelGUIInvoker : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _eventChannel;
    [SerializeField] private string _buttonLabel = "Raise Event";
    [SerializeField] private Rect _buttonRect = new Rect(20f, 20f, 160f, 40f);

    private void OnGUI()
    {
        if (!GUI.Button(_buttonRect, _buttonLabel))
        {
            return;
        }

        if (_eventChannel == null)
        {
            Debug.LogWarning(
                "VoidEventChannelGUIInvoker has no Event Channel.",
                this);
            return;
        }

        _eventChannel.Raise();

        Debug.Log(
            $"Raised event channel '{_eventChannel.name}'.",
            this);
    }
}
