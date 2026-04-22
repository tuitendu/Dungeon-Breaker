using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Đại diện cho 1 slot (ô) trong ShopUI.
/// Gắn vào mỗi SlotItem GameObject trong Shop Panel.
/// </summary>
public class ShopSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;          // Ảnh icon item
    public TextMeshProUGUI itemNameText;  // Tên item
    public TextMeshProUGUI priceText;     // Giá vàng
    public TextMeshProUGUI amountText;    // Số lượng nhận được (x1, x2...)
    public Button buyButton;        // Nút BUY

    private ShopItemData _data;
    private ShopUI _shopUI;

    private void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);
    }

    /// <summary>Cài đặt dữ liệu cho slot này</summary>
    public void Setup(ShopItemData data, ShopUI shopUI)
    {
        _data   = data;
        _shopUI = shopUI;

        gameObject.SetActive(true);

        // Hiển thị icon
        if (itemIcon != null)
            itemIcon.sprite = data.item != null ? data.item.icon : null;

        // Hiển thị tên item
        if (itemNameText != null)
            itemNameText.text = data.item != null ? data.item.itemName : "???";

        // Hiển thị giá
        if (priceText != null)
            priceText.text = $"{data.price} G";

        // Hiển thị số lượng
        if (amountText != null)
            amountText.text = data.amount > 1 ? $"x{data.amount}" : "";

        // Kích hoạt nút BUY
        if (buyButton != null)
            buyButton.interactable = true;
    }

    /// <summary>Làm trống slot (không có item)</summary>
    public void SetEmpty()
    {
        _data = null;
        gameObject.SetActive(false);
    }

    private void OnBuyClicked()
    {
        if (_data == null || _shopUI == null) return;
        _shopUI.TryBuy(_data);
    }
}
