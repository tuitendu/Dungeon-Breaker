using UnityEngine;

/// <summary>
/// Data structure cho 1 ô chứa đồ trong inventory
/// </summary>
[System.Serializable]
public class InventorySlot
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int itemAmount;

    // Properties
    public ItemData Item => itemData;
    public int Amount => itemAmount;

    // Constructor
    public InventorySlot()
    {
        itemData = null;
        itemAmount = 0;
    }

    public InventorySlot(ItemData item, int amount)
    {
        itemData = item;
        itemAmount = amount;
    }

    /// <summary>
    /// Slot có trống không?
    /// </summary>
    public bool IsEmpty()
    {
        return itemData == null || itemAmount <= 0;
    }

    /// <summary>
    /// Slot có thể stack với item này không?
    /// </summary>
    public bool CanStackWith(ItemData otherItem, int maxStackSize)
    {
        if (IsEmpty()) return false;
        if (itemData != otherItem) return false;
        if (!itemData.stackable) return false;
        if (itemAmount >= maxStackSize) return false;

        return true;
    }

    /// <summary>
    /// Thêm item vào slot này (dùng cho stacking)
    /// </summary>
    public int AddAmount(int amount, int maxStackSize)
    {
        if (IsEmpty())
        {
            Debug.LogError("Cannot AddAmount to empty slot!");
            return 0;
        }

        int spaceLeft = maxStackSize - itemAmount;
        int amountToAdd = Mathf.Min(amount, spaceLeft);

        itemAmount += amountToAdd;

        return amountToAdd; // Return số lượng thực tế đã add
    }

    /// <summary>
    /// Trừ item từ slot
    /// </summary>
    public bool RemoveAmount(int amount)
    {
        if (IsEmpty()) return false;

        if (itemAmount < amount)
        {
            Debug.LogWarning($"Không đủ {itemData.itemName} để xóa! Có: {itemAmount}, Cần: {amount}");
            return false;
        }

        itemAmount -= amount;

        // Nếu hết item, clear slot
        if (itemAmount <= 0)
        {
            Clear();
        }

        return true;
    }

    /// <summary>
    /// Set item mới vào slot (dùng khi slot trống)
    /// </summary>
    public void SetItem(ItemData item, int amount)
    {
        itemData = item;
        itemAmount = amount;
    }

    /// <summary>
    /// Xóa slot (reset về trống)
    /// </summary>
    public void Clear()
    {
        itemData = null;
        itemAmount = 0;
    }

    /// <summary>
    /// Get thông tin debug
    /// </summary>
    public override string ToString()
    {
        if (IsEmpty())
            return "[Empty Slot]";

        return $"[{itemData.itemName} x{itemAmount}]";
    }
}
