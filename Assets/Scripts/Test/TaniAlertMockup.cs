using UnityEngine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;


public class TaniAlertMockup : MonoBehaviour
{
    public VoidEventChannelSO TaniAlertEvent;
    public TextMeshProUGUI text;
    public float Time = 5f;
    void OnEnable()
    {
        TaniAlertEvent.Raised += HeadelEvent;
    }
    void OnDisable()
    {
        TaniAlertEvent.Raised -= HeadelEvent;
    }
    private void Start()
    {
        text.text = string.Empty;
    }

    IEnumerator TaniCountdown()
    {
        text.text = "Tani Ready To Attack you";
        yield return new WaitForSeconds(Time);
        text.text = "";
    }

    void HeadelEvent()
    {
        StartCoroutine(TaniCountdown());
    }
}
