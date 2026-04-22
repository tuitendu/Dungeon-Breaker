using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Nếu tích chọn, rương sẽ tự mở khi Player chạm vào. Nếu không, phải bấm nút tương tác.")]
    [SerializeField] private bool openOnApproach = false;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Animation")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Loot Content")]
    [SerializeField] private LootItem[] lootItems;

    [System.Serializable]
    public struct LootItem
    {
        public ItemData itemData;
        public int amount;
    }

    private bool isOpened = false;
    private bool playerInRange = false;

    private void Awake()
    {
        if (chestAnimator == null)
        {
            chestAnimator = GetComponent<Animator>();
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Nhấn nút mở rương khi ở gần
        if (!isOpened && playerInRange && !openOnApproach)
        {
            if (Input.GetKeyDown(interactKey))
            {
                OpenChest();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Tự động mở khi chạm vào
            if (!isOpened && openOnApproach)
            {
                OpenChest();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void OpenChest()
    {
        if (isOpened) return;

        isOpened = true;

        // Chạy Animation
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger(openAnimationTrigger);
        }

        // Chạy âm thanh mở rương
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Thêm các item vào túi đồ
        GiveItemsToPlayer();
    }

    private void GiveItemsToPlayer()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("Chưa tìm thấy InventoryManager.Instance trong scene!");
            return;
        }

        bool inventoryWasFull = false;

        foreach (var loot in lootItems)
        {
            if (loot.itemData != null && loot.amount > 0)
            {
                bool added = InventoryManager.Instance.AddItem(loot.itemData, loot.amount, false);
                
                if (!added)
                {
                    inventoryWasFull = true;
                    Debug.Log($"Túi đồ đã đầy, không thể thêm {loot.amount}x {loot.itemData.itemName}");
                    // TODO: Có thể spawn item rớt ra ngoài đất nếu túi đầy (dùng Item_Roi.cs)
                }
                else
                {
                    Debug.Log($"Rương: Đã nhận {loot.amount}x {loot.itemData.itemName}");
                }
            }
        }

        if (inventoryWasFull)
        {
            // Báo hiệu lên màn hình là túi đồ đã đầy (UI)
            // Ví dụ: UIManager.Instance.ShowMessage("Túi đồ đầy!");
        }
    }
}
