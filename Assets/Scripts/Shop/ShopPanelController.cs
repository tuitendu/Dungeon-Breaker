using UnityEngine;

/// <summary>
/// Gắn vào UI Panel (ví dụ: ShopPanel, InventoryPanel...).
/// Tự động dừng thời gian (Time.timeScale = 0) khi Panel hiển thị,
/// và phục hồi lại thời gian (Time.timeScale = 1) khi tắt Panel.
/// </summary>
public class ShopPanelController : MonoBehaviour
{
    private void OnEnable()
    {
        // Khi ShopPanel hiện lên -> Dừng thời gian
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        // Khi ShopPanel tắt đi -> Trả lại thời gian bình thường
        Time.timeScale = 1f;
    }
}
