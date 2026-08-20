using UnityEditor.PackageManager.Requests;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerInvoke : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO EventChannel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (EventChannel != null)
            {
                EventChannel.Raise();
            }
        }
    }
}
