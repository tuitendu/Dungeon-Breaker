using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Skill 3 - Spinning Sword:
/// Bám theo player, gây sát thương liên tục cho enemy trong vùng, rồi tự hủy.
/// Phần xoay visual do Animation (sprite sheet 4 frame) xử lý sẵn.
/// </summary>
public class SpinningSwordEffect : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Sát thương mỗi lần hit (set tự động từ Sword_Combat)")]
    public int damagePerHit = 0;

    [Tooltip("Giây chờ giữa 2 lần hit cùng 1 enemy")]
    public float damageCooldown = 0.5f;

    [Header("Duration")]
    [Tooltip("Tổng thời gian tồn tại (giây)")]
    public float duration = 4f;

    [Header("Range")]
    [Tooltip("Bán kính vùng gây damage xung quanh player")]
    public float damageRadius = 1.5f;

    [Header("Layer")]
    public LayerMask enemyLayer;

    // ── Runtime ──
    private Transform player;
    private float timer = 0f;
    private Dictionary<Collider2D, float> hitCooldowns = new Dictionary<Collider2D, float>();

    // ── Static Factory ──
    public static SpinningSwordEffect Spawn(
        GameObject prefab,
        Transform player,
        int damage,
        LayerMask enemyLayer)
    {
        if (prefab == null) { Debug.LogError("SpinningSwordEffect: prefab null!"); return null; }

        GameObject go = Instantiate(prefab, player.position, Quaternion.identity);
        SpinningSwordEffect effect = go.GetComponent<SpinningSwordEffect>();
        if (effect != null)
        {
            effect.player      = player;
            effect.damagePerHit = damage;
            effect.enemyLayer  = enemyLayer;
        }
        return effect;
    }

    // ── Unity Lifecycle ──
    private void Update()
    {
        if (player == null) { Destroy(gameObject); return; }

        // Bám theo player
        transform.position = player.position;

        // Gây damage
        DetectAndDamage();

        // Đếm thời gian tồn tại
        timer += Time.deltaTime;
        if (timer >= duration)
            Destroy(gameObject);
    }

    private void DetectAndDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, damageRadius, enemyLayer);
        float now = Time.time;

        foreach (Collider2D col in enemies)
        {
            if (hitCooldowns.TryGetValue(col, out float lastHit) && now - lastHit < damageCooldown)
                continue;

            EnemyStats enemy = col.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerHit);
                hitCooldowns[col] = now;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}

