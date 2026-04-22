using UnityEngine;

public class Item_Roi : MonoBehaviour
{
    public ItemData item;
    public int amount = 1;
    public float pickupDelay = 0.05f; // Delay ngắn để tránh pickup ngay khi spawn
    public float stopDelay = 0.5f; // Thời gian để tự động dừng physics

    private bool canPickup;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Invoke(nameof(EnablePickup), pickupDelay);
        
        // Tự động dừng physics sau khi rơi
        if (rb != null)
        {
            Invoke(nameof(StopPhysics), stopDelay);
        }
        
        // Gán icon từ ItemData vào sprite (QUAN TRỌNG!)
        if (item != null && item.icon != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            UnityEngine.UI.Image imageComponent = GetComponent<UnityEngine.UI.Image>();
            
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = item.icon;
                Debug.Log($"[Item_Roi] Set SpriteRenderer icon: {item.icon.name}");
            }
            else if (imageComponent != null)
            {
                imageComponent.sprite = item.icon;
                Debug.Log($"[Item_Roi] Set Image icon: {item.icon.name}");
            }
            else
            {
                Debug.LogWarning("[Item_Roi] No SpriteRenderer or Image component found!");
            }
        }
        
        Debug.Log($"[Item_Roi] Spawned: {(item != null ? item.itemName : "NULL ITEM")}");
    }

    private void EnablePickup()
    {
        canPickup = true;
        Debug.Log("[Item_Roi] Pickup enabled!");
    }

    private void StopPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; // Dừng hẳn physics
        }
        canPickup = true; // Cho phép pickup ngay khi dừng
        Debug.Log("[Item_Roi] Physics stopped!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Item_Roi] TRIGGER! Name: {other.name}, Tag: '{other.tag}', canPickup: {canPickup}");
        
        if (!canPickup)
        {
            Debug.LogWarning("[Item_Roi] Cannot pickup - still in delay");
            return;
        }
        
        if (!other.CompareTag("Player"))
        {
            Debug.LogWarning($"[Item_Roi] Not Player! Tag is: '{other.tag}'");
            return;
        }

        Debug.Log("[Item_Roi] Player detected! Attempting pickup...");

        // Add item to inventory
        if (InventoryManager.Instance != null)
        {
            Debug.Log($"[Item_Roi] Adding {amount}x {item.itemName}...");
            bool success = InventoryManager.Instance.AddItem(item, amount);

            if (success)
            {
                Debug.Log($"[Item_Roi] SUCCESS! Picked up {amount}x {item.itemName}");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[Item_Roi] FAILED! Inventory full!");
            }
        }
        else
        {
            Debug.LogError("[Item_Roi] InventoryManager.Instance is NULL!");
        }
    }
}
