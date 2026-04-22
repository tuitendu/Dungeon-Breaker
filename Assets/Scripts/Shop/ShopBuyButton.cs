using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào mỗi nút BUY trong Shop.
/// Icon của item set cứng trực tiếp trên Image trong Inspector.
/// Script này chỉ cần biết: item nào và giá bao nhiêu.
/// </summary>
public class ShopBuyButton : MonoBehaviour
{
    [Header("Item được bán")]
    public ItemData item;       // Kéo ItemData (ScriptableObject) vào đây
    public int price = 100;     // Giá vàng
    public int amount = 1;      // Số lượng nhận được khi mua

    private void Awake()
    {
        // Tự động tìm Button component và gắn sự kiện OnClick
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnBuyClicked);
    }

    private void OnBuyClicked()
    {
        // 1. Tìm PlayerWallet
        PlayerWallet wallet = FindFirstObjectByType<PlayerWallet>();
        if (wallet == null)
        {
            Debug.LogError("[ShopBuyButton] Không tìm thấy PlayerWallet!");
            return;
        }

        // 2. Kiểm tra đủ vàng chưa
        if (!wallet.HasEnoughGold(price))
        {
            Debug.Log($"[ShopBuyButton] Không đủ vàng! Cần {price}G, có {wallet.CurrentGold}G");
            // TODO: có thể hiện thông báo popup "Không đủ vàng!" lên màn hình
            return;
        }

        // 3. Kiểm tra túi đồ còn trống không
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[ShopBuyButton] Không tìm thấy InventoryManager!");
            return;
        }

        // allowAutoEquip = false: item mua từ shop luôn vào túi đồ,
        // không tự động mặc lên người (tránh mất item nếu EquipmentManager chưa set)
        bool added = InventoryManager.Instance.AddItem(item, amount, allowAutoEquip: false);
        if (!added)
        {
            Debug.Log("[ShopBuyButton] Túi đồ đầy!");
            // TODO: hiện thông báo "Túi đồ đầy"
            return;
        }

        // 4. Trừ vàng và log thành công
        wallet.RemoveGold(price);
        Debug.Log($"[ShopBuyButton] Mua thành công: {item.itemName} x{amount} (-{price}G)");

        // 5. Lưu game ngay lập tức để item không bị mất khi load lại
        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.SaveGame();
    }
}
