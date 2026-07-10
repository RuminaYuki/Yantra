using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

public class KaEventPublisher : MonoBehaviour
{
    [SerializeField] private KaEventRunName eventName = KaEventRunName.Event1;
    [SerializeField] private bool publishOnce = true;

    private bool hasPublished;

    private void OnTriggerEnter(Collider other)
    {
        if (publishOnce && hasPublished)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (EventBus.Instance == null)
        {
            Debug.LogWarning($"{nameof(KaEventPublisher)} on {name} cannot publish because EventBus instance was not found.");
            return;
        }

        EventBus.Instance.Publish(new KaEventRun(eventName));
        hasPublished = true;
        Debug.Log($"Published {nameof(KaEventRun)} {eventName}.");
    }
}
