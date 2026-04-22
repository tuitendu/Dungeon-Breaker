using UnityEngine;

/// <summary>
/// Gắn trên ROOT enemy (cùng GameObject với Animator).
/// Khi Animation Event gọi EnableHitbox(), script tính hướng thực tế
/// từ enemy đến player rồi đặt Hitbox vào đúng phía đó.
/// Hoạt động với mọi enemy dù có 2 hay 4 hướng animation.
/// </summary>
public class EnemyHitboxBridge : MonoBehaviour
{
    [Header("Hitbox Positioning")]
    [Tooltip("Kẻ khoảng cách từ tâm enemy đến tâm hitbox.")]
    public float hitboxOffset = 0.3f;

    [Tooltip(
        "Free   = hướng thực tế (360°, chính xác nhất)\n" +
        "Dir8   = snap 8 hướng (thêm chéo)\n" +
        "Dir4   = snap 4 hướng chính (trái/phải/trên/dướng)")]
    public DirectionMode directionMode = DirectionMode.Free;

    public enum DirectionMode { Free, Dir8, Dir4 }

    [Header("Debug Gizmos")]
    public bool showDirectionGizmo = true;

    private EnemyHitbox hitbox;
    private Animator anim;
    private Transform playerTransform;
    private Vector2 lastDirection = Vector2.right;

    void Awake()
    {
        anim   = GetComponent<Animator>();
        hitbox = GetComponentInChildren<EnemyHitbox>(includeInactive: true);

        if (hitbox == null)
            Debug.LogWarning($"[{name}] EnemyHitboxBridge: Không tìm thấy EnemyHitbox trong children!");
    }

    void Start()
    {
        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning($"[{name}] EnemyHitboxBridge: Không tìm thấy Player!");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gọi bởi Animation Event — frame BẮT ĐẦU vung đòn
    // ─────────────────────────────────────────────────────────────────────────
    public void EnableHitbox()
    {
        if (hitbox == null) return;

        // Tính hướng thực từ enemy → player
        Vector2 dir = GetDirectionToPlayer();
        lastDirection = dir;

        // Dịch Hitbox sang đúng phía
        hitbox.transform.localPosition = dir * hitboxOffset;

        hitbox.EnableHitbox();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gọi bởi Animation Event — frame KẾT THÚC đòn đánh
    // ─────────────────────────────────────────────────────────────────────────
    public void DisableHitbox() => hitbox?.DisableHitbox();

    // ───────────────────────────────────────────────────────────────────────────
    // Tính hướng từ enemy đến player theo DirectionMode
    // ───────────────────────────────────────────────────────────────────────────
    private Vector2 GetDirectionToPlayer()
    {
        Vector2 raw;
        if (playerTransform != null)
            raw = (playerTransform.position - transform.position).normalized;
        else
        {
            float mx = anim != null ? anim.GetFloat("MoveX") : 1f;
            float my = anim != null ? anim.GetFloat("MoveY") : 0f;
            raw = new Vector2(mx, my).normalized;
        }

        return directionMode switch
        {
            DirectionMode.Dir4 => SnapToNDir(raw, 4),
            DirectionMode.Dir8 => SnapToNDir(raw, 8),
            _                  => raw,   // Free: vector thực
        };
    }

    /// <summary>
    /// Snap vector về N hướng đều nhau (4 = trái/phải/trên/dướng, 8 = cả chéo).
    /// </summary>
    private Vector2 SnapToNDir(Vector2 dir, int n)
    {
        float angle  = Mathf.Atan2(dir.y, dir.x);          // radian [-π, +π]
        float step   = 360f / n * Mathf.Deg2Rad;            // bước góc
        float snapped = Mathf.Round(angle / step) * step;   // snap
        return new Vector2(Mathf.Cos(snapped), Mathf.Sin(snapped));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gizmos
    // ─────────────────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (!showDirectionGizmo || hitbox == null) return;

        // Đường vàng từ enemy → điểm đặt hitbox
        Vector3 hitboxWorldPos = transform.position + (Vector3)(lastDirection * hitboxOffset);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, hitboxWorldPos);

        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Gizmos.DrawSphere(hitboxWorldPos, 0.05f);
    }
}
