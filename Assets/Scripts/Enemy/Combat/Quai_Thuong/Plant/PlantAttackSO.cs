using UnityEngine;

/// <summary>
/// Plant tấn công AOE — bắn gai xung quanh bản thân, gây damage cho player trong vùng.
/// Tạo asset: chuột phải Project > Create > Enemy/Attacks/Plant Attack
/// </summary>
[CreateAssetMenu(menuName = "Enemy/Attacks/Plant Attack")]
public class PlantAttackSO : AttackBehaviour
{
    [Tooltip("Bán kính vùng gai (đơn vị Unity). Phải >= attackRange trong EnemyStats.")]
    public float splashRadius = 1.2f;

    [Tooltip("Layer của Player để OverlapCircle nhận diện đúng.")]
    public LayerMask playerLayer;

    public override void ExecuteAttack(GameObject attacker, GameObject target)
    {
        EnemyStats myStats = attacker.GetComponent<EnemyStats>();
        if (myStats == null) return;

        // Tìm tất cả collider trong vùng tròn quanh Plant
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attacker.transform.position,
            splashRadius,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerStats player = hit.GetComponent<PlayerStats>();
            if (player != null)
            {
                player.TakeDamage(myStats.Attack);
                Debug.Log($"[Plant] AOE gai trúng {hit.name} — {myStats.Attack} damage!");
            }
        }
    }

    // Hiển thị vùng AOE trong Scene View
    private void OnDrawGizmosSelected() { }  // placeholder — Gizmos chỉ dùng được trên MonoBehaviour
}
