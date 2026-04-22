using UnityEngine;

/// <summary>
/// Gắn vào GameObject của NPC thợ rèn.
/// Yêu cầu: CircleCollider2D (Is Trigger = true) trên cùng GameObject.
///
/// Chức năng:
/// 1. Phát hiện player vào/ra vùng trigger
/// 2. Đăng ký/huỷ hook với Player_Combat để mở shop thay vì đánh
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class NPC_Shop : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopPanel;   // Kéo ShopPanel (Canvas > ShopPanel) vào đây

    [Header("Trigger Settings")]
    public float triggerRadius = 2f;

    // ----- Runtime -----
    private Player_Combat _playerCombat;

    // ==================== Unity ====================

    private void Start()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = triggerRadius;

        // Đảm bảo shop đóng lúc đầu
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerCombat = other.GetComponent<Player_Combat>();

        if (_playerCombat != null)
            _playerCombat.RegisterNearbyShop(this);

        Debug.Log("[NPC_Shop] Player vào vùng shop");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_playerCombat != null)
            _playerCombat.UnregisterNearbyShop(this);

        // Đóng shop khi player rời đi
        CloseShop();

        _playerCombat = null;

        Debug.Log("[NPC_Shop] Player rời vùng shop");
    }

    // ==================== Public ====================

    /// <summary>Mở ShopPanel — gọi bởi Player_Combat khi bấm BtnBasic gần NPC</summary>
    public void OpenShop()
    {
        if (shopPanel == null)
        {
            // Tự động tìm ShopPanel trong Scene (ngay cả khi nó đang bị Tắt)
            ShopPanelController panel = Resources.FindObjectsOfTypeAll<ShopPanelController>()[0];
            if (panel != null) shopPanel = panel.gameObject;
        }

        if (shopPanel != null)
            shopPanel.SetActive(true);
    }

    /// <summary>Đóng ShopPanel</summary>
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}
