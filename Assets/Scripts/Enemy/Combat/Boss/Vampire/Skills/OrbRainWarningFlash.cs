using UnityEngine;

/// <summary>
/// Gắn lên prefab OrbRainWarning.
/// Làm nhấp nháy alpha để báo hiệu vị trí orb sắp rơi.
/// Không gây damage — chỉ là visual.
/// </summary>
public class OrbRainWarningFlash : MonoBehaviour
{
    [Tooltip("Tốc độ nhấp nháy (càng cao càng nhanh).")]
    public float flashSpeed = 5f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Tạo placeholder nếu chưa có sprite
        if (sr != null && sr.sprite == null)
        {
            sr.sprite = BloodDrainZoneEffect.CreatePlaceholderSprite(new Color(1f, 0.1f, 0.1f, 0.8f));
            // Thu nhỏ để trông như vòng cảnh báo
            transform.localScale = Vector3.one * 0.8f;
        }
    }

    void Update()
    {
        if (sr == null) return;
        // PingPong tạo hiệu ứng nhấp nháy alpha
        float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}
