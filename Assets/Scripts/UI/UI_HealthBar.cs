using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [Tooltip("CanvasGroup dùng để làm mờ thanh máu (tự động kiếm nếu để trống)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 5f;

    [Header("Auto Hide Settings")]
    [Tooltip("Bật cái này nếu muốn thanh máu ẩn đi, chỉ hiện khi bị đánh")]
    [SerializeField] private bool autoHide = false;
    [Tooltip("Thời gian hiển thị (giây) sau khi bị đánh")]
    [SerializeField] private float showDuration = 3f;
    [Tooltip("Tốc độ mờ dần/hiện ra")]
    [SerializeField] private float fadeSpeed = 10f;

    private float targetFill = 1f;
    private int currentMax = 100;
    
    private float hideTimer = 0f;
    private float targetAlpha = 1f;

    private void Start()
    {
        if (autoHide)
        {
            // Tự động thêm CanvasGroup nếu gameObject chưa có
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) 
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Vừa vào game là tàng hình ngay lập tức
            canvasGroup.alpha = 0f;
            targetAlpha = 0f;
            hideTimer = 0f;
        }
    }

    public void SetMaxHealth(int health)
    {
        currentMax = health;
        if (fillImage != null && health > 0)
        {
            float fill = (float)fillImage.fillAmount * currentMax;
            targetFill = fill / currentMax;
        }
    }

    public void SetHealth(int health, bool snap = false)
    {
        if (currentMax > 0)
        {
            targetFill = (float)health / currentMax;
            targetFill = Mathf.Clamp01(targetFill);
            if (snap && fillImage != null)
            {
                fillImage.fillAmount = targetFill;
            }

            // ====== LOGIC HIỆN THANH MÁU ======
            if (autoHide)
            {
                if (health < currentMax)
                {
                    // Nếu máu < Max (bị đánh) -> Hiện ra và reset giờ
                    targetAlpha = 1f;
                    hideTimer = showDuration;
                }
                else
                {
                    // Nếu đẩy đủ máu lại -> Bắt đầu màng trình ẩn ngay
                    targetAlpha = 0f;
                    hideTimer = 0f;
                }
            }
        }
    }

    private void Update()
    {
        // 1. Fill ảnh máu trượt từ từ
        if (fillImage != null)
        {
            if (Mathf.Abs(fillImage.fillAmount - targetFill) > 0.001f)
            {
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);
            }
            else
            {
                fillImage.fillAmount = targetFill;
            }
        }

        // 2. Làm mờ / Hiện rõ (Tính năng Ẩn/Hiện)
        if (autoHide && canvasGroup != null)
        {
            // Trừ giờ đợi
            if (hideTimer > 0)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    targetAlpha = 0f; // Hết giờ thì báo hiệu mờ đi
                }
            }

            // Lerp Alpha cho mượt
            if (Mathf.Abs(canvasGroup.alpha - targetAlpha) > 0.01f)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }
            else
            {
                canvasGroup.alpha = targetAlpha;
            }
        }
    }

    private void OnValidate()
    {
        if (fillImage == null)
        {
            Transform fill = transform.Find("Fill");
            if (fill != null)
                fillImage = fill.GetComponent<Image>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
