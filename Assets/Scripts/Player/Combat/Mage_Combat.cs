using UnityEngine;

public class Mage_Combat : Player_CombatBase
{
    [Header("Mage Settings")]
    public float magicRange = 4f;
    public int healAmount = 30;

    [Header("Projectile")]
    public GameObject magicPrefab;
    public Transform firePoint;

    private void Reset()
    {
        basicCooldown = 0.6f;
        skill1Cooldown = 2.5f;
        skill2Cooldown = 5f;
        skill3Cooldown = 8f;
        skill4Cooldown = 15f;
    }

    // ======================================================
    // BASIC ATTACK – BẮN CẦU PHÉP NHỎ
    // ======================================================
    public override bool BasicAttack()
    {
        Collider2D[] hits = GetEnemiesInRange(magicRange);
        if (hits.Length == 0) return false;

        Vector2 dir =
            (hits[0].transform.position - firePoint.position).normalized;

        GameObject magic = Instantiate(
            magicPrefab,
            firePoint.position,
            Quaternion.identity
        );

        magic.GetComponent<Projec_Tile>()
             .Init(dir, stats.MATK, ProjectileType.Normal);

        Debug.Log("Mage: Magic Bolt");
        return true;
    }

    // ======================================================
    // SKILL 1 – FIREBALL (1 PROJECTILE MẠNH)
    // ======================================================
    public override bool Skill1()
    {
        if (!UseMana(25)) return false;

        Collider2D[] hits = GetEnemiesInRange(magicRange);
        if (hits.Length == 0) return false;

        Vector2 dir =
            (hits[0].transform.position - firePoint.position).normalized;

        GameObject fireball = Instantiate(
            magicPrefab,
            firePoint.position,
            Quaternion.identity
        );

        fireball.GetComponent<Projec_Tile>()
                .Init(dir, stats.MATK * 3, ProjectileType.Normal);

        Debug.Log("Mage Skill 1: Fireball");
        return true;
    }

    // ======================================================
    // SKILL 2 – BẮN 3 PHÉP (MULTI SHOT)
    // ======================================================
    public override bool Skill2()
    {
        if (!UseMana(35)) return false;

        Collider2D[] hits = GetEnemiesInRange(magicRange);
        if (hits.Length == 0) return false;

        Vector2 baseDir =
            (hits[0].transform.position - firePoint.position).normalized;

        float[] angles = { -15f, 0f, 15f };

        foreach (float angle in angles)
        {
            Vector2 dir =
                Quaternion.Euler(0, 0, angle) * baseDir;

            GameObject magic = Instantiate(
                magicPrefab,
                firePoint.position,
                Quaternion.identity
            );

            magic.GetComponent<Projec_Tile>()
                 .Init(dir, stats.MATK * 2, ProjectileType.Normal);
        }

        Debug.Log("Mage Skill 2: Multi Magic Shot");
        return true;
    }

    // ======================================================
    // SKILL 3 – HỒI MÁU (KHÔNG PROJECTILE)
    // ======================================================
    public override bool Skill3()
    {
        if (!UseMana(30)) return false;

        stats.Heal(healAmount);

        Debug.Log("Mage Skill 3: Heal");
        return true;
    }

    // ======================================================
    // SKILL 4 – MƯA PHÉP (PROJECTILE RƠI TỪ TRÊN)
    // ======================================================
    public override bool Skill4()
    {
        if (!UseMana(60)) return false;

        Collider2D[] hits = GetEnemiesInRange(magicRange * 2f);
        if (hits.Length == 0) return false;

        foreach (var hit in hits)
        {
            Vector2 spawnPos =
                hit.transform.position +
                Vector3.up * 3f +
                Vector3.right * Random.Range(-1f, 1f);

            GameObject meteor = Instantiate(
                magicPrefab,
                spawnPos,
                Quaternion.identity
            );

            meteor.GetComponent<Projec_Tile>()
                  .Init(Vector2.down, stats.MATK * 4, ProjectileType.Rain);
        }

        Debug.Log("Mage Ultimate: Meteor Rain");
        return true;
    }
}
