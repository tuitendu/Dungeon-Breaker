using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Canvas canvas; // Canvas World Space chứa thanh máu

    [Header("Settings")]
    [SerializeField] private float fillSpeed = 5f;
    [SerializeField] private bool hideWhenFull = true;

    private EnemyStats enemyStats;
    private float targetFill = 1f;

    private void Awake()
    {
        // Tự động tìm EnemyStats ở thằng cha (con quái vật)
        enemyStats = GetComponentInParent<EnemyStats>();

        if (canvas == null)
            canvas = GetComponent<Canvas>();
            
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (enemyStats != null && enemyStats.baseStats != null && fillImage != null)
        {
            targetFill = (float)enemyStats.currentHP / enemyStats.baseStats.maxHP;
            fillImage.fillAmount = targetFill; // Snap lúc đầu
        }
    }

    private void LateUpdate()
    {
        if (enemyStats == null || enemyStats.baseStats == null || fillImage == null) return;

        // Cập nhật target dựa trên máu hiện tại của quái
        targetFill = (float)enemyStats.currentHP / enemyStats.baseStats.maxHP;
        targetFill = Mathf.Clamp01(targetFill);

        // Smooth Lerp animation
        if (Mathf.Abs(fillImage.fillAmount - targetFill) > 0.001f)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);
        }
        else
        {
            fillImage.fillAmount = targetFill;
        }

        // --- Logic Ẩn/Hiện thanh máu ---
        if (canvas != null)
        {
            bool shouldShow = true;

            // 1. Quái chết -> Ẩn
            if (enemyStats.IsDead)
            {
                shouldShow = false;
            }
            // 2. Máu đầy -> Ẩn (để đỡ rối mắt khi chưa bị đánh)
            else if (hideWhenFull && enemyStats.currentHP >= enemyStats.baseStats.maxHP)
            {
                shouldShow = false;
            }

            // Bật/tắt component Canvas để tối ưu hiệu năng (tốt hơn SetActive GameObject)
            canvas.enabled = shouldShow;
        }
    }
}
