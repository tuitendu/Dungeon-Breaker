using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Skill 2 — Tornado Zone: Tạo các vùng tornado tại vị trí ngẫu nhiên
/// bên trong bán kính vòng tròn quanh boss.
/// Gắn lên prefab TornadoZone (tornado animation + BloodDrainZoneEffect).
/// </summary>
public class Skill2_BloodDrainZone : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Kéo prefab TornadoZone vào đây (tornado animation + BloodDrainZoneEffect script).")]
    public GameObject tornadoZonePrefab;

    [Header("Random Spawn")]
    [Tooltip("Số vùng spawn mỗi lần dùng skill.")]
    [Range(2, 16)]
    public int zoneCount = 9;

    [Tooltip("Khoảng cách TỐI THIỂU từ boss → zone không spawn sát chân boss.\n" +
             "Nên = bán kính collider boss.")]
    public float minSpawnDistance = 1.5f;

    [Tooltip("Bán kính TỐI ĐA từ boss — zone spawn trong vùng vành khăn (annulus).\n" +
             "minSpawnDistance < zone < spawnRadius")]
    public float spawnRadius = 5f;

    [Tooltip("Khoảng cách tối thiểu giữa 2 vùng (tránh chồng lên nhau).")]
    public float minDistance = 1.2f;

    [Tooltip("Thời gian tồn tại của mỗi vùng (giây).")]
    public float zoneDuration = 4f;

    // Danh sách zone đang sống — dùng để destroy khi boss dừng
    private readonly List<GameObject> _activeZones = new();

    /// <summary> Gọi từ VampireBossController. </summary>
    public void Execute(Transform playerTransform)
    {
        if (tornadoZonePrefab == null)
        {
            Debug.LogError("[Skill2_BloodDrainZone] Chưa gán tornadoZonePrefab!");
            return;
        }

        GetComponent<BossAudio>()?.PlaySkill2Start();

        // Lưu các vị trí đã spawn để kiểm tra minDistance
        List<Vector3> spawnedPositions = new();

        int maxAttempts = zoneCount * 5;
        int attempts    = 0;
        int spawned     = 0;

        while (spawned < zoneCount && attempts < maxAttempts)
        {
            attempts++;

            float distance       = Random.Range(minSpawnDistance, spawnRadius);
            Vector2 direction    = Random.insideUnitCircle.normalized;
            Vector2 randomOffset = direction * distance;
            Vector3 spawnPos     = transform.position + (Vector3)randomOffset;

            bool tooClose = false;
            foreach (Vector3 existing in spawnedPositions)
            {
                if (Vector3.Distance(spawnPos, existing) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;

            spawnedPositions.Add(spawnPos);
            GameObject zone = Instantiate(tornadoZonePrefab, spawnPos, Quaternion.identity);
            Destroy(zone, zoneDuration);
            _activeZones.Add(zone); // track để StopAllZones có thể dừng sớm
            spawned++;
        }

        Debug.Log($"[Skill2_BloodDrainZone] Spawn {spawned}/{zoneCount} vùng ngẫu nhiên (R={spawnRadius})");
    }

    /// <summary>
    /// Destroy tất cả TornadoZone đang sống ngay lập tức.
    /// Gọi khi boss dừng tung skill (về Idle) → AmbientSource trên prefab tự dừng.
    /// </summary>
    public void StopAllZones()
    {
        foreach (GameObject zone in _activeZones)
        {
            if (zone != null)
                Destroy(zone);
        }
        _activeZones.Clear();
        Debug.Log("[Skill2] Dừng tất cả Tornado Zone, audio tự tắt.");
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, minSpawnDistance);

        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}
