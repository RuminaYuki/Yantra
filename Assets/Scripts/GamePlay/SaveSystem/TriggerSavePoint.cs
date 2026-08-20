using UnityEngine;

public class TriggerSavePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("Save point ignored: no SaveFile exists in the scene.");
            return;
        }

        SaveManager.Instance.SaveAll();
    }
}
