using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    // Singleton pattern
    public static PlayerWallet Instance { get; private set; }

    [Header("Currency")]
    [SerializeField] private int currentGold = 0;

    [Header("Settings")]
    [SerializeField] private int maxGold = 999999;

    // Events
    public event Action<int> OnGoldChanged;

    // Properties
    public int CurrentGold => currentGold;
    public int MaxGold => maxGold;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate PlayerWallet detected! Destroying {gameObject.name}");
            Destroy(this);
            return;
        }
        Instance = this;

        // Tùy chọn: Giữ object qua các scene
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Tải vàng đã lưu (nếu có save system)
        LoadGold();

        // Thông báo giá trị ban đầu cho UI
        OnGoldChanged?.Invoke(currentGold);
    }

    /// <summary>
    /// Thêm vàng vào ví
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"AddGold: amount phải > 0 (received: {amount})");
            return;
        }

        int oldGold = currentGold;
        currentGold = Mathf.Min(currentGold + amount, maxGold);
        int actualAdded = currentGold - oldGold;

        Debug.Log($"+{actualAdded} Gold | Total: {currentGold}");

        // Kích hoạt event
        OnGoldChanged?.Invoke(currentGold);

        // Lưu vào PlayerPrefs
        SaveGold();
    }

    /// <summary>
    /// Trừ vàng (dùng khi mua đồ)
    /// </summary>
    public bool RemoveGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"RemoveGold: amount phải > 0 (received: {amount})");
            return false;
        }

        if (currentGold < amount)
        {
            Debug.Log($"Không đủ vàng! Cần: {amount}, Có: {currentGold}");
            return false;
        }

        currentGold -= amount;
        Debug.Log($"-{amount} Gold | Còn lại: {currentGold}");

        // Kích hoạt event
        OnGoldChanged?.Invoke(currentGold);

        // Lưu
        SaveGold();

        return true;
    }

    /// <summary>
    /// Kiểm tra có đủ vàng không
    /// </summary>
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    /// <summary>
    /// Set vàng (dùng cho cheat/debug)
    /// </summary>
    public void SetGold(int amount)
    {
        currentGold = Mathf.Clamp(amount, 0, maxGold);
        OnGoldChanged?.Invoke(currentGold);
        SaveGold();
        Debug.Log($"Set Gold: {currentGold}");
    }

    // ===== SAVE/LOAD =====
    private void SaveGold()
    {
        PlayerPrefs.SetInt("PlayerGold", currentGold);
        PlayerPrefs.Save();
    }

    private void LoadGold()
    {
        currentGold = PlayerPrefs.GetInt("PlayerGold", 0); // Default = 0
        Debug.Log($"Loaded Gold: {currentGold}");
    }

    // ===== DEBUG =====
    private void Update()
    {
        // Phím debug (chỉ hoạt động trong Editor)
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
        {
            AddGold(100); // +100 vàng
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            RemoveGold(50); // -50 vàng
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
