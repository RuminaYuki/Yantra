using UnityEngine;

public class TestEventChannelListener : MonoBehaviour
{
    [SerializeField]
    private VoidEventChannelSO _eventChannel;

    private void OnEnable()
    {
        if (_eventChannel == null)
        {
            Debug.LogWarning(
                "TestEventChannelListener has no Event Channel.",
                this);

            return;
        }

        _eventChannel.Raised += OnEventRaised;
    }

    private void OnDisable()
    {
        if (_eventChannel != null)
        {
            _eventChannel.Raised -= OnEventRaised;
        }
    }

    private void OnEventRaised()
    {
        Debug.Log(
            $"{name} received event from {_eventChannel.name}.",
            this);
    }
}