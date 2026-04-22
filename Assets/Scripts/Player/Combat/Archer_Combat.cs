using UnityEngine;

public class Archer_Combat : Player_CombatBase
{
    [Header("Archer Settings")]
    public float shootRange = 6f;
    public GameObject arrowPrefab;
    public Transform firePoint;

    private void Reset()
    {
        basicCooldown = 0.4f;
        skill1Cooldown = 2f;
        skill2Cooldown = 4f;
        skill3Cooldown = 7f;
        skill4Cooldown = 12f;
    }

    // ================== CORE SHOOT ==================
    private void Shoot(Vector2 dir, int dmg, ProjectileType type)
    {
        if (arrowPrefab == null || firePoint == null)
        {
            Debug.LogError("Thiếu Arrow Prefab hoặc FirePoint");
            return;
        }

        GameObject arrow = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        arrow.GetComponent<Projec_Tile>()
             .Init(dir, dmg, type);
    }

    // ================== BASIC ==================
    public override bool BasicAttack()
    {
        Collider2D[] hits = GetEnemiesInRange(shootRange);
        if (hits.Length == 0) return false;

        Vector2 dir =
            (hits[0].transform.position - firePoint.position).normalized;

        Shoot(dir, stats.ATK, ProjectileType.Normal);
        return true;
    }

    // ================== SKILL 1: BẮN 3 MŨI ==================
    public override bool Skill1()
    {
        if (!UseMana(15)) return false;

        float[] angles = { -15f, 0f, 15f };

        foreach (float angle in angles)
        {
            Vector2 dir =
                Quaternion.Euler(0, 0, angle) * transform.right;

            Shoot(dir, stats.ATK, ProjectileType.Normal);
        }

        return true;
    }

    // ================== SKILL 2: BẮN XUYÊN ==================
    public override bool Skill2()
    {
        if (!UseMana(20)) return false;

        Shoot(transform.right,
              stats.ATK * 2,
              ProjectileType.Pierce);

        return true;
    }

    // ================== SKILL 3: MƯA TÊN ==================
    public override bool Skill3()
    {
        if (!UseMana(30)) return false;

        for (int i = 0; i < 6; i++)
        {
            Vector2 spawnPos =
                (Vector2)firePoint.position +
                new Vector2(Random.Range(-2f, 2f), 3f);

            GameObject arrow = Instantiate(
                arrowPrefab,
                spawnPos,
                Quaternion.identity
            );

            arrow.GetComponent<Projec_Tile>()
                 .Init(Vector2.down,
                       stats.ATK,
                       ProjectileType.Rain);
        }

        return true;
    }

    // ================== SKILL 4: TUYỆT XẠ ==================
    public override bool Skill4()
    {
        if (!UseMana(50)) return false;

        Shoot(transform.right,
              Mathf.RoundToInt(stats.ATK * 5f),
              ProjectileType.Ultimate);

        return true;
    }
}
