using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý UI Shop với 6 slot mua đồ.
/// Gắn vào GameObject ShopPanel trong Canvas.
/// </summary>
public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Shop Panel")]
    public GameObject shopPanel;          // Panel cha bao toàn bộ shop

    [Header("6 Slot UI (kéo vào theo thứ tự)")]
    public ShopSlotUI[] slots = new ShopSlotUI[6];

    [Header("Hiển thị vàng")]
    public TextMeshProUGUI goldText;      // Text hiển thị vàng hiện tại (tuỳ chọn)

    private PlayerWallet _wallet;

    // ==================== Unity ====================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Tìm PlayerWallet
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _wallet = player.GetComponent<PlayerWallet>();

        // Đăng ký cập nhật text vàng
        if (_wallet != null)
            _wallet.OnGoldChanged += UpdateGoldText;

        CloseShop();
    }

    private void OnDestroy()
    {
        if (_wallet != null)
            _wallet.OnGoldChanged -= UpdateGoldText;
    }

    // ==================== Public API ====================

    /// <summary>Mở shop với dữ liệu từ NPC</summary>
    public void OpenShop(ShopItemData[] shopItems)
    {
        shopPanel.SetActive(true);

        // Cập nhật từng slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < shopItems.Length && shopItems[i] != null)
                slots[i].Setup(shopItems[i], this);
            else
                slots[i].SetEmpty();
        }

        UpdateGoldText(_wallet != null ? _wallet.CurrentGold : 0);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    /// <summary>Được gọi khi player bấm BUY trên 1 slot</summary>
    public void TryBuy(ShopItemData shopItem)
    {
        if (_wallet == null)
        {
            Debug.LogError("[ShopUI] Không tìm thấy PlayerWallet!");
            return;
        }

        if (!_wallet.HasEnoughGold(shopItem.price))
        {
            Debug.Log($"[ShopUI] Không đủ vàng! Cần {shopItem.price}, có {_wallet.CurrentGold}");
            // TODO: Có thể hiện thông báo "Không đủ vàng" lên UI
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[ShopUI] Không tìm thấy InventoryManager!");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(shopItem.item, shopItem.amount);
        if (!added)
        {
            Debug.Log("[ShopUI] Túi đồ đầy!");
            // TODO: Hiện thông báo "Túi đồ đầy"
            return;
        }

        // Trừ vàng
        _wallet.RemoveGold(shopItem.price);
        Debug.Log($"[ShopUI] Mua thành công: {shopItem.item.itemName} x{shopItem.amount} (-{shopItem.price} vàng)");
    }

    // ==================== Private ====================

    private void UpdateGoldText(int gold)
    {
        if (goldText != null)
            goldText.text = $"Vàng: {gold}";
    }
}
