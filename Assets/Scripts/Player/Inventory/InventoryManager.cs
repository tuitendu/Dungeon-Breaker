using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // Singleton pattern
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 20;
    [SerializeField] private int maxStackSize = 99;

    [Header("Inventory Data")]
    [SerializeField] private InventorySlot[] slots;

    // Events
    public event Action OnInventoryChanged;

    // Properties
    public int InventorySize => inventorySize;
    public int MaxStackSize => maxStackSize;
    public InventorySlot[] Slots => slots;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate InventoryManager detected! Destroying {gameObject.name}");
            Destroy(this);
            return;
        }
        Instance = this;

        // Khởi tạo slots
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        slots = new InventorySlot[inventorySize];

        for (int i = 0; i < inventorySize; i++)
        {
            slots[i] = new InventorySlot();
        }

        Debug.Log($"Inventory khởi tạo: {inventorySize} slots");
    }

    /// <summary>
    /// Thêm item vào inventory (LOGIC CHÍNH)
    /// allowAutoEquip = false khi load save để tránh item bị tự động equip ra khỏi túi
    /// </summary>
    public bool AddItem(ItemData item, int amount, bool allowAutoEquip = true)
    {
        if (item == null)
        {
            Debug.LogError("AddItem: item is null!");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"AddItem: amount phải > 0 (received: {amount})");
            return false;
        }

        // ===== KIỂM TRA TỰ ĐỘNG MẶC TRANG BỊ =====
        if (allowAutoEquip && item is EquipmentItemData equipData && EquipmentManager.Instance != null)
        {
            if (EquipmentManager.Instance.IsSlotEmpty(equipData.equipSlot))
            {
                EquipmentManager.Instance.Equip(equipData);
                amount--;
                if (amount <= 0) return true;
            }
        }

        int remainingAmount = amount;

        // ===== BƯỚC 1: Nếu item stackable, tìm slot đã có item này =====
        if (item.stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].CanStackWith(item, maxStackSize))
                {
                    int added = slots[i].AddAmount(remainingAmount, maxStackSize);
                    remainingAmount -= added;

                    Debug.Log($"Stack vào slot {i}: +{added} {item.itemName} (Total: {slots[i].Amount})");

                    if (remainingAmount <= 0)
                    {
                        if (allowAutoEquip)
                        {
                            PlayerAudio pa = FindFirstObjectByType<PlayerAudio>();
                            if (pa != null) pa.PlayPickup();
                        }
                        NotifyInventoryChanged();
                        return true; // Đã thêm hết
                    }
                }
            }
        }

        // ===== BƯỚC 2: Tìm slot trống để thêm phần còn lại =====
        while (remainingAmount > 0)
        {
            int emptySlotIndex = FindEmptySlot();

            if (emptySlotIndex == -1)
            {
                Debug.LogWarning($"Inventory đầy! Không thể thêm {remainingAmount}x {item.itemName}");
                NotifyInventoryChanged();
                return false; // Inventory đầy
            }

            // Thêm vào slot trống
            int amountToAdd = item.stackable 
                ? Mathf.Min(remainingAmount, maxStackSize)
                : 1; // Non-stackable chỉ thêm 1

            slots[emptySlotIndex].SetItem(item, amountToAdd);
            remainingAmount -= amountToAdd;

            Debug.Log($"Thêm vào slot {emptySlotIndex}: {amountToAdd}x {item.itemName}");

            // Nếu item non-stackable và còn dư, tiếp tục loop
            if (!item.stackable && remainingAmount > 0)
            {
                continue;
            }
        }

        // Tình huống nhặt đồ thành công trong game (không phải load save)
        if (allowAutoEquip)
        {
            PlayerAudio pa = FindFirstObjectByType<PlayerAudio>();
            if (pa != null) pa.PlayPickup();
        }

        NotifyInventoryChanged();
        return true;
    }

    /// <summary>
    /// Hoán đổi hoặc xếp chồng items giữa 2 ô (dùng cho kéo thả)
    /// </summary>
    public bool SwapSlots(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= slots.Length || toIndex < 0 || toIndex >= slots.Length)
        {
            Debug.LogWarning($"SwapSlots: Invalid indices! from={fromIndex}, to={toIndex}");
            return false;
        }

        if (fromIndex == toIndex) return false; // Cùng ô thì không làm gì

        InventorySlot fromSlot = slots[fromIndex];
        InventorySlot toSlot = slots[toIndex];

        if (fromSlot.IsEmpty()) return false; // Ô nguồn trống

        // Trường hợp 1: Di chuyển vào ô trống
        if (toSlot.IsEmpty())
        {
            toSlot.SetItem(fromSlot.Item, fromSlot.Amount);
            fromSlot.Clear();
            Debug.Log($"Moved {toSlot.Item.itemName} from slot {fromIndex} to {toIndex}");
            NotifyInventoryChanged();
            return true;
        }

        // Trường hợp 2: Cùng loại item, thử xếp chồng
        if (fromSlot.Item == toSlot.Item && fromSlot.Item.stackable)
        {
            int spaceInTo = maxStackSize - toSlot.Amount;
            if (spaceInTo > 0)
            {
                int amountToMove = Mathf.Min(fromSlot.Amount, spaceInTo);
                toSlot.AddAmount(amountToMove, maxStackSize);
                fromSlot.RemoveAmount(amountToMove);
                Debug.Log($"Stacked {amountToMove}x {fromSlot.Item.itemName}");
                NotifyInventoryChanged();
                return true;
            }
        }

        // Trường hợp 3: Khác loại item, hoán đổi vị trí
        ItemData tempItem = fromSlot.Item;
        int tempAmount = fromSlot.Amount;
        fromSlot.SetItem(toSlot.Item, toSlot.Amount);
        toSlot.SetItem(tempItem, tempAmount);
        Debug.Log($"Swapped slots {fromIndex} ↔ {toIndex}");
        NotifyInventoryChanged();
        return true;
    }

    /// <summary>
    /// Sử dụng item tại slot index
    /// </summary>
    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogWarning($"UseItem: Invalid slot index {slotIndex}");
            return false;
        }

        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty())
        {
            Debug.Log("UseItem: Slot is empty");
            return false;
        }

        ItemData item = slot.Item;

        // Xử lý theo loại item
        if (item is ConsumableItemData consumable)
        {
            // ── Kiểm tra HP/MP đã đầy chưa ────────────────────────────
            PlayerStats player = FindFirstObjectByType<PlayerStats>();
            if (player != null)
            {
                bool hpFull  = consumable.healthRestore > 0 && player.currentHealth >= player.HP;
                bool mpFull  = consumable.manaRestore   > 0 && player.currentMana   >= player.MP;
                bool onlyHp  = consumable.healthRestore > 0 && consumable.manaRestore <= 0;
                bool onlyMp  = consumable.manaRestore   > 0 && consumable.healthRestore <= 0;
                bool bothHeal = consumable.healthRestore > 0 && consumable.manaRestore > 0;

                // Chặn nếu item chỉ hồi HP và HP đã đầy
                if (onlyHp && hpFull)
                {
                    Debug.Log("[Inventory] HP đã đầy, không thể dùng item hồi HP!");
                    return false;
                }
                // Chặn nếu item chỉ hồi MP và MP đã đầy
                if (onlyMp && mpFull)
                {
                    Debug.Log("[Inventory] MP đã đầy, không thể dùng item hồi MP!");
                    return false;
                }
                // Chặn nếu item hồi cả 2 và cả 2 đều đầy
                if (bothHeal && hpFull && mpFull)
                {
                    Debug.Log("[Inventory] HP và MP đều đầy, không thể dùng item!");
                    return false;
                }
            }

            UseConsumable(consumable);
            RemoveItem(item, 1); // Tiêu hao 1 item
            Debug.Log($"Đã sử dụng {item.itemName}");

            // <--- GỌI ÂM THANH UỐNG MÁU/MANA --->
            PlayerAudio pa = FindFirstObjectByType<PlayerAudio>();
            if (pa != null) pa.PlayHeal();

            return true;
        }
        else if (item is EquipmentItemData equipment)
        {
            if (EquipmentManager.Instance != null)
            {
                // Mặc trang bị vào
                EquipmentManager.Instance.Equip(equipment);
                
                // Bỏ trang bị này khỏi túi đồ (vì nó đã nằm trên người)
                // Lưu ý: Nếu ô đó có đồ cũ, EquipmentManager đã tự lo việc trả đồ cũ về túi.
                RemoveItem(item, 1);
                
                Debug.Log($"Đã mặc trang bị {equipment.itemName} từ túi đồ");

                // <--- GỌI ÂM THANH MẶC TRANG BỊ --->
                PlayerAudio pa = FindFirstObjectByType<PlayerAudio>();
                if (pa != null) pa.PlayEquip();

                return true;
            }
            return false;
        }
        else
        {
            Debug.LogWarning($"Item {item.itemName} ({item.itemType}) không thể sử dụng");
            return false;
        }
    }

    /// <summary>
    /// Vứt bỏ item tại slot index (xóa khỏi inventory).
    /// amount = -1 → xóa toàn bộ stack.
    /// </summary>
    public bool DiscardItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;

        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty()) return false;

        int toRemove = (amount < 0) ? slot.Amount : Mathf.Min(amount, slot.Amount);
        slot.RemoveAmount(toRemove);

        Debug.Log($"[Inventory] Đã vứt {toRemove}x {slot.Item?.itemName ?? "?"}" );
        NotifyInventoryChanged();
        return true;
    }

    /// <summary>
    /// Áp dụng hiệu ứng consumable cho player
    /// </summary>
    private void UseConsumable(ConsumableItemData item)
    {
        // Tìm PlayerStats
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player == null)
        {
            Debug.LogError("Không tìm thấy PlayerStats! Không thể dùng item.");
            return;
        }

        // Hồi HP
        if (item.healthRestore > 0)
        {
            player.Heal(item.healthRestore);
            Debug.Log($"Hồi {item.healthRestore} HP");
        }

        // Hồi MP
        if (item.manaRestore > 0)
        {
            player.IncreaseMana(item.manaRestore);
            Debug.Log($"Hồi {item.manaRestore} MP");
        }

        // TODO: Phase 3 - Buff effects
        if (item.buffDuration > 0)
        {
            Debug.Log($"Buff effects sẽ có trong Phase 3 (duration: {item.buffDuration}s)");
        }
    }

    /// <summary>
    /// Xóa item khỏi inventory
    /// </summary>
    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null) return false;
        if (amount <= 0) return false;

        // Kiểm tra có đủ item không
        if (!HasItem(item, amount))
        {
            Debug.LogWarning($"Không đủ {item.itemName} để xóa! Cần: {amount}");
            return false;
        }

        int remainingAmount = amount;

        // Duyệt qua các slot và xóa
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty()) continue;
            if (slots[i].Item != item) continue;

            int amountInSlot = slots[i].Amount;
            int toRemove = Mathf.Min(remainingAmount, amountInSlot);

            slots[i].RemoveAmount(toRemove);
            remainingAmount -= toRemove;

            Debug.Log($"Removed {toRemove}x {item.itemName} from slot {i}");

            if (remainingAmount <= 0)
            {
                NotifyInventoryChanged();
                return true;
            }
        }

        NotifyInventoryChanged();
        return true;
    }

    /// <summary>
    /// Kiểm tra có item không
    /// </summary>
    public bool HasItem(ItemData item, int amount)
    {
        if (item == null) return false;

        int totalCount = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty()) continue;
            if (slots[i].Item != item) continue;

            totalCount += slots[i].Amount;

            if (totalCount >= amount)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Đếm tổng số lượng item trong inventory
    /// </summary>
    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;

        int count = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty()) continue;
            if (slots[i].Item == item)
            {
                count += slots[i].Amount;
            }
        }

        return count;
    }



    /// <summary>
    /// Số lượng slots
    /// </summary>
    public int SlotCount => slots.Length;

    /// <summary>
    /// Tìm slot trống đầu tiên
    /// </summary>
    private int FindEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
                return i;
        }
        return -1; // Không có slot trống
    }

    /// <summary>
    /// Check inventory có đầy không
    /// </summary>
    public bool IsFull()
    {
        return FindEmptySlot() == -1;
    }

    /// <summary>
    /// Xóa toàn bộ inventory (cheat/debug)
    /// </summary>
    public void ClearInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Clear();
        }

        Debug.Log("Inventory cleared");
        NotifyInventoryChanged();
    }

    /// <summary>
    /// Get slot tại index (để UI dùng)
    /// </summary>
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
        {
            Debug.LogError($"Invalid slot index: {index}");
            return null;
        }

        return slots[index];
    }

    private void NotifyInventoryChanged()
    {
        Debug.Log($"[InventoryManager] NotifyInventoryChanged fired! Subscribers: {OnInventoryChanged?.GetInvocationList().Length ?? 0}");
        OnInventoryChanged?.Invoke();
    }

    // ===== DEBUG =====
    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        Debug.Log("===== INVENTORY =====");
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty())
            {
                Debug.Log($"Slot {i}: {slots[i]}");
            }
        }
        Debug.Log($"Empty slots: {inventorySize - GetUsedSlotCount()}/{inventorySize}");
    }

    private int GetUsedSlotCount()
    {
        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty())
                count++;
        }
        return count;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
