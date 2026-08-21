using UnityEngine;

public class PlayerSaveStat : MonoBehaviour, ISave, ISaveLoad
{
    [SerializeField] private GameObject PlayerRoot;
    [SerializeField] private PlayerSave PlayerSave;

    public void Save(SlotSaveSO slot)
    {
        if (!ValidateReferences()) return;

        Transform playerTransform = PlayerRoot.transform;

        PlayerSave.Position = playerTransform.position;
        PlayerSave.Rotation = playerTransform.rotation;
        PlayerSave.Scale = playerTransform.localScale;
        PlayerSave.HasData = true;

        if (TryGetPlayerHealth(out Health health))
        {
            PlayerSave.Health = health.CurrentHP;
        }
    }

    public void Load(SlotSaveSO slot)
    {
        if (!ValidateReferences()) return;

        if (!PlayerSave.HasData)
        {
            Debug.LogWarning($"{nameof(PlayerSaveStat)} load ignored: player save has no data yet.");
            return;
        }

        Transform playerTransform = PlayerRoot.transform;
        playerTransform.SetPositionAndRotation(PlayerSave.Position, PlayerSave.Rotation);
        playerTransform.localScale = PlayerSave.Scale;

        if (TryGetPlayerHealth(out Health health))
        {
            health.SetCurrentHealth(PlayerSave.Health);
        }
    }

    private bool ValidateReferences()
    {
        if (PlayerRoot == null)
        {
            Debug.LogWarning($"{nameof(PlayerSaveStat)} is missing PlayerRoot.", this);
            return false;
        }

        if (PlayerSave == null)
        {
            Debug.LogWarning($"{nameof(PlayerSaveStat)} is missing PlayerSave.", this);
            return false;
        }

        return true;
    }

    private bool TryGetPlayerHealth(out Health health)
    {
        if (TryGetComponent(out health)) return true;
        if (PlayerRoot != null && PlayerRoot.TryGetComponent(out health)) return true;
        if (PlayerRoot != null && PlayerRoot.GetComponentInChildren<Health>() is Health childHealth)
        {
            health = childHealth;
            return true;
        }

        Debug.LogWarning($"{nameof(PlayerSaveStat)} could not find Health on the player.", this);
        return false;
    }
}
