using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Menu popup để chọn consumable item
/// </summary>
public class UI_QuickUseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Button closeButton;
    
    private List<GameObject> spawnedSlots = new List<GameObject>();
    
    private void Awake()
    {
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        // Ẩn menu ban đầu
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Hiện menu và refresh danh sách
    /// </summary>
    public void Show()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            RefreshList();
        }
    }
    
    /// <summary>
    /// Ẩn menu
    /// </summary>
    public void Hide()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Refresh danh sách consumables
    /// </summary>
    private void RefreshList()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager not found!");
            return;
        }
        
        // Clear old slots
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot);
        }
        spawnedSlots.Clear();
        
        // Spawn slots cho mỗi consumable
        for (int i = 0; i < InventoryManager.Instance.SlotCount; i++)
        {
            var slot = InventoryManager.Instance.GetSlot(i);
            
            if (slot == null || slot.IsEmpty()) continue;
            if (!(slot.Item is ConsumableItemData consumable)) continue;
            
            // Tạo UI slot
            if (itemSlotPrefab != null && itemsContainer != null)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, itemsContainer);
                
                // Setup display
                var slotUI = slotObj.GetComponent<UI_QuickUseMenuSlot>();
                if (slotUI != null)
                {
                    slotUI.Setup(consumable, slot.Amount, i);
                }
                
                // Add click listener
                int slotIndex = i; // Capture for lambda
                var button = slotObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => {
                        OnItemSelected(slotIndex);
                    });
                }
                
                spawnedSlots.Add(slotObj);
            }
        }
        
        // Nếu không có consumable nào
        if (spawnedSlots.Count == 0)
        {
            Debug.Log("[QuickUseMenu] Không có consumable để hiển thị");
        }
    }
    
    /// <summary>
    /// Khi player chọn item
    /// </summary>
    private void OnItemSelected(int slotIndex)
    {
        Debug.Log($"[QuickUseMenu] Selected slot {slotIndex}");
        
        if (QuickUseManager.Instance != null)
        {
            QuickUseManager.Instance.SelectConsumable(slotIndex);
        }
        
        Hide();
    }
}
