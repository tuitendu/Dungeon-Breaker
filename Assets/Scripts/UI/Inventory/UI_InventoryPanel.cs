using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý toàn bộ Inventory UI Panel
/// </summary>
public class UI_InventoryPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;

    [Header("Settings")]
    [SerializeField] private bool autoGenerateSlots = true;

    private List<UI_InventorySlot> uiSlots = new List<UI_InventorySlot>();

    private void Awake()
    {
        // Validate
        if (slotPrefab == null)
        {
            Debug.LogError("UI_InventoryPanel: slotPrefab chưa gán!", this);
        }

        if (slotContainer == null)
        {
            Debug.LogError("UI_InventoryPanel: slotContainer chưa gán!", this);
        }
    }

    private void Start()
    {
        Debug.Log("[UI_InventoryPanel] START called!");
        
        // Wait for InventoryManager to initialize
        if (InventoryManager.Instance != null && autoGenerateSlots)
        {
            GenerateSlots();
            
            // CRITICAL FIX: Manually subscribe here to ensure we catch the event
            // OnEnable might run before InventoryManager.Instance exists
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI; // Remove first to avoid double subscription
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            Debug.Log("[UI_InventoryPanel] Subscribed to OnInventoryChanged event!");
            
            RefreshUI();
        }
        else
        {
            Debug.LogWarning("InventoryManager.Instance chưa tồn tại! Retrying...");
            // Retry sau 0.5s
            StartCoroutine(RetrySubscription());
        }
    }

    private System.Collections.IEnumerator RetrySubscription()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (InventoryManager.Instance != null)
        {
            if (autoGenerateSlots && uiSlots.Count == 0)
            {
                GenerateSlots();
            }
            
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            Debug.Log("[UI_InventoryPanel] RETRY: Subscribed to OnInventoryChanged event!");
            
            RefreshUI();
        }
        else
        {
            Debug.LogError("InventoryManager.Instance STILL NULL after retry!");
        }
    }

    private void OnEnable()
    {
        // Subscribe to inventory events
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            Debug.Log("[UI_InventoryPanel] OnEnable: Subscribed to event");
        }
        else
        {
            Debug.LogWarning("[UI_InventoryPanel] OnEnable: InventoryManager.Instance is NULL - cannot subscribe yet");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    /// <summary>
    /// Tạo UI slots dựa trên inventory size
    /// </summary>
    private void GenerateSlots()
    {
        if (InventoryManager.Instance == null) return;

        int inventorySize = InventoryManager.Instance.InventorySize;

        // Clear existing
        ClearSlots();

        // Generate new slots
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            slotObj.name = $"Slot_{i}";

            UI_InventorySlot uiSlot = slotObj.GetComponent<UI_InventorySlot>();

            if (uiSlot != null)
            {
                uiSlots.Add(uiSlot);
            }
            else
            {
                Debug.LogError($"Slot prefab không có UI_InventorySlot component!");
            }
        }

        Debug.Log($"Generated {uiSlots.Count} inventory slots");
    }

    /// <summary>
    /// Refresh toàn bộ UI (gọi khi inventory thay đổi)
    /// </summary>
    public void RefreshUI()
    {
        Debug.Log("[UI_InventoryPanel] 🔄 RefreshUI called!");
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Cannot refresh UI: InventoryManager.Instance is null");
            return;
        }

        InventorySlot[] slots = InventoryManager.Instance.Slots;
        Debug.Log($"[UI_InventoryPanel] Refreshing {uiSlots.Count} UI slots with {slots.Length} data slots");

        // Update each UI slot
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < slots.Length)
            {
                uiSlots[i].UpdateSlot(slots[i], i);
            }
        }
    }

    /// <summary>
    /// Xóa tất cả UI slots
    /// </summary>
    private void ClearSlots()
    {
        foreach (var slot in uiSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        uiSlots.Clear();
    }

    /// <summary>
    /// Toggle inventory panel
    /// </summary>
    public void ToggleInventory()
    {
        gameObject.SetActive(!gameObject.activeSelf);

        if (gameObject.activeSelf)
        {
            RefreshUI();
        }
    }

    public void ShowInventory()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void HideInventory()
    {
        gameObject.SetActive(false);
    }

    private void OnValidate()
    {
        // Auto-find slotContainer nếu chưa gán
        if (slotContainer == null)
        {
            Transform container = transform.Find("SlotContainer");
            if (container != null)
                slotContainer = container;
        }
    }
}
