using UnityEngine;

/// <summary>
/// Vampire tấn công hút máu — gây damage cho player rồi hồi lại một phần HP cho bản thân.
/// Tạo asset: chuột phải Project > Create > Enemy/Attacks/Vampire Attack
/// </summary>
[CreateAssetMenu(menuName = "Enemy/Attacks/Vampire Attack")]
public class VampireAttackSO : AttackBehaviour
{
    [Tooltip("Phần trăm damage gây ra được hút lại thành HP (0–100).")]
    [Range(0f, 100f)]
    public float lifeStealPct = 30f;

    public override void ExecuteAttack(GameObject attacker, GameObject target)
    {
        EnemyStats myStats = attacker.GetComponent<EnemyStats>();
        if (myStats == null) return;

        PlayerStats player = target.GetComponent<PlayerStats>();
        if (player == null) return;

        // 1. Tính damage thực tế sau DEF của player
        int rawDamage   = myStats.Attack;
        int finalDamage = Mathf.Max(rawDamage - player.DEF, 1);

        // 2. Gây damage
        player.TakeDamage(rawDamage); // TakeDamage tự tính DEF bên trong

        // 3. Hút máu: dựa trên finalDamage thực sự trừ đi
        int healAmount = Mathf.RoundToInt(finalDamage * lifeStealPct / 100f);
        if (healAmount > 0)
        {
            myStats.currentHP = Mathf.Min(myStats.currentHP + healAmount, myStats.baseStats.maxHP);
            Debug.Log($"[Vampire] Hút {healAmount} HP (lifesteal {lifeStealPct}% of {finalDamage} dmg).");
        }

        Debug.Log($"[Vampire] Cắn {target.name} — {rawDamage} damage!");
    }
}
