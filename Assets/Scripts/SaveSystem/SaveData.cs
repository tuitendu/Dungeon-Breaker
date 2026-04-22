using System.Collections.Generic;

/// <summary>
/// Plain data container. All fields here will be written to / read from the save file.
/// Add new fields here when you need to persist more data.
/// </summary>
[System.Serializable]
public class SaveData
{
    // ---- PlayerLevel ----
    public int level      = 1;
    public int currentExp = 0;

    // ---- PlayerWallet ----
    public int gold = 0;

    // ---- PlayerStats (runtime values only, max values are recalculated from level) ----
    public int currentHP   = 0;
    public int currentMana = 0;

    // ---- World position ----
    public float playerPosX = 0f;
    public float playerPosY = 0f;
    public string lastMapID = "MainMap";

    // ---- Inventory ----
    // Parallel lists: inventoryItemIDs[i] pairs with inventoryItemAmounts[i].
    public List<string> inventoryItemIDs     = new List<string>();
    public List<int>    inventoryItemAmounts = new List<int>();

    // ---- Equipment (trang bị đang mặc) ----
    // Parallel lists: equipmentSlotNames[i] pairs with equipmentItemIDs[i].
    // equipmentSlotNames stores the EquipmentSlot enum name (e.g. "Weapon", "Armor").
    public List<string> equipmentSlotNames = new List<string>();
    public List<string> equipmentItemIDs   = new List<string>();
}
