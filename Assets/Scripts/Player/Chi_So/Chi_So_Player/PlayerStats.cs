using UnityEngine;
using System;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData baseStats;

    public int currentHealth;
    public int currentMana;

    [SerializeField] private UI_HealthBar healthBar;
    [SerializeField] private UI_ManaBar manaBar;

    [Header("Runtime Stats")]
    public int HP;
    public int MP;
    public int STA;

    public int ATK;
    public int MATK;
    public int DEF;
    public int MDEF;

    public float speed;
    public float critRate;
    public float range;

    // ─── State ──────────────────────────────────────────────────
    private bool isDead = false;
    public bool IsDead => isDead;

    /// <summary>
    /// True khi player đang có bản lập (shield) — Sword Skill 3.
    /// Dame vật lý bị chặn hoàn toàn trong khi IsShielded = true.
    /// </summary>
    public bool IsShielded = false;

    // ─── Component cache ────────────────────────────────────────
    private Animator anim;

    void Awake()
    {
        LoadStats();
        anim = GetComponent<Animator>();
    }

    void LoadStats()
    {
        // Khởi tạo cơ bản, tính toán chi tiết sẽ qua RecomputeStats
        RecomputeStats();
    }

    /// <summary>
    /// Tính toán lại toàn bộ chỉ số dựa trên BaseStats + Trang bị đang mặc
    /// </summary>
    public void RecomputeStats()
    {
        if (baseStats == null) return;

        // 1. Lấy chỉ số gốc
        HP = baseStats.maxHP;
        MP = baseStats.maxMP;
        STA = baseStats.maxSTA;

        ATK  = baseStats.ATK;
        MATK = baseStats.MATK;
        DEF  = baseStats.DEF;
        MDEF = baseStats.MDEF;

        speed    = baseStats.speed;
        critRate = baseStats.critRate;
        range    = baseStats.range;

        // 2. Cộng thêm chỉ số từ trang bị
        if (EquipmentManager.Instance != null)
        {
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                EquipmentItemData item = EquipmentManager.Instance.GetEquippedItem(slot);
                if (item != null)
                {
                    HP += item.hpBonus;
                    MP += item.mpBonus;
                    
                    ATK += item.atkBonus;
                    MATK += item.matkBonus;
                    
                    DEF += item.defBonus;
                    MDEF += item.mdefBonus;
                    
                    speed += item.speedBonus;
                    critRate += item.critRateBonus;
                }
            }
        }

        // Đảm bảo HP và MP hiện tại không vượt quá Max mới
        if (currentHealth > HP) currentHealth = HP;
        if (currentMana   > MP) currentMana   = MP;

        // Cập nhật max và GIỮ NGUYÊN giá trị hiện tại (không reset về full)
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(HP);
            healthBar.SetHealth(currentHealth);   // ← FIX: giữ current HP
        }
        if (manaBar != null)
        {
            manaBar.SetMaxMana(MP);
            manaBar.SetMana(currentMana);         // ← FIX: giữ current MP
        }
    }

    void Start()
    {
        // Chỉ khởi tạo về Full nếu chưa có giá trị (new game, chưa load save)
        // GameSaveManager [order 100] sẽ load và gọi RefreshUI SAU đây
        if (currentHealth <= 0) currentHealth = HP;
        if (currentMana   <= 0) currentMana   = MP;

        healthBar.SetMaxHealth(HP);
        healthBar.SetHealth(currentHealth, true);

        manaBar.SetMaxMana(MP);
        manaBar.SetMana(currentMana, true);
    }

    void Update()
    {
        // Debug keys
        if (Input.GetKeyDown(KeyCode.J)) TakeDamage(1);
        if (Input.GetKeyDown(KeyCode.H)) Heal(1);
        if (Input.GetKeyDown(KeyCode.K)) DecreaseMana(10);
        if (Input.GetKeyDown(KeyCode.L)) IncreaseMana(20);
    }

    // ─── Mana ───────────────────────────────────────────────────
    public void DecreaseMana(int amount)
    {
        currentMana -= amount;
        manaBar.SetMana(currentMana);
    }

    public void IncreaseMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, MP);
        manaBar.SetMana(currentMana);
        Debug.Log($"Restored {amount} MP. Current: {currentMana}/{MP}");
    }

    // ─── Nhận sát thương ────────────────────────────────────────
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Bản lập (Skill 3 Sword): chặn hoàn toàn dame vật lý
        if (IsShielded)
        {
            Debug.Log("[PlayerStats] Bản lập chặn dame!");
            return;
        }

        int finalDamage = Mathf.Max(damage - DEF, 1);
        currentHealth -= finalDamage;

        healthBar.SetHealth(currentHealth);
        Debug.Log($"Player bị đánh {finalDamage} máu | HP còn {currentHealth}/{HP}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            GetComponent<PlayerAudio>()?.PlayTakeDamage();
            if (anim != null) 
            {
                // Xoá hàng chờ của các animation tấn công để Hurt nhảy lên ưu tiên
                anim.ResetTrigger("IsAttackBase");
                anim.ResetTrigger("IsSkill1");
                anim.ResetTrigger("IsSkill2");
                anim.ResetTrigger("IsSkill3");
                anim.ResetTrigger("IsSkill4");

                anim.SetTrigger("IsHurt");
            }
        }
    }

    // ─── Hồi máu ────────────────────────────────────────────────
    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, HP);
        healthBar.SetHealth(currentHealth);
        Debug.Log($"Healed {amount} HP. Current: {currentHealth}/{HP}");
    }

    // ─── Chết ───────────────────────────────────────────────────
    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player chết");
        GetComponent<PlayerAudio>()?.PlayDeath();

        // FIX: Dừng toàn bộ vật lý ngay khi chết
        // Tránh bị hất ra xa do knockback từ đòn đánh cuối cùng
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Khoá input (Playermove sẽ check IsDead)
        if (anim != null) anim.SetBool("IsDead", true);

        // Bắt đầu quá trình hồi sinh ngầm
        StartCoroutine(HandleDeathRoutine());
    }

    private IEnumerator HandleDeathRoutine()
    {
        // 1. Đợi 2 giây cho player xem animation gục ngã
        yield return new WaitForSeconds(2f);

        // 2. Mờ đen màn hình
        if (SceneFadeManager.Instance != null)
        {
            yield return StartCoroutine(SceneFadeManager.Instance.FadeRoutine(1f, 1f)); // Thời gian mờ 1s
        }

        // 3. Phục hồi sinh lực (Revive Logic)
        isDead = false;
        if (anim != null) 
        {
            anim.SetBool("IsDead", false);
            anim.Play("Idle"); // Ép về Idle để tránh bị kẹt animation Die ở frame đầu
        }

        currentHealth = HP;
        currentMana = MP;
        RefreshUI();

        // 4. Lấy toạn độ Respawn Cố Định (từ cấu hình GameSaveManager)
        if (GameSaveManager.Instance != null)
        {
            // Đưa player về điểm Hồi Sinh Setup sẵn
            transform.position = new Vector3(GameSaveManager.Instance.respawnPosition.x, GameSaveManager.Instance.respawnPosition.y, transform.position.z);
            GameSaveManager.Instance.currentMapID = GameSaveManager.Instance.respawnMapID;
        }
        else
        {
            // Fallback (nhỡ mất tham chiếu)
            transform.position = Vector3.zero;
        }

        Debug.Log($"[PlayerStats] Player đã hồi sinh tại Checkpoint {GameSaveManager.Instance?.respawnMapID}!");

        // 5. Mẹo xử lý Camera & Giữ nguyên đồ:
        // Quá trình này sẽ:
        // - SaveGame(): Lưu LẠI Gold/Item hiện đang có, nhưng với tọa độ SafeArea và HP 100% vừa set.
        // - LoadGame(): Kích hoạt lại hoàn hảo Cinemachine Confiner và Camera Position cho Map đó.
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
            GameSaveManager.Instance.LoadGame();
        }

        yield return new WaitForSeconds(0.5f);

        // 6. Tắt màn đen, chơi tiếp
        if (SceneFadeManager.Instance != null)
        {
            yield return StartCoroutine(SceneFadeManager.Instance.FadeRoutine(0f, 1f));
        }
    }

    // ─── Refresh UI sau khi load save ───────────────────────────
    public void RefreshUI()
    {
        if (healthBar != null) healthBar.SetHealth(currentHealth, true);
        if (manaBar   != null) manaBar.SetMana(currentMana, true);
    }
}
