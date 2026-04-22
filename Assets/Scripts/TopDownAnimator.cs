using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(Animator))]
public class TopDownAnimator : MonoBehaviour
{
    private AIPath aiPath;
    private Animator anim;

    private Vector2 lastMoveDir = Vector2.down;

    // Thoi gian cho truoc khi chuyen sang Idle (tranh nhap nhay khi A* tinh lai duong)
    [SerializeField] private float idleDelay = 0.15f;
    private float idleTimer = 0f;
    private bool wasMoving = false;

    private EnemyStats enemyStats;

    private void Awake()
    {
        aiPath      = GetComponent<AIPath>();
        anim        = GetComponent<Animator>();
        enemyStats  = GetComponent<EnemyStats>();
    }

    // Nguong de chuyen huong: truc nay phai > truc kia it nhat [threshold]%
    [SerializeField] private float directionThreshold = 0.3f;

    private void Update()
    {
        // ── Không update khi đã chết ──────────────────────────
        if (enemyStats != null && enemyStats.IsDead) return;
        if (!aiPath.enabled) return;  // aiPath bị tắt khi chết

        Vector2 velocity = aiPath.velocity;
        bool isMovingNow = velocity.sqrMagnitude > 0.1f;

        if (isMovingNow)
        {
            // Neu dang trong trang thai tan cong -> bo qua, de EnemyAttackController xu ly
            if (anim.GetBool("IsAttacking")) return;

            idleTimer = 0f;
            wasMoving = true;

            anim.SetBool("IsMoving", true);

            float absX = Mathf.Abs(velocity.x);
            float absY = Mathf.Abs(velocity.y);

            // Gán cả 2 trục độc lập. Nếu vận tốc trên trục đủ lớn thì coi như có đi hướng đó (cho phép đi chéo 8 hướng)
            float moveX = (absX > 0.1f) ? Mathf.Sign(velocity.x) : 0f;
            float moveY = (absY > 0.1f) ? Mathf.Sign(velocity.y) : 0f;

            lastMoveDir = new Vector2(moveX, moveY);
            if (lastMoveDir == Vector2.zero) lastMoveDir = Vector2.down; // Mac dinh quay xuong

            anim.SetFloat("MoveX", lastMoveDir.x);
            anim.SetFloat("MoveY", lastMoveDir.y);
        }
        else
        {
            if (wasMoving)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleDelay)
                {
                    wasMoving = false;
                    anim.SetBool("IsMoving", false);
                }
                anim.SetFloat("MoveX", lastMoveDir.x);
                anim.SetFloat("MoveY", lastMoveDir.y);
            }
            else
            {
                anim.SetFloat("MoveX", lastMoveDir.x);
                anim.SetFloat("MoveY", lastMoveDir.y);
            }
        }
    }
}