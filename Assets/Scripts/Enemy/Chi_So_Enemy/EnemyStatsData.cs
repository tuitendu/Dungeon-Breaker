using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStatsData",
    menuName = "Enemy/Stats Data"
)]
public class EnemyStatsData : ScriptableObject
{
    [Header("Base Stats")]
    public int maxHP;
    public int attack;
    public int defense;

    [Header("Movement")]
    public float moveSpeed;

    [Header("Combat")]
    public float attackRange;
    public float attackCooldown;

    [Header("Rewards")]
    [Tooltip("Kinh nghiệm nhận được khi giết enemy này.")]
    public int expReward = 5;

    [Tooltip("Tỉ lệ rơi đồ tổng (0-100%). Dùng cho DropTable nếu cần override.")]
    [Range(0f, 100f)]
    public float dropChance = 100f;
}
