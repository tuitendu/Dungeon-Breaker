using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nút "Vứt bỏ" trong Inventory — xóa item đang được chọn khỏi túi.
/// Gắn vào Button "BtnDiscard" trong InventoryPanel.
/// </summary>
public class UI_DiscardButton : MonoBehaviour
{
    [SerializeField] private Button discardButton;

    private void Awake()
    {
        if (discardButton == null) discardButton = GetComponent<Button>();
        if (discardButton != null)
            discardButton.onClick.AddListener(OnDiscardClicked);

        UpdateButtonState(null);
    }

    private void Start()
    {
        UI_InventorySlot.OnSlotSelected += UpdateButtonState;
    }

    private void OnDestroy()
    {
        UI_InventorySlot.OnSlotSelected -= UpdateButtonState;
        if (discardButton != null)
            discardButton.onClick.RemoveListener(OnDiscardClicked);
    }

    private void UpdateButtonState(UI_InventorySlot selectedSlot)
    {
        bool hasItem = selectedSlot != null && selectedSlot.GetItemData() != null;
        if (discardButton != null)
            discardButton.interactable = hasItem;
    }

    private void OnDiscardClicked()
    {
        UI_InventorySlot current = UI_InventorySlot.SelectedSlot;
        if (current == null || current.GetItemData() == null) return;

        if (InventoryManager.Instance != null)
        {
            // Xóa toàn bộ stack (-1 = xóa hết)
            InventoryManager.Instance.DiscardItem(current.GetSlotIndex(), -1);
            current.Deselect();
        }
    }
}
