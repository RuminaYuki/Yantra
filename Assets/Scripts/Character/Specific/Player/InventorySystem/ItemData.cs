using UnityEngine;

public enum ItemType
{
    Consumable,
    KeyItem,
    QuestItem,
    Material,
    Equipment
}

[CreateAssetMenu(
    fileName = "New Item Data",
    menuName = "Player/Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;

    [Header("Display")]
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject gameObject;

    [Header("Item Settings")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private bool stackable = true;
    [SerializeField] private int maxStack = 99;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public GameObject GameObject => gameObject;
    public ItemType ItemType => itemType;
    public bool Stackable => stackable;
    public int MaxStack => maxStack;
}
