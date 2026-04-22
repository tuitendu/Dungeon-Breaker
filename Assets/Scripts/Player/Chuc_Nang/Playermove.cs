using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Joystick joystick;

    private Rigidbody2D rb;
    private PlayerStats  stats;
    private Animator     anim;
    private PlayerAudio  _audio;

    private Vector2 movement;
    private Vector2 lastMoveDir = Vector2.down; // mặc định nhìn xuống (Idle_Front)

    public Vector2 MoveInput => movement;

    private void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        stats  = GetComponent<PlayerStats>();
        anim   = GetComponent<Animator>();
        _audio = GetComponent<PlayerAudio>();
    }

    private void Update()
    {
        // ── Không xử lý input khi đã chết ──────────────────────
        if (stats != null && stats.IsDead)
        {
            movement = Vector2.zero;
            return;
        }

        if (joystick == null) return;

        movement = new Vector2(
            joystick.Horizontal,
            joystick.Vertical
        );

        if (movement.sqrMagnitude > 1)
            movement.Normalize();

        // ── Animation ─────────────────────────────────
        UpdateAnimation();

        // ── Footstep ──────────────────────────────────
        bool isMoving = movement.sqrMagnitude > 0.01f;
        if (isMoving) _audio?.TickFootstep();
        else          _audio?.StopFootstep();
    }

    private void FixedUpdate()
    {
        // ── Không di chuyển khi đã chết ────────────────────────
        if (stats != null && stats.IsDead) return;

        rb.MovePosition(
            rb.position + movement * stats.speed * Time.fixedDeltaTime
        );
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;

        // ── Không override animation khi đang chết ──────────────
        if (anim.GetBool("IsDead")) return;

        bool isMoving = movement.sqrMagnitude > 0.01f;
        anim.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            // Ưu tiên trục ngang hay dọc
            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y))
            {
                // Đi ngang
                anim.SetFloat("MoveX", Mathf.Sign(movement.x));
                anim.SetFloat("MoveY", 0f);
                lastMoveDir = new Vector2(Mathf.Sign(movement.x), 0f);
            }
            else
            {
                // Đi dọc
                anim.SetFloat("MoveX", 0f);
                anim.SetFloat("MoveY", Mathf.Sign(movement.y));
                lastMoveDir = new Vector2(0f, Mathf.Sign(movement.y));
            }
        }
        else
        {
            // Đứng yên: giữ hướng cuối để idle đúng hướng
            anim.SetFloat("MoveX", lastMoveDir.x);
            anim.SetFloat("MoveY", lastMoveDir.y);
        }
    }

    // ── Hàm hỗ trợ để ép Player nhìn về một hướng (dùng khi tấn công) ──
    public void FaceDirection(Vector2 direction)
    {
        if (anim == null) return;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            lastMoveDir = new Vector2(Mathf.Sign(direction.x), 0f);
        }
        else
        {
            lastMoveDir = new Vector2(0f, Mathf.Sign(direction.y));
        }

        anim.SetFloat("MoveX", lastMoveDir.x);
        anim.SetFloat("MoveY", lastMoveDir.y);
    }
}
