using UnityEngine;

public class PickupInteractable : MonoBehaviour
{
    private Iinteractable interactable;

    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount;

    private void Awake()
    {
        interactable = GetComponent<Iinteractable>();
    }

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.OnInteract += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.OnInteract -= OnInteract;
        }
    }

    public void OnInteract(GameObject rootPlayer)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemData is not assigned on " + gameObject.name);
            return;
        }

        Inventory inventory = rootPlayer.GetComponentInChildren<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("Inventory component not found on player");
            return;
        }

        if (!inventory.TryAddItem(itemData, amount))
        {
            Debug.LogWarning("Failed to add item to inventory");
            return;
        }

        this.gameObject.SetActive(false);
    }
}
