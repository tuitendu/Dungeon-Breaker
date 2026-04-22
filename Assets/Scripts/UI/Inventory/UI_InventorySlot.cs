using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Component UI cho một ô chứa đồ trong inventory
/// </summary>
public class UI_InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Text amountTextLegacy; // Fallback
    [SerializeField] private Image background;

    [Header("Visual Settings")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    [SerializeField] private Color filledColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

    private InventorySlot slotData;
    private int slotIndex = -1;

    // ===== MOBILE SUPPORT: LƯU SLOT ĐANG CHỌN =====
    public static UI_InventorySlot SelectedSlot { get; private set; }
    public static event System.Action<UI_InventorySlot> OnSlotSelected;

    private void Awake()
    {
        // Tự xác định index từ vị trí trong parent
        slotIndex = transform.GetSiblingIndex();

        // Runtime fallback: tự tìm nếu chưa gán trong Inspector
        if (iconImage == null)
        {
            Transform t = transform.Find("Icon_Con") ?? transform.Find("Icon");
            if (t != null) iconImage = t.GetComponent<Image>();
        }
        if (background == null)
            background = GetComponent<Image>();
    }

    private void Start()
    {
        // Mỗi slot tự subscribe vào event — không phụ thuộc UI_InventoryPanel
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += AutoRefresh;
        }
        // Khởi tạo hiển thị ngay lập tức
        AutoRefresh();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= AutoRefresh;
    }

    private void AutoRefresh()
    {
        if (slotIndex < 0 || InventoryManager.Instance == null) return;
        InventorySlot slot = InventoryManager.Instance.GetSlot(slotIndex);
        UpdateSlot(slot, slotIndex);
    }

    /// <summary>
    /// Update slot UI với data mới
    /// </summary>
    public void UpdateSlot(InventorySlot slot, int index)
    {
        slotData = slot;
        slotIndex = index;

        if (slot == null || slot.IsEmpty())
        {
            ShowEmpty();
        }
        else
        {
            ShowItem(slot.Item, slot.Amount);
        }
    }

    private void ShowItem(ItemData item, int amount)
    {
        if (item == null) return;

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
            iconImage.enabled = (item.icon != null);
        }
        else
        {
            Debug.LogError("[UI_InventorySlot] iconImage is NULL!");
        }

        if (item.stackable && amount > 1)
            SetAmountText(amount.ToString());
        else
            SetAmountText("");

        if (background != null)
            background.color = filledColor;
    }

    private void ShowEmpty()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconImage.enabled = false;
            iconImage.gameObject.SetActive(false);
        }

        SetAmountText("");

        if (background != null)
            background.color = emptyColor;
    }

    private void SetAmountText(string text)
    {
        if (amountText != null)
        {
            amountText.text = text;
        }

        if (amountTextLegacy != null)
        {
            amountTextLegacy.text = text;
        }
    }

    // ===== HOVER TOOLTIP (DI CHUỘT) =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotData != null && !slotData.IsEmpty())
        {
            // Tô sáng
            if (background != null)
            {
                background.color = highlightColor;
            }

            // Hiển thị tooltip
            ShowTooltip(slotData.Item);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Bỏ tô sáng (chỉ khi không phải là ô đang được chọn)
        if (background != null && SelectedSlot != this)
        {
            if (slotData != null && !slotData.IsEmpty())
            {
                background.color = filledColor;
            }
            else
            {
                background.color = emptyColor;
            }
        }

        // Ẩn tooltip
        HideTooltip();
    }

    private void ShowTooltip(ItemData item)
    {
        // TODO: Triển khai hệ thống tooltip
        // Tạm thời chỉ debug log
        Debug.Log($"Tooltip: {item.itemName} (ID: {item.id})");
    }

    private void HideTooltip()
    {
        // TODO: Ẩn UI tooltip
    }

    // ===== CLICK ĐỂ DÙNG / TRANG BỊ / CHỌN =====
    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotData == null || slotData.IsEmpty()) return;

        // Chuột phải: Dùng ngay lập tức (dành cho PC)
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.UseItem(slotIndex);
                Deselect(); // Dùng xong thì bỏ chọn
            }
        }
        // Chuột trái (Tab trên Mobile): Chọn item
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            SelectThisSlot();
        }
    }

    private void SelectThisSlot()
    {
        // Bỏ chọn slot cũ
        if (SelectedSlot != null && SelectedSlot != this)
        {
            SelectedSlot.Deselect();
        }

        // Chọn slot này
        SelectedSlot = this;
        if (background != null) background.color = highlightColor;

        // Gửi event để UI (như nút Use) biết
        OnSlotSelected?.Invoke(this);
    }

    public void Deselect()
    {
        if (SelectedSlot == this) SelectedSlot = null;

        if (background != null)
        {
            background.color = (slotData != null && !slotData.IsEmpty()) ? filledColor : emptyColor;
        }
        
        // Cập nhật UI nút Use (bỏ chọn)
        OnSlotSelected?.Invoke(null);
    }

    public int GetSlotIndex() => slotIndex;
    public ItemData GetItemData() => slotData?.Item;

    #region Kéo Thả (Drag and Drop)
    private static GameObject draggedIcon; // Icon đang được kéo
    private static UI_InventorySlot draggedSlot; // Slot đang được kéo

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotData == null || slotData.IsEmpty()) return;

        draggedIcon = new GameObject("DragIcon");
        draggedIcon.transform.SetParent(transform.root);
        draggedIcon.transform.SetAsLastSibling();

        Image img = draggedIcon.AddComponent<Image>();
        img.sprite = iconImage.sprite;
        img.raycastTarget = false;
        img.SetNativeSize();

        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 0.5f;
            iconImage.color = c;
        }

        draggedSlot = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            draggedIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }

        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
        }

        draggedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SwapSlots(draggedSlot.slotIndex, this.slotIndex);
        }
    }
    #endregion

    // ===== TỰ ĐỘNG TÌM COMPONENTS KHI EDIT PREFAB =====
    private void OnValidate()
    {
        // Tự động tìm components
        if (iconImage == null)
        {
            // Tìm cả "Icon" và "Icon_Con"
            Transform iconTransform = transform.Find("Icon_Con") ?? transform.Find("Icon");
            if (iconTransform != null)
                iconImage = iconTransform.GetComponent<Image>();
        }

        if (amountText == null)
        {
            Transform amountTransform = transform.Find("Amount");
            if (amountTransform != null)
                amountText = amountTransform.GetComponent<TextMeshProUGUI>();
        }

        if (background == null)
        {
            background = GetComponent<Image>();
        }
    }
}
