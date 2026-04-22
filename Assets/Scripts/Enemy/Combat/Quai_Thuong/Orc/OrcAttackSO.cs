using UnityEngine;

/// <summary>
/// Orc tấn công nặng — gây damage nhân hệ số + đẩy player ra (knockback).
/// Tạo asset: chuột phải Project > Create > Enemy/Attacks/Orc Attack
/// </summary>
[CreateAssetMenu(menuName = "Enemy/Attacks/Orc Attack")]
public class OrcAttackSO : AttackBehaviour
{
    [Tooltip("Hệ số nhân damage (ví dụ: 1.5 = đánh đau hơn 50% so với chỉ số base).")]
    [Range(1f, 3f)]
    public float damageMultiplier = 1.5f;

    [Tooltip("Lực đẩy player ra khi trúng đòn (Rigidbody2D.AddForce).")]
    public float knockbackForce = 8f;

    [Tooltip("Thời gian player bị stun / không điều khiển được sau knockback (giây).")]
    public float stunDuration = 0.4f;

    public override void ExecuteAttack(GameObject attacker, GameObject target)
    {
        EnemyStats myStats = attacker.GetComponent<EnemyStats>();
        if (myStats == null) return;

        PlayerStats player = target.GetComponent<PlayerStats>();
        if (player == null) return;

        // 1. Damage nhân hệ số
        int boostedDamage = Mathf.RoundToInt(myStats.Attack * damageMultiplier);
        player.TakeDamage(boostedDamage);

        Debug.Log($"[Orc] Chém mạnh {target.name} — {boostedDamage} damage (x{damageMultiplier})!");

        // 2. Knockback: đẩy player ra theo hướng ngược lại
        // FIX: không knockback nếu player đã chết (tránh bị hất xa trong lúc animation Die)
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null && !player.IsDead)
        {
            Vector2 knockDir = (target.transform.position - attacker.transform.position).normalized;
            rb.linearVelocity = Vector2.zero; // Reset velocity trước để knockback nhất quán
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            Debug.Log($"[Orc] Knockback player với lực {knockbackForce}!");
        }
    }
}
