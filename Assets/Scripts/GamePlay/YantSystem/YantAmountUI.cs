using TMPro;
using UnityEngine;

public class YantAmountUI : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO EventChannel;
    [SerializeField] private TextMeshProUGUI TextAmount;
    private bool isSubscribed;

    private void OnEnable()
    {
        if (EventChannel == null || isSubscribed)
            return;

        EventChannel.Raised += OnEventRaised;
        isSubscribed = true;
    }

    private void OnDisable()
    {
        if (EventChannel == null || !isSubscribed)
            return;

        EventChannel.Raised -= OnEventRaised;
        isSubscribed = false;
    }

    private void OnEventRaised(int value)
    {
        if (TextAmount == null)
            return;

        TextAmount.text = value.ToString();
    }
}
