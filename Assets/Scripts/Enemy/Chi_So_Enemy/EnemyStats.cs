using System.Collections;
using UnityEngine;
using Pathfinding;

public class EnemyStats : MonoBehaviour
{
    [Header("Base Stats")]
    public EnemyStatsData baseStats;
    public int currentHP;

    [Header("Rewards")]
    [HideInInspector] public int expReward = 5;

    [Tooltip("Giây chờ sau khi chết trước khi xóa GameObject.\n" +
             "Quái thường: 0.5-1s | Boss: 2-3s | 0 = xóa ngay.")]
    public float deathDelay = 0.8f;  // Đủ cho animation Die ngắn ~0.5-0.8s

    // Properties cho hệ AI attack
    public int Attack           => baseStats.attack;
    public float AttackRange    => baseStats.attackRange;
    public float AttackCooldown => baseStats.attackCooldown;

    private bool isDead;

    // ─── Component cache ───────────────────────────────────────
    private Animator    anim;
    private AIPath      aiPath;
    private Collider2D  col;
    private EnemyAudio  _enemyAudio;
    private BossAudio   _bossAudio;

    private void Awake()
    {
        currentHP = baseStats.maxHP;
        anim   = GetComponentInChildren<Animator>();
        aiPath = GetComponent<AIPath>();
        col    = GetComponent<Collider2D>();
        _enemyAudio = GetComponent<EnemyAudio>();
        _bossAudio  = GetComponent<BossAudio>();

        if (anim == null)
            Debug.LogWarning($"[EnemyStats] {gameObject.name}: Không tìm thấy Animator!");
    }

    // ─── Nhận sát thương ───────────────────────────────────────
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int finalDamage = Mathf.Max(damage - baseStats.defense, 1);
        currentHP -= finalDamage;

        if (currentHP > 0)
        {
            _enemyAudio?.PlayTakeDamage();
            _bossAudio?.PlayTakeDamage();
            if (anim != null) 
            {
                // Reset các trigger đánh để khỏi lấn Hurt
                EnemyAttackController atkCtrl = GetComponent<EnemyAttackController>();
                if (atkCtrl != null && atkCtrl.attackStrategy != null)
                {
                    anim.ResetTrigger(atkCtrl.attackStrategy.animTrigger);
                }

                // Nếu là boss thì Reset các skill của boss
                anim.ResetTrigger("Skill1");
                anim.ResetTrigger("Skill2");
                anim.ResetTrigger("Skill3");
                anim.ResetTrigger("Skill4");

                anim.SetTrigger("IsHurt");
            }
        }
        else
        {
            Die();
        }
    }

    // ─── Chết ────────────────────────────────────────────
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[EnemyStats] {gameObject.name} DIE called!");
        _enemyAudio?.PlayDeath();
        _bossAudio?.PlayDeath();

        if (anim != null)
        {
            anim.SetTrigger("IsDead");
            Debug.Log($"[EnemyStats] SetTrigger('IsDead') OK");
        }
        else
        {
            Debug.LogError($"[EnemyStats] {gameObject.name}: anim = NULL, không thể phát animation Die!");
        }

        // Tắt AI di chuyển ngay
        if (aiPath != null) aiPath.enabled = false;

        // Rớt item/vàng/exp TRƯỚC khi xóa
        EnemyDropper dropper = GetComponent<EnemyDropper>();
        if (dropper != null) dropper.Drop();

        // Khoá tấn công ngay lập tức
        EnemyAttackController atkCtrl = GetComponent<EnemyAttackController>();
        if (atkCtrl != null) atkCtrl.enabled = false;

        // Tắt Boss controller nếu là Boss
        VampireBossController bossCtrl = GetComponent<VampireBossController>();
        if (bossCtrl != null) bossCtrl.enabled = false;

        // Xóa sau deathDelay giây
        Debug.Log($"[EnemyStats] Destroy {gameObject.name} sau {deathDelay}s");
        Destroy(gameObject, deathDelay);
    }

    // Cho TopDownAnimator biết enemy đã chết
    public bool IsDead => isDead;
}
