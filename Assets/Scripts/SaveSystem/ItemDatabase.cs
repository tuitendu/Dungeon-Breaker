using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject registry that maps item id/name to ItemData assets.
/// Create one via Assets > Create > SaveSystem > ItemDatabase,
/// then drag all ItemData assets into the Items list.
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "SaveSystem/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("Drag all ItemData assets here.")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    private Dictionary<string, ItemData> _lookupByName;
    private Dictionary<string, ItemData> _lookupById;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        _lookupByName = new Dictionary<string, ItemData>(items.Count);
        _lookupById   = new Dictionary<string, ItemData>(items.Count);

        foreach (ItemData item in items)
        {
            if (item == null) continue;

            // Index theo id (ưu tiên)
            if (!string.IsNullOrEmpty(item.id))
            {
                if (_lookupById.ContainsKey(item.id))
                    Debug.LogWarning("[ItemDatabase] Duplicate item id: " + item.id);
                else
                    _lookupById[item.id] = item;
            }

            // Index theo itemName (backward compat)
            if (!string.IsNullOrEmpty(item.itemName))
            {
                if (_lookupByName.ContainsKey(item.itemName))
                    Debug.LogWarning("[ItemDatabase] Duplicate item name: " + item.itemName);
                else
                    _lookupByName[item.itemName] = item;
            }
        }

        Debug.Log($"[ItemDatabase] Loaded {_lookupById.Count} items by id, {_lookupByName.Count} by name.");
    }

    /// <summary>Tìm item theo id (khuyên dùng).</summary>
    public ItemData GetItemById(string id)
    {
        if (_lookupById == null) BuildLookup();
        if (string.IsNullOrEmpty(id)) return null;
        _lookupById.TryGetValue(id, out ItemData result);
        return result;
    }

    /// <summary>Tìm item theo itemName (backward compat). Nếu không thấy, thử dùng id.</summary>
    public ItemData GetItemByName(string name)
    {
        if (_lookupByName == null) BuildLookup();
        if (string.IsNullOrEmpty(name)) return null;

        if (_lookupByName.TryGetValue(name, out ItemData result)) return result;

        // Fallback: thử tìm bằng id
        if (_lookupById == null) BuildLookup();
        _lookupById.TryGetValue(name, out result);
        return result;
    }
}
