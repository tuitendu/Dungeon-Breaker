using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class Sword_Combat : Player_CombatBase
{
    [Header("Sword Settings")]
    public float range = 1.5f;

    [Header("Skill 3 - Spinning Sword")]
    [Tooltip("Prefab chứa SpinningSwordEffect (kéo prefab vào đây trong Inspector)")]
    public GameObject spinningSwordPrefab;
    [Tooltip("Thời gian kiếm xoay tồn tại (giây)")]
    public float spinDuration = 4f;
    [Tooltip("Sát thương mỗi lần hit của kiếm xoay (x ATK của player)")]
    public float spinDamageMultiplier = 0.8f;
    private bool isSpinning = false;
    private PlayerStats  _playerStats;
    private PlayerAudio  _playerAudio;

    [Header("Buff Settings")]
    public float buffDuration = 5f;
    public float atkBuffMultiplier = 1.5f;
    public float defBuffMultiplier = 1.5f;

    int originalATK;
    int originalDEF;

    private bool isBuffing;

    private void Reset()
    {
        basicCooldown  = 0.5f;
        skill1Cooldown = 2.5f;
        skill2Cooldown = 8f;
        skill3Cooldown = 6f;
        skill4Cooldown = 15f;
    }

    protected override void Awake()
    {
        base.Awake();
        _playerStats = GetComponent<PlayerStats>();
        _playerAudio = GetComponent<PlayerAudio>();
    }

    // ===== BASIC ATTACK =====
    public override bool BasicAttack()
    {
        Collider2D[] hits = GetEnemiesInRange(range);
        if (hits.Length == 0) return false;

        foreach (var hit in hits)
        {
            EnemyStats enemy = hit.GetComponent<EnemyStats>();
            if (enemy != null)
                enemy.TakeDamage(stats.ATK);
        }

        Debug.Log("Sword: Basic Slash");
        return true;
    }

    // ===== SKILL 1: CHÉM BÁN NGUYỆT =====
    public override bool Skill1()
    {
        if (!UseMana(10)) return false;

        Collider2D[] hits = GetEnemiesInRange(range * 2f);
        foreach (var hit in hits)
        {
            EnemyStats enemy = hit.GetComponent<EnemyStats>();
            if (enemy != null)
                enemy.TakeDamage(stats.ATK * 2);
        }

        Debug.Log("Sword Skill 1: Crescent Slash");
        return true;
    }

    // ===== SKILL 2: BUFF GIÁP + BUFF DAME =====
    public override bool Skill2()
    {
        if (isBuffing) return false;
        if (!UseMana(20)) return false;

        StartCoroutine(BuffCoroutine());

        Debug.Log("Sword Skill 2: Battle Spirit (Buff)");
        return true;
    }

    IEnumerator BuffCoroutine()
    {
        isBuffing = true;

        originalATK = stats.ATK;
        originalDEF = stats.DEF;

        stats.ATK = Mathf.RoundToInt(stats.ATK * atkBuffMultiplier);
        stats.DEF = Mathf.RoundToInt(stats.DEF * defBuffMultiplier);

        yield return new WaitForSeconds(buffDuration);

        stats.ATK = originalATK;
        stats.DEF = originalDEF;

        isBuffing = false;
    }

    // ===== SKILL 3: KIẾM XOAY (SPINNING SWORD) =====
    public override bool Skill3()
    {
        // Không cho dùng lại khi đang xoay
        if (isSpinning) return false;
        if (!UseMana(25)) return false;

        if (spinningSwordPrefab == null)
        {
            Debug.LogError("Sword_Combat: Chưa gán spinningSwordPrefab trong Inspector!");
            return false;
        }

        // Tính sát thương dựa trên ATK hiện tại
        int damage = Mathf.RoundToInt(stats.ATK * spinDamageMultiplier);

        // Spawn hiệu ứng kiếm xoay
        SpinningSwordEffect effect = SpinningSwordEffect.Spawn(
            spinningSwordPrefab,
            transform,
            damage,
            enemyLayer
        );

        if (effect != null)
        {
            // Đồng bộ thời gian tồn tại
            effect.duration = spinDuration;
            StartCoroutine(SpinCooldownRoutine());
        }

        Debug.Log($"Sword Skill 3: Spinning Sword (damage/hit: {damage})");
        return true;
    }

    private IEnumerator SpinCooldownRoutine()
    {
        isSpinning = true;

        // Bật bản lập: chặn dame trong suốt thời gian kiếm xoay
        if (_playerStats != null) _playerStats.IsShielded = true;
        Debug.Log("[Skill3] Bản lập BẬT — player miễn dame!");

        yield return new WaitForSeconds(spinDuration);

        // Tắt bản lập khi skill kết thúc
        if (_playerStats != null) _playerStats.IsShielded = false;
        Debug.Log("[Skill3] Bản lập TẮT.");

        // Dừng audio loop đúng lúc skill kết thúc
        _playerAudio?.StopSkill3();

        isSpinning = false;
    }

    // ===== SKILL 4: TRẢM KÍCH (ULTIMATE) =====
    public override bool Skill4()
    {
        if (!UseMana(40)) return false;

        Collider2D[] hits = GetEnemiesInRange(range * 3f);
        foreach (var hit in hits)
        {
            EnemyStats enemy = hit.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                float multiplier = UnityEngine.Random.Range(4f, 5f);
                int damage = Mathf.RoundToInt(stats.ATK * multiplier);
                enemy.TakeDamage(damage);
            }
        }

        Debug.Log("Sword Ultimate: Trảm Kích!");
        return true;
    }

}
