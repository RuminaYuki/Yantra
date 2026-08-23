using UnityEngine;

public class DebugAddItem : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private ItemData yantItem;
    private void Awake()
    {
        if (_inventory != null) _inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            _inventory.TryAddItem(yantItem, 1);
        }
    }
}
