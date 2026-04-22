using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI slot trong QuickUseMenu
/// </summary>
public class UI_QuickUseMenuSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private Image background;
    
    /// <summary>
    /// Setup slot với consumable data
    /// </summary>
    public void Setup(ConsumableItemData item, int count, int slotIndex)
    {
        if (item == null) return;
        
        // Icon
        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = (item.icon != null);
        }
        
        // Name
        if (nameText != null)
        {
            nameText.text = item.itemName;
        }
        
        // Count
        if (countText != null)
        {
            countText.text = $"x{count}";
        }
        
        // Rarity border color
        if (rarityBorder != null)
        {
            rarityBorder.color = GetRarityColor(item.rarity);
        }
    }
    
    /// <summary>
    /// Lấy màu theo rarity
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f); // Xám nhạt
            
            case ItemRarity.Uncommon:
                return new Color(0.2f, 0.8f, 0.2f); // Xanh lá
            
            case ItemRarity.Rare:
                return new Color(0.2f, 0.5f, 1f); // Xanh dương
            
            case ItemRarity.Epic:
                return new Color(0.6f, 0.2f, 1f); // Tím
            
            case ItemRarity.Legendary:
                return new Color(1f, 0.5f, 0f); // Vàng cam
            
            default:
                return Color.white;
        }
    }
}
