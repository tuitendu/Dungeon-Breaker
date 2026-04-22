using UnityEngine;

/// <summary>
/// NPCAudio — Gắn vào NPC prefab.
/// Sử dụng cùng với Animation Events để phát tiếng bước chân cho mỗi frame anim.
///
/// ===== HƯỚNG DẪN SỬ DỤNG =====
/// 1. Gắn script này vào NPC
/// 2. Kéo AudioClip tiếng bước chân vào "Footstep Clips"
/// 3. Chọn clip Animation của NPC (vd: Walk_Left)
/// 4. Thêm Event vào các frame lúc chân chạm đất, gọi hàm: PlayFootstep()
/// ==============================
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class NPCAudio : MonoBehaviour
{
    [Header("===== TIẾNG BƯỚC CHÂN =====")]
    [Tooltip("Danh sách AudioClip tiếng bước chân.\n" +
             "Kéo 1 hoặc nhiều clip vào. Nếu nhiều clip → phát ngẫu nhiên mỗi bước.")]
    public AudioClip[] footstepClips;

    [Header("===== ÂM LƯỢNG =====")]
    [Tooltip("Âm lượng tiếng bước chân (0 = im lặng, 1 = tối đa)")]
    [Range(0f, 1f)]
    public float footstepVolume = 0.4f;

    [Header("===== TIẾNG HÀNH ĐỘNG (TƯƠNG TÁC) =====")]
    [Tooltip("Danh sách AudioClip sự kiện hành động của NPC (ví dụ: gõ búa, lau bàn, cuốc đất...).\n" +
             "Kéo 1 hoặc nhiều clip vào. Nếu nhiều clip → phát ngẫu nhiên.")]
    public AudioClip[] actionClips;

    [Tooltip("Âm lượng tiếng hành động")]
    [Range(0f, 1f)]
    public float actionVolume = 0.5f;

    [Header("===== KHOẢNG CÁCH NGHE =====")]
    [Tooltip("NPC cách Player bao xa thì bắt đầu nghe thấy tiếng bước.\n" +
             "Ngoài khoảng cách này → im lặng (tiết kiệm hiệu năng).")]
    public float maxHearDistance = 10f;

    [Tooltip("Âm thanh 2D (0) hay 3D (1).\n" +
             "0 = nghe rõ ở mọi khoảng cách.\n" +
             "1 = tiếng nhỏ dần khi xa NPC (chân thực hơn).\n" +
             "Gợi ý: 0.6 ~ 0.8 cho game 2D top-down.")]
    [Range(0f, 1f)]
    public float spatialBlend = 0.7f;

    // =========================================================
    //  BIẾN NỘI BỘ
    // =========================================================
    private AudioSource _audioSource;
    private Transform   _playerTransform;

    // =========================================================
    //  KHỞI TẠO
    // =========================================================
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        // Cấu hình AudioSource
        _audioSource.loop         = false;
        _audioSource.playOnAwake  = false;
        _audioSource.spatialBlend = spatialBlend;
        _audioSource.maxDistance  = maxHearDistance;
        _audioSource.rolloffMode  = AudioRolloffMode.Linear;
        _audioSource.dopplerLevel = 0f; // Tắt Doppler (không cần cho game 2D)
    }

    private void Start()
    {
        // Tìm Player 1 lần
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _playerTransform = playerObj.transform;
    }

    // =========================================================
    //  PHÁT VÀO MỖI FRAME TỪ ANIMATION EVENT
    // =========================================================
    
    /// <summary>
    /// GỌI HÀM NÀY TỪ ANIMATION EVENT (Ví dụ trên các frame chạm đất).
    /// </summary>
    public void PlayFootstep()
    {
        // Kiểm tra Player có trong tầm nghe không
        if (!CheckPlayerInRange()) return; 

        // Nếu không có clip, không làm gì
        if (footstepClips == null || footstepClips.Length == 0) return;

        // Chọn ngẫu nhiên 1 clip
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        if (clip == null) return;

        // Lấy volume từ AudioManager (nếu có) để đồng bộ settings
        float masterVolume = AudioManager.Instance != null
            ? AudioManager.Instance.sfxVolume
            : 1f;

        // Dùng PlayOneShot để cho phép chồng âm thanh (chân thực hơn khi bước nhanh)
        _audioSource.pitch = Random.Range(0.9f, 1.1f); // Variation nhẹ cho tự nhiên
        _audioSource.PlayOneShot(clip, footstepVolume * masterVolume);
    }
    
    /// <summary>
    /// GỌI HÀM NÀY TỪ ANIMATION EVENT ĐỂ CHƠI ÂM THANH TUỲ CHỈNH (Tiếng búa, v.v)
    /// </summary>
    public void PlaySpecificSound(AudioClip clip)
    {
        if (clip == null || !CheckPlayerInRange()) return;

        float masterVolume = AudioManager.Instance != null
            ? AudioManager.Instance.sfxVolume
            : 1f;

        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.PlayOneShot(clip, footstepVolume * masterVolume);
    }

    /// <summary>
    /// GỌI HÀM NÀY TỪ ANIMATION EVENT CỦA HÀNH ĐỘNG (Ví dụ: frame đập búa xuống).
    /// Phát ngẫu nhiên clip trong mảng actionClips.
    /// </summary>
    public void PlayActionSound()
    {
        // Kiểm tra Player có trong tầm nghe không
        if (!CheckPlayerInRange()) return; 

        // Nếu không có clip, không làm gì
        if (actionClips == null || actionClips.Length == 0) return;

        // Chọn ngẫu nhiên 1 clip hành động
        AudioClip clip = actionClips[Random.Range(0, actionClips.Length)];
        if (clip == null) return;

        float masterVolume = AudioManager.Instance != null
            ? AudioManager.Instance.sfxVolume
            : 1f;

        _audioSource.pitch = Random.Range(0.9f, 1.1f); // Variation nhẹ
        _audioSource.PlayOneShot(clip, actionVolume * masterVolume);
    }

    // =========================================================
    //  KIỂM TRA TRẠNG THÁI
    // =========================================================

    /// <summary>
    /// Kiểm tra Player có trong tầm nghe không.
    /// Nếu không có Player hoặc quá xa → trả false → không phát tiếng.
    /// </summary>
    private bool CheckPlayerInRange()
    {
        if (_playerTransform == null) return true; // Không tìm thấy Player → phát bình thường

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        return dist <= maxHearDistance;
    }

    // =========================================================
    //  GIZMOS
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        // Vòng tròn tầm nghe (xám nhạt)
        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
        Gizmos.DrawSphere(transform.position, maxHearDistance);

        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxHearDistance);
    }
}
