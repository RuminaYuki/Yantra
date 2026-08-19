using TMPro;
using UnityEngine;

public class YantAmountUI : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO EventChannel;
    [SerializeField] private TextMeshProUGUI TextAmount;

    private void Awake()
    {
        if (EventChannel != null)
        {
            EventChannel.Raised += OnEventRaised;
        }
    }

    private void OnEventRaised(int value)
    {
        if (EventChannel != null)
        {
            TextAmount.text = value.ToString();
        }
    }
}
