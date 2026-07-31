using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private string promptFormat = "Press {0} to pick up {1}";

    public string ItemId => string.IsNullOrWhiteSpace(itemId) ? gameObject.name : itemId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;

    private void Reset()
    {
        itemId = gameObject.name;
        displayName = gameObject.name;
    }

    public string GetPromptText(string keyName)
    {
        string safeKeyName = string.IsNullOrWhiteSpace(keyName) ? "E" : keyName;
        return string.Format(promptFormat, safeKeyName, DisplayName);
    }
}
