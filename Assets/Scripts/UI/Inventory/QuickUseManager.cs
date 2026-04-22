using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý quick use button - chọn và dùng consumables
/// </summary>
public class QuickUseManager : MonoBehaviour
{
    public static QuickUseManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private UI_QuickUseButton quickButton;
    [SerializeField] private UI_QuickUseMenu itemMenu;
    
    [Header("Settings")]
    [SerializeField] private bool autoSelectFirst = true;
    
    private ConsumableItemData currentItem;
    private int currentSlotIndex = -1;
    
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate QuickUseManager! Destroying.");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Subscribe to inventory changes
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshCurrentItem;
        }
        
        // Tự động chọn consumable đầu tiên
        if (autoSelectFirst)
        {
            AutoSelectFirstConsumable();
        }
    }
    
    /// <summary>
    /// Dùng consumable hiện tại
    /// </summary>
    public void UseCurrentItem()
    {
        if (currentSlotIndex < 0)
        {
            Debug.Log("[QuickUse] Chưa chọn item!");
            return;
        }
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }
        
        // Null check to prevent error when currentItem is null
        if (currentItem == null)
        {
            Debug.LogWarning("[QuickUse] Item is null! Auto-selecting...");
            AutoSelectFirstConsumable();
            return;
        }
        
        Debug.Log($"[QuickUse] Attempting to use {currentItem.itemName} at slot {currentSlotIndex}");
        
        bool success = InventoryManager.Instance.UseItem(currentSlotIndex);
        
        if (success)
        {
            Debug.Log($"[QuickUse] Đã dùng {currentItem.itemName}");
            
            // Refresh sẽ tự động gọi qua event
        }
        else
        {
            Debug.LogWarning($"[QuickUse] Không thể dùng item!");
        }
    }
    
    /// <summary>
    /// Mở menu chọn item
    /// </summary>
    public void OpenItemMenu()
    {
        if (itemMenu != null)
        {
            itemMenu.Show();
        }
        else
        {
            Debug.LogWarning("QuickUseMenu chưa gán!");
        }
    }
    
    /// <summary>
    /// Chọn consumable từ slot index
    /// </summary>
    public void SelectConsumable(int slotIndex)
    {
        if (InventoryManager.Instance == null) return;
        
        var slot = InventoryManager.Instance.GetSlot(slotIndex);
        
        if (slot != null && !slot.IsEmpty() && slot.Item is ConsumableItemData consumable)
        {
            currentItem = consumable;
            currentSlotIndex = slotIndex;
            
            Debug.Log($"[QuickUse] Đổi sang {consumable.itemName} (slot {slotIndex})");
            
            RefreshDisplay();
        }
    }
    
    /// <summary>
    /// Refresh khi inventory thay đổi
    /// </summary>
    private void RefreshCurrentItem()
    {
        if (currentSlotIndex < 0)
        {
            AutoSelectFirstConsumable();
            return;
        }
        
        if (InventoryManager.Instance == null) return;
        
        var slot = InventoryManager.Instance.GetSlot(currentSlotIndex);
        
        // Nếu slot trống hoặc không còn consumable → tìm item khác
        if (slot == null || slot.IsEmpty() || !(slot.Item is ConsumableItemData))
        {
            Debug.Log("[QuickUse] Item hết hoặc slot trống → Auto-select");
            AutoSelectFirstConsumable();
            return;
        }
        
        // Update display với count mới
        currentItem = slot.Item as ConsumableItemData;
        RefreshDisplay();
    }
    
    private void RefreshDisplay()
    {
        if (quickButton == null) return;
        
        if (currentItem != null && currentSlotIndex >= 0)
        {
            var slot = InventoryManager.Instance.GetSlot(currentSlotIndex);
            int count = slot != null ? slot.Amount : 0;
            
            quickButton.UpdateDisplay(currentItem, count);
        }
        else
        {
            quickButton.UpdateDisplay(null, 0);
        }
    }
    
    /// <summary>
    /// Tự động chọn consumable đầu tiên trong inventory
    /// </summary>
    private void AutoSelectFirstConsumable()
    {
        if (InventoryManager.Instance == null)
        {
            currentItem = null;
            currentSlotIndex = -1;
            RefreshDisplay();
            return;
        }
        
        // Tìm consumable đầu tiên
        for (int i = 0; i < InventoryManager.Instance.SlotCount; i++)
        {
            var slot = InventoryManager.Instance.GetSlot(i);
            
            if (slot != null && !slot.IsEmpty() && slot.Item is ConsumableItemData)
            {
                SelectConsumable(i);
                return;
            }
        }
        
        // Không có consumable nào
        Debug.Log("[QuickUse] Không có consumable trong inventory");
        currentItem = null;
        currentSlotIndex = -1;
        RefreshDisplay();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        // Unsubscribe
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshCurrentItem;
        }
    }
}
