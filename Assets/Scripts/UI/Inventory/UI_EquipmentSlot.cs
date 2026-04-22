using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Quản lý UI cho 1 ô thiết bị (Đầu, Quần, Vũ khí...)
/// </summary>
public class UI_EquipmentSlot : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [Header("Slot Type")]
    [Tooltip("Ô này dành cho trang bị loại nào?")]
    public EquipmentSlot equipType;

    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image background;

    [Header("Visual Settings")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private Color filledColor = new Color(1f, 1f, 1f, 0.8f);

    private void Start()
    {
        // Tự tìm Image nếu quên kéo vào Inspector
        if (iconImage == null)
        {
            Transform t = transform.Find("Icon_Con") ?? transform.Find("Icon");
            if (t != null) iconImage = t.GetComponent<Image>();
        }
        if (background == null) background = GetComponent<Image>();

        // Lắng nghe sự kiện thay đổi trang bị
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
        }
    }

    // Cập nhật giao diện của ô này (Icon sáng lên nếu có đồ, tối đi nếu không có)
    private void RefreshUI()
    {
        if (EquipmentManager.Instance == null) return;

        EquipmentItemData equippedItem = EquipmentManager.Instance.GetEquippedItem(equipType);

        if (equippedItem == null)
        {
            // Không có đồ
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = Color.clear;
                iconImage.enabled = false;
                iconImage.gameObject.SetActive(false);
            }
            if (background != null) background.color = emptyColor;
        }
        else
        {
            // Có đồ
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = equippedItem.icon;
                iconImage.color = Color.white;
                iconImage.enabled = true;
            }
            if (background != null) background.color = filledColor;
        }
    }

    // Bấm vào để tháo đồ cất lại vô túi
    public void OnPointerClick(PointerEventData eventData)
    {
        // Chuột trái
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (EquipmentManager.Instance != null && !EquipmentManager.Instance.IsSlotEmpty(equipType))
            {
                // Gọi hàm tháo ra
                EquipmentManager.Instance.Unequip(equipType);
            }
        }
    }

    // ===== HỖ TRỢ KÉO THẢ TỪ TÚI ĐỒ VÀO Ô TRANG BỊ =====
    public void OnDrop(PointerEventData eventData)
    {
        // Lấy con trỏ đang được kéo (từ UI_InventorySlot)
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject != null)
        {
            UI_InventorySlot invSlot = draggedObject.GetComponent<UI_InventorySlot>();
            if (invSlot != null)
            {
                // Gọi mượn hàm UseItem để tự động kiểm tra xem có phải Equipment không và có đúng slot không
                // Thay vì check lằng nhằng, ta lấy data của slot đang kéo
                InventorySlot data = InventoryManager.Instance.GetSlot(invSlot.GetSlotIndex());
                if (data != null && !data.IsEmpty())
                {
                    if (data.Item is EquipmentItemData equipData)
                    {
                        // Kiểm tra xem món đồ có ĐÚNG loại với ô này không?
                        if (equipData.equipSlot == this.equipType)
                        {
                            // Mặc nó vào!
                            InventoryManager.Instance.UseItem(invSlot.GetSlotIndex());
                            Debug.Log($"Kéo thả thành công: {equipData.itemName} vào ô {this.equipType}");
                        }
                        else
                        {
                            Debug.LogWarning($"Sai loại trang bị! {equipData.itemName} ({equipData.equipSlot}) không thể bỏ vào ô {this.equipType}");
                        }
                    }
                }
            }
        }
    }
}
