using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ExpBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;

    [Header("Animation")]
    [SerializeField] private float fillSpeed = 3f;
    [SerializeField] private bool animateLevelUp = true;

    [Header("Colors (Optional)")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color levelUpColor = Color.yellow;

    private float targetFill = 0f;
    private float currentFill = 0f;
    private bool isLevelingUp = false;

    private void Awake()
    {
        // Validate
        if (fillImage == null)
        {
            Debug.LogError("UI_ExpBar: Fill Image chưa gán!", this);
        }
    }

    private bool isRegistered = false;

    private void Start()
    {
        // Start luôn chạy sau tất cả các Awake(), đảm bảo PlayerLevel.Instance đã có mặt
        TryRegisterEvents();
    }

    private void OnEnable()
    {
        TryRegisterEvents();
    }

    private void TryRegisterEvents()
    {
        if (isRegistered) return;

        if (PlayerLevel.Instance != null)
        {
            PlayerLevel.Instance.OnExpChanged += UpdateExpBar;
            PlayerLevel.Instance.OnLevelUp += OnLevelUp;

            // Cập nhật giá trị ban đầu
            UpdateExpBar(PlayerLevel.Instance.CurrentExp);
            if (fillImage != null)
            {
                fillImage.fillAmount = targetFill;
                currentFill = targetFill;
            }
            
            isRegistered = true;
        }
    }

    private void OnDisable()
    {
        if (PlayerLevel.Instance != null)
        {
            PlayerLevel.Instance.OnExpChanged -= UpdateExpBar;
            PlayerLevel.Instance.OnLevelUp -= OnLevelUp;
        }
        isRegistered = false;
    }

    private void UpdateExpBar(int currentExp)
    {
        if (PlayerLevel.Instance == null) return;

        int expToNext = PlayerLevel.Instance.ExpToNextLevel;

        if (expToNext == 0) // Max level
        {
            targetFill = 1f;
        }
        else
        {
            targetFill = (float)currentExp / expToNext;
        }

        // Bỏ qua Lerp, khóa cứng UI ở vị trí đúng ngay khi màn chơi vừa Load (khoảng 1 giây đầu)
        if (Time.timeSinceLevelLoad < 1f)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = targetFill;
                currentFill = targetFill;
            }
        }
    }

    private int levelUpQueue = 0;

    private void OnLevelUp(int newLevel)
    {
        // Chặn hoàn toàn animation LevelUp chớp vàng nếu màn chơi vừa Load (Data đắp vào)
        if (Time.timeSinceLevelLoad < 1f) return;

        if (animateLevelUp)
        {
            levelUpQueue++;
            if (!isLevelingUp)
            {
                StartCoroutine(ProcessLevelUpQueue());
            }
        }
        else
        {
            // Reset fill về 0 ngay lập tức
            currentFill = 0f;
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
            }
        }
    }

    private System.Collections.IEnumerator ProcessLevelUpQueue()
    {
        isLevelingUp = true;

        while (levelUpQueue > 0)
        {
            levelUpQueue--;

            // Đợi thanh exp trượt đầy lên 100%
            while (Mathf.Abs(currentFill - 1f) > 0.005f)
            {
                currentFill = Mathf.Lerp(currentFill, 1f, fillSpeed * Time.deltaTime);
                if (fillImage != null) fillImage.fillAmount = currentFill;
                yield return null;
            }

            // Fill to 100% chắc chắn
            if (fillImage != null)
            {
                fillImage.fillAmount = 1f;
            }

            // Flash color
            if (fillImage != null)
            {
                Color original = fillImage.color;
                fillImage.color = levelUpColor;

                yield return new WaitForSeconds(0.3f);

                fillImage.color = original;
            }

            // Reset fill
            currentFill = 0f;
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
            }
        }

        isLevelingUp = false;
    }

 

    private void Update()
    {
        // Smooth fill animation
        if (fillImage != null && !isLevelingUp)
        {
            if (Mathf.Abs(currentFill - targetFill) > 0.001f)
            {
                currentFill = Mathf.Lerp(currentFill, targetFill, fillSpeed * Time.deltaTime);
                fillImage.fillAmount = currentFill;
            }
        }
    }

    // ===== VALIDATION =====
    private void OnValidate()
    {
        // Auto-find fill image nếu chưa gán
        if (fillImage == null)
        {
            Transform fill = transform.Find("Fill");
            if (fill != null)
                fillImage = fill.GetComponent<Image>();
        }
    }
}
