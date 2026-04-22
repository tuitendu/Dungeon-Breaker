using UnityEngine;

public abstract class Player_CombatBase : MonoBehaviour
{

    protected PlayerStats stats;

    [Header("Target")]
    public LayerMask enemyLayer;

    protected virtual void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (stats == null)
            Debug.LogError("Thiếu PlayerStats!");
    }
    [Header("Cooldown Settings")]
    public float basicCooldown = 0.8f;
    public float skill1Cooldown = 2f;
    public float skill2Cooldown = 5f;
    public float skill3Cooldown = 8f;
    public float skill4Cooldown = 15f;
    public abstract bool BasicAttack();
    public abstract bool Skill1();
    public abstract bool Skill2();
    public abstract bool Skill3();
    public abstract bool Skill4();



    protected Collider2D[] GetEnemiesInRange(float range)
    {
        return Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
    }

    protected bool UseMana(int amount)
    {
        if (stats.currentMana < amount)
        {
            Debug.Log("Không đủ mana!");
            return false;
        }

        stats.DecreaseMana(amount);
        return true;
    }
}
