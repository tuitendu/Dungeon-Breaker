using UnityEngine;
using System;

public class PlayerLevel : MonoBehaviour
{
    // Singleton
    public static PlayerLevel Instance { get; private set; }

    [Header("Level Data")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;

    [Header("Level Settings")]
    [SerializeField] private int maxLevel = 99;
    [SerializeField] private int baseExpToLevel = 100;
    [SerializeField] private float expMultiplier = 1.5f; // Mỗi level cần x1.5 exp

    [Header("Stat Increase Per Level")]
    [SerializeField] private int hpPerLevel = 10;
    [SerializeField] private int mpPerLevel = 5;
    [SerializeField] private int atkPerLevel = 2;
    [SerializeField] private int matkPerLevel = 2;
    [SerializeField] private int defPerLevel = 1;
    [SerializeField] private int mdefPerLevel = 1;

    // Events
    public event Action<int> OnExpChanged;       // (currentExp)
    public event Action<int> OnLevelUp;          // (newLevel)

    // Properties
    public int CurrentLevel => currentLevel;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => CalculateExpToNextLevel();

    private PlayerStats playerStats;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate PlayerLevel detected! Destroying {gameObject.name}");
            Destroy(this);
            return;
        }
        Instance = this;

        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // Tải tiến trình đã lưu
        LoadProgress();

        // Thông báo giá trị ban đầu
        OnExpChanged?.Invoke(currentExp);
    }

    /// <summary>
    /// Thêm EXP (gọi khi kill enemy)
    /// </summary>
    public void AddExp(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"AddExp: amount phải > 0 (received: {amount})");
            return;
        }

        if (currentLevel >= maxLevel)
        {
            Debug.Log("Đã đạt max level!");
            return;
        }

        currentExp += amount;
        Debug.Log($"+{amount} EXP | Total: {currentExp}/{ExpToNextLevel}");

        OnExpChanged?.Invoke(currentExp);

        // Kiểm tra level up (có thể lên nhiều level nếu exp đủ nhiều)
        while (currentExp >= ExpToNextLevel && currentLevel < maxLevel)
        {
            LevelUp();
        }

        // Tự động lưu
        SaveProgress();
    }

    private void LevelUp()
    {
        int expNeeded = ExpToNextLevel;
        currentExp -= expNeeded; // EXP thừa chuyển sang level sau

        currentLevel++;
        Debug.Log($"LEVEL UP! → Level {currentLevel}");

        // Tăng stats
        IncreaseStats();

        // Kích hoạt events
        OnLevelUp?.Invoke(currentLevel);
        OnExpChanged?.Invoke(currentExp);

        // Lưu
        SaveProgress();

        // TODO: Play effect/sound
        // LevelUpEffect();
    }

    private void IncreaseStats()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats không tìm thấy! Không thể tăng stats.");
            return;
        }

        // Tăng max stats
        playerStats.HP += hpPerLevel;
        playerStats.MP += mpPerLevel;
        playerStats.ATK += atkPerLevel;
        playerStats.MATK += matkPerLevel;
        playerStats.DEF += defPerLevel;
        playerStats.MDEF += mdefPerLevel;

        // Hồi full HP/MP khi level up
        playerStats.currentHealth = playerStats.HP;
        playerStats.currentMana = playerStats.MP;

        Debug.Log($"Stats tăng! HP +{hpPerLevel}, ATK +{atkPerLevel}, DEF +{defPerLevel}");
    }

    private int CalculateExpToNextLevel()
    {
        if (currentLevel >= maxLevel) return 0;

        // Formula: 100 → 150 → 225 → 337 → ...
        return Mathf.RoundToInt(baseExpToLevel * Mathf.Pow(expMultiplier, currentLevel - 1));
    }

    /// <summary>
    /// Tính tổng EXP cần để đạt level hiện tại (từ level 1)
    /// </summary>
    public int GetTotalExpForLevel(int level)
    {
        int total = 0;
        for (int i = 1; i < level; i++)
        {
            total += Mathf.RoundToInt(baseExpToLevel * Mathf.Pow(expMultiplier, i - 1));
        }
        return total;
    }

    /// <summary>
    /// Get progress (0.0 - 1.0)
    /// </summary>
    public float GetExpProgress()
    {
        int expNeeded = ExpToNextLevel;
        if (expNeeded == 0) return 1f; // Max level

        return (float)currentExp / expNeeded;
    }

    // ===== SAVE/LOAD =====
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("PlayerLevel", currentLevel);
        PlayerPrefs.SetInt("PlayerExp", currentExp);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentExp = PlayerPrefs.GetInt("PlayerExp", 0);

        Debug.Log($"Loaded: Level {currentLevel}, EXP {currentExp}");
    }

    public void ResetProgress()
    {
        currentLevel = 1;
        currentExp = 0;
        SaveProgress();

        OnLevelUp?.Invoke(currentLevel);
        OnExpChanged?.Invoke(currentExp);

        Debug.Log("Progress reset!");
    }

    /// <summary>
    /// Called by GameSaveManager when loading a save file.
    /// Restores level and exp without triggering level-up side effects.
    /// </summary>
    public void SetLevelData(int level, int exp)
    {
        currentLevel = Mathf.Clamp(level, 1, maxLevel);
        currentExp   = Mathf.Max(exp, 0);

        OnExpChanged?.Invoke(currentExp);
        OnLevelUp?.Invoke(currentLevel);

        Debug.Log($"[PlayerLevel] Loaded: Level {currentLevel}, EXP {currentExp}");
    }

    // ===== DEBUG =====
    [ContextMenu("Add 100 EXP")]
    private void DebugAddExp()
    {
        AddExp(100);
    }

    [ContextMenu("Level Up")]
    private void DebugLevelUp()
    {
        AddExp(ExpToNextLevel);
    }

    [ContextMenu("Reset Progress")]
    private void DebugReset()
    {
        ResetProgress();
    }

    private void Update()
    {
        #if UNITY_EDITOR
        // Phím debug
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddExp(50); // +50 exp
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            AddExp(ExpToNextLevel); // Force level up
        }
        #endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
