using UnityEngine;

public class TriggerSavePoint : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private YantraStatsController stats;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (player == null) player = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform;
        if (stats == null) stats = player.GetComponentInChildren<YantraStatsController>();

        if (SaveFile.Instance == null)
        {
            Debug.LogWarning("Save point ignored: no SaveFile exists in the scene.");
            return;
        }

        SaveFile.Instance.SaveGame(player, stats);
    }
}
