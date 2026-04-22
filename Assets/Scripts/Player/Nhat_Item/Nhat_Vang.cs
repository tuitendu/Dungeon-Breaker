using UnityEngine;

public class Nhat_Vang : MonoBehaviour
{
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
    }

    private void EnablePickup() => canPickup = true;

    private void StopPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; // Dừng hẳn physics
        }
        canPickup = true; // Cho phép pickup ngay khi dừng
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup) return;
        if (!other.CompareTag("Player")) return;

        // Thêm vàng vào ví người chơi
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.AddGold(amount);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("PlayerWallet.Instance is null! Không thể nhặt vàng.");
        }
    }
}
