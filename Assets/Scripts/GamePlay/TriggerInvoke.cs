using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerInvoke : MonoBehaviour
{
    [SerializeField] private List<VoidEventChannelSO> EventChannels = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var channel in EventChannels)
            {
                if (channel != null)
                {
                    channel.Raise();
                }
            }
        }
    }
}
