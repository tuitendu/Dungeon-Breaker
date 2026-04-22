using UnityEngine;

/// <summary>
/// Item trang bị (vũ khí, áo giáp, phụ kiện)
/// </summary>
[CreateAssetMenu(menuName = "Game/Items/Equipment Item")]
public class EquipmentItemData : ItemData
{
    [Header("Equipment Info")]
    [Tooltip("Vị trí trang bị")]
    public EquipmentSlot equipSlot = EquipmentSlot.Weapon;
    
    [Header("Stat Bonuses")]
    [Tooltip("Tăng HP tối đa")]
    public int hpBonus = 0;
    
    [Tooltip("Hồi phục % HP mỗi giây")]
    [Range(0f, 100f)]
    public float hpRegenPctBonus = 0f;
    
    [Tooltip("Tăng MP tối đa")]
    public int mpBonus = 0;
    
    [Tooltip("Hồi phục % MP mỗi giây")]
    [Range(0f, 100f)]
    public float mpRegenPctBonus = 0f;
    
    [Tooltip("Tăng sức tấn công vật lý")]
    public int atkBonus = 0;
    
    [Tooltip("Tăng sức tấn công phép")]
    public int matkBonus = 0;
    
    [Tooltip("Tăng phòng thủ vật lý")]
    public int defBonus = 0;
    
    [Tooltip("Tăng phòng thủ phép")]
    public int mdefBonus = 0;
    
    [Header("Special Stats")]
    [Tooltip("Tăng tỷ lệ chí mạng (%)")]
    [Range(0f, 100f)]
    public float critRateBonus = 0f;
    
    [Tooltip("Tăng tốc độ di chuyển (%)")]
    [Range(0f, 100f)]
    public float speedBonus = 0f;

    public EquipmentItemData()
    {
        itemType = ItemType.Equipment;
        stackable = false; // Equipment không thể xếp chồng
        description = "Trang bị";
    }

    /// <summary>
    /// Lấy tổng bonus stats dạng string để hiển thị
    /// </summary>
    public string GetBonusStatsText()
    {
        string stats = "";
        
        if (hpBonus > 0) stats += $"HP +{hpBonus}\n";
        if (hpRegenPctBonus > 0) stats += $"HP Regen +{hpRegenPctBonus}%\n";
        if (mpBonus > 0) stats += $"MP +{mpBonus}\n";
        if (mpRegenPctBonus > 0) stats += $"MP Regen +{mpRegenPctBonus}%\n";
        if (atkBonus > 0) stats += $"ATK +{atkBonus}\n";
        if (matkBonus > 0) stats += $"MATK +{matkBonus}\n";
        if (defBonus > 0) stats += $"DEF +{defBonus}\n";
        if (mdefBonus > 0) stats += $"MDEF +{mdefBonus}\n";
        if (critRateBonus > 0) stats += $"Crit Rate +{critRateBonus}%\n";
        if (speedBonus > 0) stats += $"Speed +{speedBonus}\n";
        
        return stats.TrimEnd('\n');
    }
}
