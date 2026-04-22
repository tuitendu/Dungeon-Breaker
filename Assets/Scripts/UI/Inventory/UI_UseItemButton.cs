using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Nút chuyên dụng để Dùng (Potion) hoặc Mặc (Trang bị) cho Mobile.
/// Yêu cầu người chơi phải CHỌN 1 ô trong túi đồ trước.
/// </summary>
public class UI_UseItemButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button useButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private void Awake()
    {
        if (useButton == null) useButton = GetComponent<Button>();
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        // Gắn sự kiện click
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
        }

        // Mặc định là tắt (ví chưa chọn gì)
        UpdateButtonState(null);
    }

    private void Start()
    {
        // Lắng nghe khi người chơi bấm chọn 1 ô trong túi
        UI_InventorySlot.OnSlotSelected += UpdateButtonState;
    }

    private void OnDestroy()
    {
        UI_InventorySlot.OnSlotSelected -= UpdateButtonState;
        
        if (useButton != null)
        {
            useButton.onClick.RemoveListener(OnUseButtonClicked);
        }
    }

    private void UpdateButtonState(UI_InventorySlot selectedSlot)
    {
        if (selectedSlot == null || selectedSlot.GetItemData() == null)
        {
            useButton.interactable = false;
            if (buttonText != null) buttonText.text = "Sử dụng";
            return;
        }

        useButton.interactable = true;

        ItemData item = selectedSlot.GetItemData();
        if (item is EquipmentItemData)
        {
            if (buttonText != null) buttonText.text = "Trang bị"; // Equip
        }
        else if (item is ConsumableItemData)
        {
            if (buttonText != null) buttonText.text = "Dùng";     // Use
        }
        else
        {
            if (buttonText != null) buttonText.text = "Sử dụng";
        }
    }

    private void OnUseButtonClicked()
    {
        UI_InventorySlot currentSelected = UI_InventorySlot.SelectedSlot;

        if (currentSelected != null && currentSelected.GetItemData() != null)
        {
            if (InventoryManager.Instance != null)
            {
                // Thực hiện Dùng/Trang bị
                InventoryManager.Instance.UseItem(currentSelected.GetSlotIndex());

                // Dùng xong thì tự động bỏ chọn để nút trở về trạng thái mờ đi (Disable)
                currentSelected.Deselect();
            }
        }
    }
}
