using UnityEngine;

/// <summary>
/// Vampire Boss Controller — Boss đứng yên 1 chỗ, tung 3 skill liên tục xoay vòng.
///
/// State:
///   IDLE    → Đứng yên, chờ player bước vào tầm kích hoạt
///   CASTING → Tung skill liên tục, xoay vòng Skill1 → Skill2 → Skill3 → Skill1…
///
/// Setup trên Prefab VampireBoss:
///   - Rigidbody2D (Kinematic, Freeze Rotation Z)
///   - Collider2D
///   - Animator (optional)
///   - Skill1_BloodBurst, Skill2_BloodDrainZone, Skill3_OrbRain (trên cùng GameObject)
///   - VampireBossController (script này)
///   - KHÔNG cần AIPath vì boss không di chuyển
/// </summary>
public class VampireBossController : MonoBehaviour
{
    // ─── State ────────────────────────────────────────────────────────────────
    private enum BossState { Idle, Casting }
    private BossState state = BossState.Idle;

    // ─── References ───────────────────────────────────────────────────────────
    [Header("Target")]
    [Tooltip("Để trống — tự tìm theo tag Player.")]
    public Transform targetPlayer;

    [Header("Range")]
    [Tooltip("Player bước vào phạm vi này → boss bắt đầu tung skill.")]
    public float activationRange   = 10f;

    [Tooltip("Player ra khỏi phạm vi này → boss dừng skill về Idle.\n" +
             "Nên đặt lớn hơn activationRange một chút để tránh boss bật/tắt liên tục (hysteresis).")]
    public float deactivationRange = 13f;

    [Header("Skill Timing")]
    [Tooltip("Thời gian mỗi skill kéo dài (giây) trước khi chuyển skill kế.")]
    public float skillDuration = 4f;

    [Tooltip("Khoảng dừng ngắn giữa 2 skill (giây).")]
    public float betweenSkillPause = 0.8f;

    [Header("Animation Duration Per Skill")]
    [Tooltip("Skill 1 (liên thanh): giữ animation Attack suốt thời gian bắn.")]
    public float skill1AnimDuration = 4f;
    [Tooltip("Skill 2 (spawn zone): animation Attack ngắn rồi về Idle chờ.")]
    public float skill2AnimDuration = 0.8f;
    [Tooltip("Skill 3 (2 orb to): animation Attack ngắn rồi về Idle chờ.")]
    public float skill3AnimDuration = 0.8f;

    // ─── Components ───────────────────────────────────────────────────────────
    private Animator              anim;
    private Skill1_BloodBurst     skill1;
    private Skill2_BloodDrainZone skill2;
    private Skill3_OrbRain        skill3;
    private BossAudio             _audio;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private int   currentSkillIndex    = 0;
    private float skillTimer           = 0f;
    private bool  isBetweenSkill       = false;
    private float pauseTimer           = 0f;
    private bool  skillFiredThisCycle  = false;
    private float attackAnimTimer      = 0f;  // đếm ngược thời gian animation Attack

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    void Start()
    {
        anim   = GetComponent<Animator>();
        skill1 = GetComponent<Skill1_BloodBurst>();
        skill2 = GetComponent<Skill2_BloodDrainZone>();
        skill3 = GetComponent<Skill3_OrbRain>();
        _audio = GetComponent<BossAudio>();

        if (targetPlayer == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) targetPlayer = p.transform;
        }
    }

    void Update()
    {
        if (targetPlayer == null) return;

        float dist = Vector2.Distance(transform.position, targetPlayer.position);

        switch (state)
        {
            case BossState.Idle:    UpdateIdle(dist);        break;
            case BossState.Casting: UpdateCasting(dist);     break;
        }
    }

    // ─── State: IDLE — chờ player vào tầm ────────────────────────────────────
    void UpdateIdle(float dist)
    {
        if (anim != null) anim.SetBool("IsAttacking", false);
        if (dist <= activationRange)
        {
            state                = BossState.Casting;
            skillTimer           = 0f;
            skillFiredThisCycle  = false;
            isBetweenSkill       = false;
            _audio?.PlayIntro();
            _audio?.PlayBossBGM(); // ← Tự động bật nhạc Boss
            Debug.Log("[Boss] Player đến gần — bắt đầu tung skill!");
        }
    }

    // ─── State: CASTING — tung skill liên tục, không di chuyển ───────────────
    void UpdateCasting(float dist)
    {
        // Kiểm tra player đã ra quá xa → dừng về Idle
        if (dist > deactivationRange)
        {
            state = BossState.Idle;
            isBetweenSkill      = false;
            skillFiredThisCycle = false;
            skillTimer          = 0f;
            attackAnimTimer     = 0f;
            if (anim != null) anim.SetBool("IsAttacking", false);

            // Dừng tất cả TornadoZone + audio ngay lập tức
            skill2?.StopAllZones();

            // Trả lại nhạc nền cũ khi chạy ra khỏi vùng đánh boss
            _audio?.RestoreBGM();

            Debug.Log("[Boss] Player ra xa — boss dừng tấn công.");
            return;
        }

        FaceTarget();

        // Giai đoạn nghỉ ngắn giữa 2 skill → về Idle animation
        if (isBetweenSkill)
        {
            if (anim != null) anim.SetBool("IsAttacking", false);
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isBetweenSkill      = false;
                skillTimer          = 0f;
                skillFiredThisCycle = false;
            }
            return;
        }

        // Bắn skill 1 lần mỗi chu kỳ
        if (!skillFiredThisCycle)
        {
            FireCurrentSkill();
            skillFiredThisCycle = true;
        }

        // Animation Attack chỉ chạy trong attackAnimTimer giây, sau đó về Idle
        if (attackAnimTimer > 0f)
        {
            attackAnimTimer -= Time.deltaTime;
            if (anim != null) anim.SetBool("IsAttacking", true);
        }
        else
        {
            if (anim != null) anim.SetBool("IsAttacking", false); // Idle trong khi chờ
        }

        // Đếm thời gian skill
        skillTimer += Time.deltaTime;
        if (skillTimer >= skillDuration)
        {
            // Nếu đang ở Skill 2 (Tornado), dừng tất cả zone ngay khi chuyển sang skill kế
            if (currentSkillIndex == 1)
                skill2?.StopAllZones();

            currentSkillIndex = (currentSkillIndex + 1) % 3;
            isBetweenSkill    = true;
            pauseTimer        = betweenSkillPause;
            Debug.Log($"[Boss] ── Chuyển sang Skill {currentSkillIndex + 1} ──");
        }
    }

    // ─── Bắn skill theo index hiện tại ───────────────────────────────────────
    void FireCurrentSkill()
    {
        switch (currentSkillIndex)
        {
            case 0:
                attackAnimTimer = skill1AnimDuration; // giữ Attack animation cả 4s
                if (skill1 != null) skill1.Execute();
                else Debug.LogWarning("[Boss] Thiếu Skill1_BloodBurst!");
                break;
            case 1:
                attackAnimTimer = skill2AnimDuration; // Attack animation ngắn ~0.8s
                if (skill2 != null) skill2.Execute(targetPlayer);
                else Debug.LogWarning("[Boss] Thiếu Skill2_BloodDrainZone!");
                break;
            case 2:
                attackAnimTimer = skill3AnimDuration; // Attack animation ngắn ~0.8s
                if (skill3 != null) skill3.Execute(targetPlayer);
                else Debug.LogWarning("[Boss] Thiếu Skill3_OrbRain!");
                break;
        }
    }

    // ─── Quay mặt về phía player (cho Animator) ──────────────────────────────
    void FaceTarget()
    {
        if (anim == null || targetPlayer == null) return;
        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        anim.SetFloat("MoveX", dir.x);
        anim.SetFloat("MoveY", dir.y);
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Activation range — cam
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, activationRange);

        // Deactivation range — đỏ nhạt
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, deactivationRange);
    }
}
