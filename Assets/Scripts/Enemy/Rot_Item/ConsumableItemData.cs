using UnityEngine;

/// <summary>
/// Item có thể tiêu thụ (potion, buff items)
/// </summary>
[CreateAssetMenu(menuName = "Game/Items/Consumable Item")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable Effects")]
    [Tooltip("Lượng HP hồi phục")]
    public int healthRestore = 0;
    
    [Tooltip("Lượng MP hồi phục")]
    public int manaRestore = 0;
    
    [Header("Buff Effects (Optional)")]
    [Tooltip("Thời gian buff (giây), 0 = không có buff")]
    public float buffDuration = 0f;
    
    [Tooltip("Tăng ATK tạm thời")]
    public int atkBonus = 0;
    
    [Tooltip("Tăng DEF tạm thời")]
    public int defBonus = 0;
    
    [Tooltip("Tăng tốc độ di chuyển (%)")]
    public float speedBonus = 0f;

    public ConsumableItemData()
    {
        itemType = ItemType.Consumable;
        stackable = true;
        description = "Vật phẩm tiêu hao";
    }
}
