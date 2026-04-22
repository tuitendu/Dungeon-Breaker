using UnityEngine;

public class Dinh_Nghia_O_Chua_Item : MonoBehaviour
{
    [Header("Cài đặt")]
    public Transform gridContent;   // Cái Khay (Grid_Content)
    public GameObject slotPrefab;   // Cái Khuôn (Slot_Item Prefab)

    [Header("Số lượng muốn tạo")]
    public int soLuongItem = 18;    // Muốn bao nhiêu ô thì nhập số này

    void Start()
    {
        TaoTuiDo();
    }

    public void TaoTuiDo()
    {
        // 1. Dọn sạch khay trước khi làm (xóa đồ cũ nếu có)
        foreach (Transform child in gridContent)
        {
            Destroy(child.gameObject);
        }

        // 2. Bắt đầu dập khuôn
        for (int i = 0; i < soLuongItem; i++)
        {
            // Lệnh Instantiate: Tạo ra bản sao từ Prefab, nhét vào gridContent
            Instantiate(slotPrefab, gridContent);
        }
    }
}
