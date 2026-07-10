using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

public class TaniRunningEventPublisher : MonoBehaviour
{

    private void OnTriggerEnter(Collider other) 
    {

        if (other.CompareTag("Player"))
       {
           if (EventBus.Instance != null)
           {
               EventBus.Instance.Publish(new TaniRunningEvent());
               Debug.Log("Published TaniRunningEvent!");
               Destroy(this.gameObject);
           }

           else
           {
               Debug.LogWarning("EventBus instance not found. Cannot publish TaniRunningEvent.");
           }
        }   



    }
}
