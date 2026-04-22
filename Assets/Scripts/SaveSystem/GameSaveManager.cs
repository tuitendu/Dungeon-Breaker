using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton MonoBehaviour that coordinates between game systems and the save file.
/// Attach this to a persistent GameManager object in the scene.
/// DefaultExecutionOrder(100) đảm bảo Start() chạy SAU PlayerLevel, PlayerWallet.
/// </summary>
[DefaultExecutionOrder(100)]
public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    [Header("Save Settings")]
    [SerializeField] private int saveSlot = 0;
    [SerializeField] private float autoSaveInterval = 30f; // giây, đặt 0 để tắt

    // References to game systems - assigned via Inspector or auto-found in Awake.
    [Header("References")]
    [SerializeField] private PlayerStats    playerStats;
    [SerializeField] private PlayerLevel    playerLevel;
    [SerializeField] private PlayerWallet   playerWallet;
    [SerializeField] private InventoryManager  inventoryManager;
    [SerializeField] private EquipmentManager  equipmentManager;

    // Reference to the item database so we can look up ItemData by name on load.
    [SerializeField] private ItemDatabase itemDatabase;

    [System.Serializable]
    public class MapBoundaryConfig
    {
        public string mapID;
        public PolygonCollider2D mapBoundary;
    }

    [Header("Map & Camera Config")]
    [SerializeField] private List<MapBoundaryConfig> mapConfigs = new List<MapBoundaryConfig>();
    public string currentMapID = "MainMap";

    [Header("Respawn Config (Điểm hồi sinh khi chết)")]
    [Tooltip("ID của Map nơi người chơi sống dậy (phải khớp MapBoundaryConfig). Ví dụ: MainMap")]
    public string respawnMapID = "MainMap";
    [Tooltip("Toạ độ X,Y trên bản đồ (lấy tọa độ của 1 cái giường/lửa trại)")]
    public Vector2 respawnPosition = Vector2.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResolveReferences();
    }

    private void Start()
    {
        // Auto-load khi game khởi động.
        if (SaveSystem.HasSave(saveSlot))
        {
            LoadGame();
        }

        // Bật auto-save định kỳ.
        if (autoSaveInterval > 0f)
        {
            InvokeRepeating(nameof(SaveGame), autoSaveInterval, autoSaveInterval);
            Debug.Log("[GameSaveManager] Auto-save bật, mỗi " + autoSaveInterval + "s.");
        }
    }

    // Save khi người chơi tắt game.
    private void OnApplicationQuit()
    {
        SaveGame();
        Debug.Log("[GameSaveManager] Đã lưu khi thoát game.");
    }

    // Save khi app vào background (quan trọng trên mobile).
    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveGame();
    }

    // ── Test trong Editor ────────────────────────────────────────
    // Chuột phải vào component trong Inspector → chọn để test
    [UnityEngine.ContextMenu("💾 Save Game NOW")]
    private void EditorSave() => SaveGame();

    [UnityEngine.ContextMenu("📂 Load Game NOW")]
    private void EditorLoad() => LoadGame();

    [UnityEngine.ContextMenu("🗑️ Delete Save")]
    private void EditorDelete() => DeleteSave();

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Collects data from all game systems and writes a save file.
    /// Call this when the player pauses, exits a dungeon, etc.
    /// </summary>
    public void SaveGame()
    {
        if (!ValidateReferences()) return;

        SaveData data = new SaveData();

        // PlayerLevel
        data.level      = playerLevel.CurrentLevel;
        data.currentExp = playerLevel.CurrentExp;

        // PlayerWallet
        data.gold = playerWallet.CurrentGold;

        // PlayerStats
        data.currentHP   = Mathf.Max(0, playerStats.currentHealth); // clamp: không lưu HP âm
        data.currentMana = Mathf.Max(0, playerStats.currentMana);

        // World position & Map ID
        data.playerPosX = playerStats.transform.position.x;
        data.playerPosY = playerStats.transform.position.y;
        data.lastMapID = currentMapID;

        // Inventory
        CollectInventoryData(data);

        // Equipment (trang bị đang mặc)
        CollectEquipmentData(data);

        SaveSystem.Save(data, saveSlot);
    }

    /// <summary>
    /// Reads the save file and applies data back to all game systems.
    /// </summary>
    public void LoadGame()
    {
        if (!ValidateReferences()) return;

        SaveData data = SaveSystem.Load(saveSlot);
        if (data == null) return;

        // PlayerLevel - use reflection-friendly public setter approach.
        playerLevel.SetLevelData(data.level, data.currentExp);

        // PlayerWallet
        playerWallet.SetGold(data.gold);

        // PlayerStats — clamp về [0, MaxHP] để tránh load HP âm hoặc vượt max
        playerStats.currentHealth = Mathf.Clamp(data.currentHP,   0, playerStats.HP);
        playerStats.currentMana   = Mathf.Clamp(data.currentMana, 0, playerStats.MP);
        // Nếu HP = 0 (chết), hồi về 1 để không bị stuck dead state
        if (playerStats.currentHealth <= 0) playerStats.currentHealth = 1;
        playerStats.RefreshUI();

        // World position
        playerStats.transform.position = new UnityEngine.Vector3(
            data.playerPosX,
            data.playerPosY,
            playerStats.transform.position.z
        );

        // Map Boundary & Camera Recovery — dùng Coroutine để chờ Cinemachine init xong
        currentMapID = data.lastMapID;
        if (string.IsNullOrEmpty(currentMapID)) currentMapID = "MainMap";
        Debug.Log($"[Save/Load] Đang load Map ID: {currentMapID}");
        StartCoroutine(ApplyCameraConfinerDelayed(currentMapID));

        // Inventory
        ApplyInventoryData(data);

        // Equipment (trang bị đang mặc) - load SAU inventory để tránh conflict
        ApplyEquipmentData(data);
    }

    /// <summary>
    /// Delay gán Camera Confiner để đảm bảo Cinemachine đã khởi tạo xong.
    /// Gán nhiều lần qua nhiều frame để chống Cinemachine override.
    /// </summary>
    private System.Collections.IEnumerator ApplyCameraConfinerDelayed(string mapID)
    {
        MapBoundaryConfig mapConfig = mapConfigs.Find(m => m.mapID == mapID);
        if (mapConfig == null || mapConfig.mapBoundary == null)
        {
            Debug.LogError($"[Save/Load] LỖI: Chưa cấu hình Map Boundary cho Map ID: {mapID} trong GameSaveManager!");
            yield break;
        }

        // Gán ngay lần 1 (cho nhanh)
        ApplyCameraConfiner(mapConfig);

        // Chờ hết frame hiện tại
        yield return new WaitForEndOfFrame();
        ApplyCameraConfiner(mapConfig);

        // Chờ thêm 1 frame nữa (Cinemachine thường init trong LateUpdate)
        yield return null;
        ApplyCameraConfiner(mapConfig);

        Debug.Log($"[Save/Load] Camera Confiner đã gán xong cho {mapID} (sau delay)");
    }

    private void ApplyCameraConfiner(MapBoundaryConfig mapConfig)
    {
        Unity.Cinemachine.CinemachineConfiner2D confiner = FindFirstObjectByType<Unity.Cinemachine.CinemachineConfiner2D>();
        if (confiner == null) return;

        confiner.BoundingShape2D = mapConfig.mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        // Ép camera dịch chuyển ngay lập tức theo player
        Unity.Cinemachine.CinemachineCamera cam = confiner.GetComponent<Unity.Cinemachine.CinemachineCamera>();
        if (cam != null && playerStats != null)
            cam.ForceCameraPosition(playerStats.transform.position, cam.transform.rotation);
    }

    /// <summary>
    /// Deletes the current slot's save file.
    /// </summary>
    public void DeleteSave()
    {
        SaveSystem.Delete(saveSlot);
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    private void CollectInventoryData(SaveData data)
    {
        data.inventoryItemIDs.Clear();
        data.inventoryItemAmounts.Clear();

        for (int i = 0; i < inventoryManager.SlotCount; i++)
        {
            InventorySlot slot = inventoryManager.GetSlot(i);

            if (slot == null || slot.IsEmpty())
            {
                data.inventoryItemIDs.Add(string.Empty);
                data.inventoryItemAmounts.Add(0);
            }
            else
            {
                // ưu tiên dùng id, fallback sang itemName
                string key = !string.IsNullOrEmpty(slot.Item.id)
                    ? slot.Item.id
                    : slot.Item.itemName;
                data.inventoryItemIDs.Add(key);
                data.inventoryItemAmounts.Add(slot.Amount);
            }
        }
    }

    private void ApplyInventoryData(SaveData data)
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("[GameSaveManager] ItemDatabase is not assigned. Inventory will not be restored.");
            return;
        }

        inventoryManager.ClearInventory();

        for (int i = 0; i < data.inventoryItemIDs.Count; i++)
        {
            string id = data.inventoryItemIDs[i];
            if (string.IsNullOrEmpty(id)) continue;

            // Tìm theo id trước, fallback sang itemName (cho save file cũ)
            ItemData item = itemDatabase.GetItemById(id)
                         ?? itemDatabase.GetItemByName(id);

            if (item == null)
            {
                Debug.LogWarning("[GameSaveManager] Item not found (id/name): " + id);
                continue;
            }

            // allowAutoEquip = false: không tự equip khi load save
            // (tránh item trong túi bị đẩy ra equipment slot và mất dần)
            inventoryManager.AddItem(item, data.inventoryItemAmounts[i], false);
        }
    }

    private void CollectEquipmentData(SaveData data)
    {
        data.equipmentSlotNames.Clear();
        data.equipmentItemIDs.Clear();

        if (equipmentManager == null) return;

        System.Array slots = System.Enum.GetValues(typeof(EquipmentSlot));
        foreach (EquipmentSlot slot in slots)
        {
            EquipmentItemData item = equipmentManager.GetEquippedItem(slot);
            if (item != null)
            {
                data.equipmentSlotNames.Add(slot.ToString());
                // ưu tiên dùng id, fallback sang itemName
                string key = !string.IsNullOrEmpty(item.id) ? item.id : item.itemName;
                data.equipmentItemIDs.Add(key);
            }
        }

        Debug.Log($"[GameSaveManager] Đã lưu {data.equipmentSlotNames.Count} trang bị.");
    }

    private void ApplyEquipmentData(SaveData data)
    {
        if (equipmentManager == null)
        {
            Debug.LogWarning("[GameSaveManager] EquipmentManager is null, bỏ qua load equipment.");
            return;
        }
        if (itemDatabase == null)
        {
            Debug.LogWarning("[GameSaveManager] ItemDatabase is null, bỏ qua load equipment.");
            return;
        }

        for (int i = 0; i < data.equipmentSlotNames.Count; i++)
        {
            string key = data.equipmentItemIDs[i];
            // Tìm theo id trước, fallback sang itemName
            ItemData raw = itemDatabase.GetItemById(key)
                        ?? itemDatabase.GetItemByName(key);

            if (raw == null)
            {
                Debug.LogWarning("[GameSaveManager] Equipment not found (id/name): " + key);
                continue;
            }

            if (raw is EquipmentItemData equipItem)
            {
                equipmentManager.EquipFromSave(equipItem);
            }
        }

        Debug.Log($"[GameSaveManager] Đã load {data.equipmentSlotNames.Count} trang bị.");
    }

    private void ResolveReferences()
    {
        // Only auto-find if not already assigned via Inspector.
        if (playerStats      == null) playerStats      = FindFirstObjectByType<PlayerStats>();
        if (playerLevel      == null) playerLevel      = FindFirstObjectByType<PlayerLevel>();
        if (playerWallet     == null) playerWallet     = FindFirstObjectByType<PlayerWallet>();
        if (inventoryManager == null) inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (equipmentManager == null) equipmentManager = FindFirstObjectByType<EquipmentManager>();
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (playerStats      == null) { Debug.LogError("[GameSaveManager] Missing PlayerStats.");      valid = false; }
        if (playerLevel      == null) { Debug.LogError("[GameSaveManager] Missing PlayerLevel.");      valid = false; }
        if (playerWallet     == null) { Debug.LogError("[GameSaveManager] Missing PlayerWallet.");     valid = false; }
        if (inventoryManager == null) { Debug.LogError("[GameSaveManager] Missing InventoryManager."); valid = false; }
        // EquipmentManager là optional — chỉ warn, không fail
        if (equipmentManager == null) Debug.LogWarning("[GameSaveManager] EquipmentManager not found, equipment sẽ không được lưu.");

        return valid;
    }
}
