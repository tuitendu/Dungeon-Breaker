using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_GoldDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Text goldTextLegacy; // Fallback cho Unity UI Text cũ

    [Header("Animation")]
    [SerializeField] private bool animateOnChange = true;
    [SerializeField] private float animationDuration = 0.3f;

    private int displayedGold = 0;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        // Kiểm tra UI reference
        if (goldText == null && goldTextLegacy == null)
        {
            Debug.LogError("UI_GoldDisplay: Chưa gán Text component!", this);
        }
    }

    private void OnEnable()
    {
        // Đăng ký event từ PlayerWallet (nếu Instance đã có)
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnGoldChanged -= OnGoldChanged; // Tránh đăng ký 2 lần
            PlayerWallet.Instance.OnGoldChanged += OnGoldChanged;

            // Hiển thị giá trị ban đầu
            OnGoldChanged(PlayerWallet.Instance.CurrentGold);
        }
    }

    private void Start()
    {
        // Thử đăng ký lại ở Start (chắc chắn PlayerWallet.Awake đã chạy)
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnGoldChanged -= OnGoldChanged; // Tránh đăng ký 2 lần
            PlayerWallet.Instance.OnGoldChanged += OnGoldChanged;

            // Hiển thị giá trị ban đầu
            OnGoldChanged(PlayerWallet.Instance.CurrentGold);
        }
        else
        {
            Debug.LogWarning("PlayerWallet.Instance vẫn chưa tồn tại trong UI_GoldDisplay Start!");
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh memory leak
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnGoldChanged -= OnGoldChanged;
        }
    }

    private void OnGoldChanged(int newGold)
    {
        if (animateOnChange)
        {
            // Animation đếm số
            StartCoroutine(AnimateGoldChange(displayedGold, newGold));

            // Kích hoạt animation (nếu có Animator)
            if (anim != null)
            {
                anim.SetTrigger("GoldChanged");
            }
        }
        else
        {
            // Cập nhật trực tiếp
            displayedGold = newGold;
            UpdateText(newGold);
        }
    }

    private System.Collections.IEnumerator AnimateGoldChange(int from, int to)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Lerp với easing
            int currentGold = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            UpdateText(currentGold);

            yield return null;
        }

        // Đảm bảo giá trị cuối cùng
        displayedGold = to;
        UpdateText(to);
    }

    private void UpdateText(int gold)
    {
        string formattedGold = FormatGold(gold);

        // Cập nhật TextMeshPro
        if (goldText != null)
        {
            goldText.text = formattedGold;
        }

        // Cập nhật Legacy Text
        if (goldTextLegacy != null)
        {
            goldTextLegacy.text = formattedGold;
        }
    }

    private string FormatGold(int gold)
    {
        // Format với dấu phẩy: 1,234,567
        return $"{gold:N0}";
    }

    // ===== DEBUG =====
    private void OnValidate()
    {
        // Auto-find text component nếu chưa gán
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }

        if (goldTextLegacy == null)
        {
            goldTextLegacy = GetComponent<Text>();
        }
    }
}
