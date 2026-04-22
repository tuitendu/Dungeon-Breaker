using UnityEngine;

/// <summary>
/// Gắn script này vào Prefab của Enemy (cùng chỗ với EnemyPatrol)
/// Script này tạo ra một lực đẩy nhẹ giữa các Enemy với nhau để chúng không chụm lại thành 1 cục
/// </summary>
public class EnemySeparation : MonoBehaviour
{
    [Tooltip("Bán kính phát hiện các con quái khác xung quanh")]
    public float separationRadius = 0.8f;

    [Tooltip("Lực đẩy dạt ra (càng cao đẩy càng mạnh)")]
    public float separationForce = 1.5f;

    [Tooltip("Layer chứa Enemy (thường là Default hoặc Enemy)")]
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Pathfinding.AIPath aiPath;

    private static Collider2D[] results = new Collider2D[10]; // Dùng chung mảng cấp phát sẵn, Zero GC
    private float checkTimer = 0f;
    private const float CHECK_INTERVAL = 0.15f; // Không cần quét vật lý mỗi frame (0.15s/lần là đủ)

    // Lưu lại target push từ frame trước để làm mượt
    private Vector2 currentPushVelocity = Vector2.zero; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aiPath = GetComponent<Pathfinding.AIPath>();
    }

    private void Update() 
    {
        checkTimer += Time.deltaTime;
        
        // Cứ 0.15 giây mới quét Collider xung quanh 1 lần để tối ưu Game
        if (checkTimer >= CHECK_INTERVAL)
        {
            checkTimer = 0f;
            CalculateSeparation();
        }

        if (currentPushVelocity != Vector2.zero && aiPath != null)
        {
            // Tránh xuyên tường bằng cách truyền vận tốc đẩy sang A* Pathfinding
            // AIPath sẽ thu thập lực này và thực hiện di chuyển một cách đồng bộ không bị rách frame vật lý.
            aiPath.Move(currentPushVelocity * Time.deltaTime);
            
            // Giảm dần lực (Damping)
            currentPushVelocity = Vector2.Lerp(currentPushVelocity, Vector2.zero, Time.deltaTime * 5f);
        }
    }

    private void CalculateSeparation()
    {
        // Dùng NonAlloc -> KHÔNG GÂY RÁC BỘ NHỚ (No GC Spikes)
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, separationRadius, results, enemyLayer);
        
        Vector2 separationVector = Vector2.zero;
        int count = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = results[i];

            // Bỏ qua chính bản thân nó
            if (hit.gameObject != this.gameObject && hit.CompareTag(this.tag))
            {
                // Tính toán hướng đẩy ra xa con quái kia
                Vector2 direction = transform.position - hit.transform.position;
                
                // Càng ở gần thì lực đẩy càng mạnh
                float distance = direction.magnitude;
                if (distance > 0 && distance < separationRadius)
                {
                    separationVector += (direction.normalized / distance);
                    count++;
                }
                else if (distance == 0)
                {
                    // Đang vô tình đứng lồng đúng y xì toạ độ 0,0 của nhau -> đẩy random ra
                    Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                    separationVector += randomDir * 2f;
                    count++;
                }
            }
        }

        // Áp dụng lực dạt ra cho Transform
        if (count > 0)
        {
            separationVector /= count; // Chia trung bình
            currentPushVelocity = separationVector * separationForce;
        }
    }

    // Vẽ vòng tròn vàng trong Editor để dễ chỉnh bán kính
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
