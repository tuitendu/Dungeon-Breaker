using UnityEngine;

/// <summary>
/// Skill 3 — Một orb xuất hiện tại vị trí đất (top-down game).
/// Không dùng gravity vì game nhìn từ trên xuống.
/// Orb xuất hiện ở đúng vị trí warning → damage player nếu đứng đó → tự hủy sau ít giây.
/// </summary>
public class OrbRainDrop : MonoBehaviour
{
    [Header("Damage")]
    public int   damage      = 15;

    [Tooltip("Thời gian orb tồn tại tại vị trí đáp xuống trước khi tự hủy (giây).")]
    public float lingerTime  = 0.4f;

    [Header("Hit Radius")]
    [Tooltip("Bán kính vùng damage khi orb xuất hiện (OverlapCircle).")]
    public float hitRadius   = 0.5f;
    public LayerMask playerLayer;

    void Start()
    {
        // Damage ngay khi xuất hiện — nếu player đứng đúng chỗ
        CheckHit();

        // Tự hủy sau lingerTime
        Destroy(gameObject, lingerTime);
    }

    void CheckHit()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerStats ps = hit.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(damage);
                Debug.Log($"[OrbRainDrop] Trúng player: {damage} dmg");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
