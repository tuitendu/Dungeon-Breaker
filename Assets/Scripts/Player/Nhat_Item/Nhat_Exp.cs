using UnityEngine;

public class Nhat_Exp : MonoBehaviour
{
    public int expAmount = 5;
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

        // Thêm EXP cho player
        if (PlayerLevel.Instance != null)
        {
            PlayerLevel.Instance.AddExp(expAmount);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("PlayerLevel.Instance is null! Không thể nhặt EXP.");
        }
    }
}
