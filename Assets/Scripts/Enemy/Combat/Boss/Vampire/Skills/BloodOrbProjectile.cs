using UnityEngine;

/// <summary>
/// Projectile cho Skill 1 &amp; Skill 3 của Boss.
/// Bay theo hướng được gán, animation bằng Animator gắn trên prefab (Fire sprite).
/// Tự huỷ khi: chạm player (gây dame) | bay quá maxRange | hết lifetime.
/// </summary>
public class BloodOrbProjectile : MonoBehaviour
{
    [HideInInspector] public Vector2 direction = Vector2.right;
    [HideInInspector] public float   speed     = 6f;
    [HideInInspector] public int     damage    = 10;
    [HideInInspector] public float   lifetime  = 4f;

    [Tooltip("Khoảng cách tối đa bay từ vị trí spawn. Vượt quá → tự hủy (tránh lag). 0 = không giới hạn.")]
    [HideInInspector] public float   maxRange  = 15f;

    private bool    initialized  = false;
    private Vector3 spawnPos;

    void Awake()
    {
        // TỰ ĐỘNG FIX LỖI LAG VẬT LÝ VÀ DROP FPS:
        // Đạn có Collider mà không có Rigidbody khi di chuyển sẽ làm Unity build lại Physics Tree liên tục.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Kinematic để không bị rớt do trọng lực
            rb.simulated = true;
        }
    }

    void Start()
    {
        initialized = true;
        spawnPos    = transform.position;

        // Xoay sprite theo hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!initialized) return;

        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        // Tự hủy khi bay quá maxRange
        if (maxRange > 0f && Vector3.Distance(transform.position, spawnPos) >= maxRange)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null) ps.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, (Vector3)direction.normalized * 1.5f);

        // Vùng range tối đa
        if (maxRange > 0f)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxRange);
        }
    }
}

