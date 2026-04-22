using UnityEngine;

/// <summary>
/// Enums cho hệ thống item
/// </summary>
public enum ItemType
{
    Material,      // Nguyên liệu thông thường
    Consumable,    // Potion, buff items
    Equipment,     // Vũ khí, áo giáp
    QuestItem      // Item nhiệm vụ
}

public enum ItemRarity
{
    Common,        // Trắng
    Uncommon,      // Xanh lá
    Rare,          // Xanh dương
    Epic,          // Tím
    Legendary      // Vàng
}

public enum EquipmentSlot
{
    None,
    Weapon,
    Armor,
    Pants,
    Boots,
    Accessory
}

[CreateAssetMenu(menuName = "Game/Items/Base Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string itemName;
    public Sprite icon;
    
    [Header("Item Type")]
    public ItemType itemType = ItemType.Material;
    public ItemRarity rarity = ItemRarity.Common;
    
    [Header("Description")]
    [TextArea(2, 4)]
    public string description = "";
    
    [Header("Stacking")]
    public bool stackable = true;
    public int maxStack = 99; // Số lượng tối đa trong 1 stack
    
    [Header("Value")]
    public int sellPrice = 10; // Giá bán cho shop
}

