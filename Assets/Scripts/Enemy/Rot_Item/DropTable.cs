using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Drop Table")]
public class DropTable : ScriptableObject
{
    [Header("Gold Drop")]
    [Range(0f, 1f)] public float goldChance = 1f;
    public int goldMin = 1;
    public int goldMax = 3;

    [Header("EXP Drop")]
    [Range(0f, 1f)] public float expChance = 1f;
    public int expMin = 1;
    public int expMax = 5;

    [System.Serializable]
    public class ItemDrop
    {
        public ItemData item;
        [Tooltip("Prefab riêng cho item này. Để trống → dùng Default Item Prefab của EnemyDropper")]
        public Item_Roi prefab;
        [Range(0f, 1f)] public float chance = 0.2f;
        public int min = 1;
        public int max = 1;
    }

    [Header("Item Drops")]
    public ItemDrop[] itemDrops;

    public bool TryRollGold(out int gold)
    {
        gold = 0;
        if (Random.value > goldChance) return false;

        gold = Random.Range(goldMin, goldMax + 1);
        return gold > 0;
    }

    public bool TryRollExp(out int exp)
    {
        exp = 0;
        if (Random.value > expChance) return false;

        exp = Random.Range(expMin, expMax + 1);
        return exp > 0;
    }

    /// <summary>
    /// Roll từng item độc lập → trả về list tất cả item pass chance
    /// </summary>
    public List<ItemDrop> RollAllItems()
    {
        var results = new List<ItemDrop>();

        if (itemDrops == null) return results;

        foreach (var drop in itemDrops)
        {
            if (drop.item == null) continue;
            if (Random.value <= drop.chance)
                results.Add(drop);
        }

        return results;
    }
}
