using UnityEngine;
using Pathfinding;

public class EnemyAttackController : MonoBehaviour
{
    [Header("SETTING")]
    public AttackBehaviour attackStrategy;

    [Header("Hitbox Mode")]
    [Tooltip("Tick vào nếu enemy dùng hitbox + Animation Event thay vì damage tức thì.\n" +
             "Khi tick, PerformAttack() chỉ trigger animation, EnemyHitbox tự lo damage.")]
    public bool useHitbox = false;

    [Header("Target")]
    public Transform targetPlayer;

    [Header("Range Smoothing")]
    public float exitRangeBonus = 0.15f;

    [Header("Leash Settings")]
    [Tooltip("Khoang cach toi da tu vi tri spawn, qua xa se ngung duoi player")]
    public float maxLeashDistance = 15f;

    private EnemyStats stats;
    private AIPath aiPath;
    private Animator anim;
    private Collider2D enemyCol;
    private Collider2D playerCol;
    private EnemyHitbox hitbox; // null nếu không dùng hitbox mode

    private float nextAttackTime = 0f;
    private bool inAttackRange;
    private Vector3 homePosition;
    public bool isLeashed = false;

    void Start()
    {
        stats   = GetComponent<EnemyStats>();
        aiPath  = GetComponent<AIPath>();
        anim    = GetComponent<Animator>();
        enemyCol = GetComponent<Collider2D>();

        homePosition = transform.position;

        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                targetPlayer = playerObj.transform;
        }

        if (targetPlayer != null)
            playerCol = targetPlayer.GetComponent<Collider2D>();

        // Tìm EnemyHitbox trong toàn bộ children (kể cả nested)
        hitbox = GetComponentInChildren<EnemyHitbox>(includeInactive: true);
        if (hitbox != null)
        {
            // Inject reference để hitbox biết dùng stats & strategy nào
            hitbox.ownerStats    = stats;
            hitbox.ownerObject   = gameObject;
            hitbox.attackStrategy = attackStrategy;

            if (!useHitbox)
                Debug.LogWarning($"[{name}] Tìm thấy EnemyHitbox nhưng useHitbox = false. " +
                                 "Tick useHitbox trong Inspector để bật hitbox mode.");
        }
        else if (useHitbox)
        {
            Debug.LogWarning($"[{name}] useHitbox = true nhưng không tìm thấy EnemyHitbox trong children!");
        }
    }

    void Update()
    {
        if (targetPlayer == null) return;

        float distFromHome = Vector2.Distance(transform.position, homePosition);
        if (distFromHome > maxLeashDistance)
        {
            isLeashed     = true;
            inAttackRange = false;
            aiPath.isStopped = false;   // cho phép di chuyển về nhà
            aiPath.canMove = true;
            return;
        }
        else isLeashed = false;

        float range     = stats.AttackRange;
        float distance  = GetRealDistanceToTarget();
        float enterRange = range;
        float exitRange  = range + exitRangeBonus;

        if (!inAttackRange && distance <= enterRange) inAttackRange = true;
        if (inAttackRange  && distance >= exitRange)  inAttackRange = false;

        aiPath.canMove = !inAttackRange;
        // isStopped = true buộc velocity = 0 NGAY LẬP TỨC
        // (canMove = false chỉ ngăn di chuyển mới, nhưng velocity cũ vẫn còn vài frame)
        aiPath.isStopped = inAttackRange;
        anim.SetBool("IsAttacking", inAttackRange);

        if (inAttackRange)
        {
            anim.SetBool("IsMoving", false);
            FaceTarget();

            if (Time.time >= nextAttackTime)
            {
                // Nếu đang bị đánh (Hurt) thì không thể ra chiêu
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt") || 
                    anim.GetNextAnimatorStateInfo(0).IsName("Hurt")) return;

                PerformAttack();
                nextAttackTime = Time.time + stats.AttackCooldown;
            }
        }
    }

    float GetRealDistanceToTarget()
    {
        if (enemyCol != null && playerCol != null)
        {
            Vector2 p1 = enemyCol.ClosestPoint(targetPlayer.position);
            Vector2 p2 = playerCol.ClosestPoint(transform.position);
            return Vector2.Distance(p1, p2);
        }
        return Vector2.Distance(transform.position, targetPlayer.position);
    }

    void PerformAttack()
    {
        // Luôn trigger animation
        if (anim != null && attackStrategy != null)
            anim.SetTrigger(attackStrategy.animTrigger);

        // Chỉ gọi ExecuteAttack trực tiếp khi KHÔNG dùng hitbox mode
        // (hitbox mode: damage được xử lý bởi EnemyHitbox qua Animation Event)
        if (!useHitbox && attackStrategy != null)
            attackStrategy.ExecuteAttack(gameObject, targetPlayer.gameObject);
    }

    void FaceTarget()
    {
        if (anim == null) return;

        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        float moveX = (Mathf.Abs(direction.x) > 0.1f) ? Mathf.Sign(direction.x) : 0f;
        float moveY = (Mathf.Abs(direction.y) > 0.1f) ? Mathf.Sign(direction.y) : 0f;

        anim.SetFloat("MoveX", moveX);
        anim.SetFloat("MoveY", moveY);
    }
}
