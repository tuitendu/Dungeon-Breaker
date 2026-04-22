using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attacks/Melee Attack")]
public class MeleeAttackSO : AttackBehaviour
{
    public override void ExecuteAttack(GameObject attacker, GameObject target)
    {
        // 1. Lấy chỉ số của người đánh (Slime)
        var myStats = attacker.GetComponent<EnemyStats>();
        if (myStats == null) return;

        var playerStats = target.GetComponent<PlayerStats>();

        if (playerStats != null)
        {

            playerStats.TakeDamage(myStats.Attack);

            Debug.Log($"{attacker.name} đấm cận chiến vào {target.name}!");
        }
    }
}