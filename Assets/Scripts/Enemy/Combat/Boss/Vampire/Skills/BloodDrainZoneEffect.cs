using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Skill 2 — Vùng tornado (Tornado Zone).
/// Gắn lên prefab TornadoZone (có SpriteRenderer + Animator dùng tornado sprites).
/// Player đứng trong vùng → mất máu theo tick, rời ra → dừng.
/// Prefab tự Destroy sau zoneDuration (set bởi Skill2_BloodDrainZone khi Instantiate).
/// </summary>
public class BloodDrainZoneEffect : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Sát thương mỗi tick khi player đứng trong vùng.")]
    public int damagePerTick = 5;

    [Tooltip("Giây giữa 2 tick damage liên tiếp.")]
    public float tickInterval = 0.5f;

    // Theo dõi thời điểm hit cuối của từng collider để chống spam damage
    private readonly Dictionary<Collider2D, float> hitTimers = new();

    // ─── Damage khi player đứng trong trigger ────────────────────────────────
    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        float now = Time.time;
        if (hitTimers.TryGetValue(other, out float lastHit) && now - lastHit < tickInterval)
            return;

        PlayerStats ps = other.GetComponent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(damagePerTick);
            Debug.Log($"[BloodDrainZone] Hút máu player: {damagePerTick} dmg");
        }

        hitTimers[other] = now;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Xóa timer khi player rời khỏi vùng
        hitTimers.Remove(other);
    }

    // ─── Tạo sprite placeholder màu đỏ hình tròn (dùng khi chưa có asset) ──
    public static Sprite CreatePlaceholderSprite(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            if (dist <= radius)
                tex.SetPixel(x, y, color);
            else
                tex.SetPixel(x, y, Color.clear);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 32f);
    }

    // ─── Gizmos ──────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
            Gizmos.DrawSphere(transform.position, col.radius);
    }
}
