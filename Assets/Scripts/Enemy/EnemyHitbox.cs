using UnityEngine;

/// <summary>
/// Gắn vào child GameObject "Hitbox" của enemy prefab.
/// Bật/tắt bằng Animation Event trong clip Attack.
/// 
/// Setup trong prefab:
///   Enemy (root)
///   └── Hitbox  ← GameObject này
///         - Collider2D (IsTrigger = true, disable mặc định)
///         - EnemyHitbox (script này)
/// </summary>
public class EnemyHitbox : MonoBehaviour
{
    [Tooltip("Tham chiếu tới AttackBehaviour SO của enemy cha (kéo vào từ Inspector).")]
    public AttackBehaviour attackStrategy;

    // Gán tự động từ EnemyAttackController khi Start
    [HideInInspector] public EnemyStats ownerStats;
    [HideInInspector] public GameObject ownerObject;

    private Collider2D hitCol;

    void Awake()
    {
        hitCol = GetComponent<Collider2D>();
        // Hitbox tắt mặc định — chỉ bật khi Animation Event gọi
        if (hitCol != null) hitCol.enabled = false;
    }

    // ─── Gọi từ Animation Event (frame BẮT ĐẦU vung đòn) ───────────────────
    public void EnableHitbox()
    {
        if (hitCol != null) hitCol.enabled = true;
    }

    // ─── Gọi từ Animation Event (frame KẾT THÚC đòn đánh) ──────────────────
    public void DisableHitbox()
    {
        if (hitCol != null) hitCol.enabled = false;
    }

    // ─── Phát hiện va chạm với Player ───────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (attackStrategy != null && ownerObject != null)
        {
            // Dùng lại toàn bộ logic trong AttackBehaviour SO (damage, knockback, lifesteal…)
            attackStrategy.ExecuteAttack(ownerObject, other.gameObject);
        }
        else if (ownerStats != null)
        {
            // Fallback: gây damage thẳng nếu chưa gán SO
            var playerStats = other.GetComponent<PlayerStats>();
            playerStats?.TakeDamage(ownerStats.Attack);
        }

        // Tắt ngay sau khi hit để tránh damage nhiều lần trong 1 đòn
        DisableHitbox();
    }

    // ─── Hiển thị hitbox trong Scene View ────────────────────────────────────
    void OnDrawGizmos()
    {
        if (hitCol == null) hitCol = GetComponent<Collider2D>();
        if (hitCol == null) return;

        // Đỏ khi đang active, xám mờ khi disabled
        Gizmos.color = hitCol.enabled
            ? new Color(1f, 0f, 0f, 0.5f)
            : new Color(1f, 1f, 1f, 0.15f);

        if (hitCol is CircleCollider2D circle)
        {
            Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
        }
        else if (hitCol is BoxCollider2D box)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}
