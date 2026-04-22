using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    // Sự kiện khi trang bị thay đổi (UI sẽ lắng nghe cái này)
    public event Action OnEquipmentChanged;

    // Lưu trữ trang bị đang mặc theo Slot
    private Dictionary<EquipmentSlot, EquipmentItemData> equippedItems = new Dictionary<EquipmentSlot, EquipmentItemData>();

    private PlayerStats playerStats;

    // Biến cho Regen (Hồi phục)
    private float hpRegenTimer = 0f;
    private float mpRegenTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        playerStats = GetComponent<PlayerStats>();

        // Khởi tạo dictionary rỗng cho 5 slot
        equippedItems[EquipmentSlot.Weapon] = null;
        equippedItems[EquipmentSlot.Armor] = null;
        equippedItems[EquipmentSlot.Pants] = null;
        equippedItems[EquipmentSlot.Boots] = null;
        equippedItems[EquipmentSlot.Accessory] = null;
    }

    private void Update()
    {
        HandleRegeneration();
    }

    /// <summary>
    /// Mặc trang bị mới. Nếu ô đó đã có đồ thì tháo đồ cũ ra cất vào túi.
    /// </summary>
    public void Equip(EquipmentItemData newItem)
    {
        if (newItem == null) return;

        EquipmentSlot slot = newItem.equipSlot;

        // Nếu ô này đã có đồ đang mặc, tháo nó ra và cho lại vào túi
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            EquipmentItemData oldItem = equippedItems[slot];
            InventoryManager.Instance.AddItem(oldItem, 1);
        }

        // Mặc đồ mới
        equippedItems[slot] = newItem;

        // Cập nhật chỉ số
        if (playerStats != null)
        {
            playerStats.RecomputeStats();
        }

        OnEquipmentChanged?.Invoke();
        Debug.Log($"Đã trang bị: {newItem.itemName} vào ô {slot}");
    }

    /// <summary>
    /// Tháo trang bị hiện tại ở một slot và cất vào túi.
    /// </summary>
    public void Unequip(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            EquipmentItemData oldItem = equippedItems[slot];
            
            // Xóa khỏi người
            equippedItems[slot] = null;

            // Cập nhật chỉ số
            if (playerStats != null)
            {
                playerStats.RecomputeStats();
            }

            // Cho lại vào túi
            InventoryManager.Instance.AddItem(oldItem, 1);

            OnEquipmentChanged?.Invoke();
            Debug.Log($"Đã tháo: {oldItem.itemName} khỏi ô {slot}");
        }
    }

    /// <summary>
    /// Lấy trang bị đang mặc ở một slot cụ thể
    /// </summary>
    public EquipmentItemData GetEquippedItem(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            return equippedItems[slot];
        }
        return null;
    }

    /// <summary>
    /// Kiểm tra xem ô trang bị này có đang trống không
    /// </summary>
    public bool IsSlotEmpty(EquipmentSlot slot)
    {
        return !equippedItems.ContainsKey(slot) || equippedItems[slot] == null;
    }

    /// <summary>
    /// Chỉ dùng khi load game — trang bị thẳng vào slot mà KHÔNG trả đồ cũ về túi.
    /// Tránh vòng lặp: load inventory → auto-equip → unequip → add back inventory.
    /// </summary>
    public void EquipFromSave(EquipmentItemData item)
    {
        if (item == null) return;

        equippedItems[item.equipSlot] = item;

        if (playerStats != null)
            playerStats.RecomputeStats();

        OnEquipmentChanged?.Invoke();
        Debug.Log($"[EquipmentManager] Load save: trang bị {item.itemName} vào ô {item.equipSlot}");
    }

    /// <summary>
    /// Xử lý hồi máu và mana mỗi giây dựa trên trang bị
    /// </summary>
    private void HandleRegeneration()
    {
        if (playerStats == null || playerStats.IsDead) return;

        // HP Regen (Quần)
        float totalHpRegenPct = 0f;
        if (equippedItems.TryGetValue(EquipmentSlot.Pants, out var pants) && pants != null)
            totalHpRegenPct += pants.hpRegenPctBonus;

        if (totalHpRegenPct > 0f && playerStats.currentHealth < playerStats.HP)
        {
            hpRegenTimer += Time.deltaTime;
            if (hpRegenTimer >= 30f) // Mỗi 30 giây
            {
                int healAmount = Mathf.Max(1, Mathf.RoundToInt(playerStats.HP * (totalHpRegenPct / 100f)));
                playerStats.Heal(healAmount);
                hpRegenTimer = 0f;
            }
        }

        // MP Regen (Giày)
        float totalMpRegenPct = 0f;
        if (equippedItems.TryGetValue(EquipmentSlot.Boots, out var boots) && boots != null)
            totalMpRegenPct += boots.mpRegenPctBonus;

        if (totalMpRegenPct > 0f && playerStats.currentMana < playerStats.MP)
        {
            mpRegenTimer += Time.deltaTime;
            if (mpRegenTimer >= 30f) // Mỗi 30 giây
            {
                int manaAmount = Mathf.Max(1, Mathf.RoundToInt(playerStats.MP * (totalMpRegenPct / 100f)));
                playerStats.IncreaseMana(manaAmount);
                mpRegenTimer = 0f;
            }
        }
    }
}
