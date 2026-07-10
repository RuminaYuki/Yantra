using UnityEngine;
using Kogetsu.Library.DesignPatternCore;

public class TaniEventListener : MonoBehaviour
{
    [SerializeField] private TaniGhostDriver _taniGhostDriver;

    private void OnEnable()
    {
        if (!EventBus.Instance) return;
        EventBus.Instance.Subscribe<TaniRunningEvent>(OnTaniRunningEvent);

        _taniGhostDriver = GetComponent<TaniGhostDriver>();
    }
    private void OnDisable()
    {
        if (!EventBus.Instance) return;
        EventBus.Instance.Unsubscribe<TaniRunningEvent>(OnTaniRunningEvent);
    }

    private void OnTaniRunningEvent(TaniRunningEvent taniRunning)
    {
        Debug.Log("Tani is running!");

        if (_taniGhostDriver != null)
        {
            StartCoroutine(_taniGhostDriver.TaniEventHandle());
        }
        else
        {
            Debug.LogWarning("TaniGhostDriver component not found on this GameObject.");
        }
    }
}
