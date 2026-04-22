using UnityEngine;
using System.Collections;

/// <summary>
/// Skill 1 — Spinning Rapid Fire:
/// Bắn viên lửa liên thanh, góc bắn xoay ngược chiều kim đồng hồ.
/// Dùng prefab FireOrb_Projectile (fire animation + BloodOrbProjectile script).
/// </summary>
public class Skill1_BloodBurst : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Kéo prefab FireOrb_Projectile vào đây (fire animation + damage).")]
    public GameObject fireOrbPrefab;

    [Header("Max Range (chống lag)")]
    [Tooltip("Projectile tự hủy khi bay quá khoảng cách này. 0 = không giới hạn.")]
    public float maxRange = 12f;

    [Header("Damage & Speed")]
    public int   damage          = 12;
    public float projectileSpeed = 7f;

    [Header("Rapid Fire")]
    [Tooltip("Giây giữa 2 viên liên tiếp (nhỏ = bắn nhanh hơn).")]
    [Range(0.05f, 1f)]
    public float fireInterval = 0.18f;

    [Tooltip("Tổng thời gian skill bắn (giây) — nên khớp với skillDuration trên BossController.")]
    public float totalDuration = 4f;

    [Header("Rotation")]
    [Tooltip("Số độ xoay ngược chiều kim đồng hồ sau mỗi phát bắn.")]
    [Range(5f, 60f)]
    public float degreePerShot = 15f;

    // Cache
    private Transform playerTransform;
    private Coroutine spinRoutine;

    void Start()
    {
        // Tìm player 1 lần
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    // ─── Gọi từ VampireBossController ────────────────────────────────────────
    public void Execute()
    {
        if (fireOrbPrefab == null)
        {
            Debug.LogError("[Skill1] Chưa gán fireOrbPrefab!");
            return;
        }

        GetComponent<BossAudio>()?.PlaySkill1Start();

        if (spinRoutine != null)
            StopCoroutine(spinRoutine);

        spinRoutine = StartCoroutine(SpinFireRoutine());
    }

    // ─── Coroutine: bắn liên thanh xoay vòng ─────────────────────────────────
    private IEnumerator SpinFireRoutine()
    {
        // Tính góc khởi đầu = hướng từ boss → player lúc kích hoạt
        float startAngle = 0f;
        if (playerTransform != null)
        {
            Vector2 toPlayer = (playerTransform.position - transform.position).normalized;
            startAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        }

        float currentAngle = startAngle;
        float elapsed      = 0f;

        while (elapsed < totalDuration)
        {
            FireOrb(currentAngle);

            // Xoay ngược chiều kim đồng hồ: cộng góc (CCW = góc tăng trong toán học 2D)
            currentAngle += degreePerShot;

            elapsed += fireInterval;
            yield return new WaitForSeconds(fireInterval);
        }

        spinRoutine = null;
        Debug.Log("[Skill1] Spinning Fire kết thúc.");
    }

    private void FireOrb(float angleDeg)
    {
        float rad        = angleDeg * Mathf.Deg2Rad;
        Vector2 dir      = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector3 spawnPos = transform.position;

        GetComponent<BossAudio>()?.PlayFireOrb();

        GameObject orb = Instantiate(fireOrbPrefab, spawnPos, Quaternion.identity);
        BloodOrbProjectile proj = orb.GetComponent<BloodOrbProjectile>();
        if (proj != null)
        {
            proj.direction = dir;
            proj.speed     = projectileSpeed;
            proj.damage    = damage;
            proj.maxRange  = maxRange;
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        // Hiển thị hướng bắn đầu (hướng phải - default)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.right * 2f);

        // Vài hướng demo theo degreePerShot
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        for (int i = 1; i <= 5; i++)
        {
            float rad = (i * degreePerShot) * Mathf.Deg2Rad;
            Vector3 d = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
            Gizmos.DrawRay(transform.position, d * 1.5f);
        }
    }
}
