using UnityEngine;

public class EnemyDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    public DropTable dropTable;

    [Header("Prefabs")]
    public Nhat_Vang goldPrefab;
    public Nhat_Exp expPrefab;
    [Tooltip("Prefab dùng chung khi item trong DropTable không có prefab riêng")]
    public Item_Roi defaultItemPrefab;

    [Header("Scatter")]
    public float scatterRadius = 0.4f;
    public float explosionForce = 3f;
    public float upwardModifier = 0.5f;

    public void Drop()
    {
        if (dropTable == null) return;

        // 1. Rơi vàng
        DropGold();

        // 2. Rơi EXP
        DropExp();

        // 3. Rơi item (tất cả enemy, tùy DropTable)
        DropItems();
    }

    private void DropGold()
    {
        if (goldPrefab == null) return;
        if (!dropTable.TryRollGold(out int totalGold)) return;

        for (int i = 0; i < totalGold; i++)
        {
            var p = ScatterPos(transform.position);
            Nhat_Vang g = Instantiate(goldPrefab, p, Quaternion.identity);
            g.amount = 1;
            ApplyExplosiveForce(g.gameObject);
        }
    }

    private void DropExp()
    {
        if (expPrefab == null) return;
        if (!dropTable.TryRollExp(out int totalExp)) return;

        for (int i = 0; i < totalExp; i++)
        {
            var p = ScatterPos(transform.position);
            Nhat_Exp exp = Instantiate(expPrefab, p, Quaternion.identity);
            exp.expAmount = 1;
            ApplyExplosiveForce(exp.gameObject);
        }
    }

    private void DropItems()
    {
        var rolledItems = dropTable.RollAllItems();

        foreach (var rolled in rolledItems)
        {
            // Ưu tiên prefab riêng của item, không có thì dùng default
            Item_Roi prefabToUse = rolled.prefab != null ? rolled.prefab : defaultItemPrefab;

            if (prefabToUse == null)
            {
                Debug.LogWarning($"[EnemyDropper] Không có prefab cho item {rolled.item.itemName}!");
                continue;
            }

            var p = ScatterPos(transform.position);
            Item_Roi drop = Instantiate(prefabToUse, p, Quaternion.identity);
            drop.item = rolled.item;
            drop.amount = Random.Range(rolled.min, rolled.max + 1);
            ApplyExplosiveForce(drop.gameObject);
        }
    }

    private Vector3 ScatterPos(Vector3 center)
    {
        return center + (Vector3)Random.insideUnitCircle * scatterRadius;
    }

    private void ApplyExplosiveForce(GameObject obj)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            direction.y = Mathf.Abs(direction.y) + upwardModifier;
            rb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
        }
    }
}
