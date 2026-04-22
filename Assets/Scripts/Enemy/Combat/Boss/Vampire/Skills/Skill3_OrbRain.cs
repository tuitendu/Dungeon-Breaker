using UnityEngine;
using System.Collections;

/// <summary>
/// Skill 3 — Sequential Fire Shot:
/// Bắn các quả lửa lần lượt về phía player.
/// Mỗi quả lệch ngẫu nhiên trong ±(spreadAngle/2)° so với hướng thẳng đến player.
/// → Player luôn ở chính giữa vùng góc, từng quả bay lệch ngẫu nhiên.
/// </summary>
public class Skill3_OrbRain : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Kéo prefab FireOrb_Projectile vào đây (fire animation + damage, dùng chung với Skill 1).")]
    public GameObject fireOrbPrefab;

    [Header("Max Range (chống lag)")]
    [Tooltip("Projectile tự hủy khi bay quá khoảng cách này. 0 = không giới hạn.")]
    public float maxRange = 15f;

    [Header("Stats")]
    [Tooltip("Sát thương mỗi quả.")]
    public int   damage   = 30;

    [Tooltip("Tốc độ bay của quả lửa.")]
    public float orbSpeed = 4f;

    [Tooltip("Scale của orb to so với orb thường (2 = to gấp đôi).")]
    public float orbScale = 2.5f;

    [Tooltip("Thời gian tồn tại của orb nếu không trúng gì.")]
    public float lifetime = 5f;

    [Header("Firing")]
    [Tooltip("Tổng số quả bắn ra mỗi lần dùng skill.")]
    public int orbCount = 6;

    [Tooltip("Thời gian giữa 2 quả liên tiếp (giây).")]
    [Range(0.05f, 2f)]
    public float fireInterval = 0.25f;

    [Header("Spread")]
    [Tooltip("Tổng góc phát tán (độ). Player ở chính giữa.\n" +
             "VD: 30 → mỗi quả lệch random ±15° so với hướng đến player.")]
    [Range(5f, 90f)]
    public float spreadAngle = 30f;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private Coroutine fireRoutine;

    // ─── Gọi từ VampireBossController ────────────────────────────────────────
    public void Execute(Transform playerTransform)
    {
        if (fireOrbPrefab == null)
        {
            Debug.LogError("[Skill3] Chưa gán fireOrbPrefab!");
            return;
        }

        GetComponent<BossAudio>()?.PlaySkill3Start();

        if (fireRoutine != null)
            StopCoroutine(fireRoutine);

        fireRoutine = StartCoroutine(FireRoutine(playerTransform));
    }

    // ─── Coroutine: bắn lần lượt ─────────────────────────────────────────────
    private IEnumerator FireRoutine(Transform playerTransform)
    {
        for (int i = 0; i < orbCount; i++)
        {
            // Tính lại hướng đến player tại thời điểm bắn quả này (tracking)
            Vector2 dirToPlayer = Vector2.right;
            if (playerTransform != null)
                dirToPlayer = (playerTransform.position - transform.position).normalized;

            float baseAngle    = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            float randomOffset = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            float finalAngle   = baseAngle + randomOffset;

            float rad = finalAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            SpawnOrb(dir);

            yield return new WaitForSeconds(fireInterval);
        }

        fireRoutine = null;
        Debug.Log($"[Skill3] Bắn xong {orbCount} quả lửa (spread={spreadAngle}°).");
    }

    // ─── Spawn 1 quả ─────────────────────────────────────────────────────────
    private void SpawnOrb(Vector2 direction)
    {
        GetComponent<BossAudio>()?.PlayFireOrbBig();

        GameObject orb = Instantiate(fireOrbPrefab, transform.position, Quaternion.identity);
        orb.transform.localScale = Vector3.one * orbScale;

        BloodOrbProjectile proj = orb.GetComponent<BloodOrbProjectile>();
        if (proj != null)
        {
            proj.direction = direction;
            proj.speed     = orbSpeed;
            proj.damage    = damage;
            proj.lifetime  = lifetime;
            proj.maxRange  = maxRange;
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        float half = spreadAngle * 0.5f;

        // 2 cạnh biên của cung
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        float radL = (-half) * Mathf.Deg2Rad;
        float radR = ( half) * Mathf.Deg2Rad;
        Gizmos.DrawRay(transform.position, new Vector3(Mathf.Cos(radL), Mathf.Sin(radL)) * 3f);
        Gizmos.DrawRay(transform.position, new Vector3(Mathf.Cos(radR), Mathf.Sin(radR)) * 3f);

        // Hướng chính giữa (hướng phải = demo, lúc runtime = hướng đến player)
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.right * 3.5f);
    }
}
