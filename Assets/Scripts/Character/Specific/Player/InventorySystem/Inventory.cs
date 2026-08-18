using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();

    public bool TryAddItem(ItemData item, int amount)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot add a null item.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("Amount must be greater than 0.");
            return false;
        }

        if (CheckMaxStack(item, amount))
        {
            Debug.LogWarning("item is max stack");
            return false;
        }

        if (_inventory.ContainsKey(item))
        {
            _inventory[item] += amount;
        }
        else
        {
            _inventory.Add(item, amount);
        }

        Debug.Log($"Added item: {item.ItemName} | Amount: {amount} | Total: {_inventory[item]}");
        return true;
    }

    public bool TryRemoveItem(ItemData item, int amount)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot remove a null item.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("Amount must be greater than 0.");
            return false;
        }

        if (CheckMinStack(item, amount))
        {
            Debug.LogWarning("Not enough items to remove.");
            return false;
        }

        if (_inventory.ContainsKey(item))
        {
            _inventory[item] -= amount;
        }
        else
        {
            return false;
        }

        Debug.Log($"Removed item: {item.ItemName} | Amount: {amount} | Total: {_inventory[item]}");
        return true;
    }

    public bool CheckMaxStack(ItemData item, int amount)
    {
        if (amount < 0) amount = -amount;

        foreach (var pair in _inventory)
        {
            if (pair.Key == item)
            {
                if (pair.Value + amount > pair.Key.MaxStack)
                    return true;
            }
        }
        return false;
    }

    public bool CheckMinStack(ItemData item, int amount)
    {
        if (amount > 0) amount = -amount;

        if (_inventory.TryGetValue(item, out int count))
        {
            if (count + amount < 0)
                return true;  // Not enough items
            return false;  // Enough items
        }
        return true;  // Item not found
    }

    public int GetItemCount(ItemData item)
    {
        return _inventory.TryGetValue(item, out int count) ? count : 0;
    }
}
