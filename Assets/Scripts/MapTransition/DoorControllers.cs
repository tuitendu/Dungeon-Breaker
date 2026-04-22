using UnityEngine;
using System.Collections;

/// <summary>
/// Điều khiển animation mở/đóng cửa mượt mà khi người chơi bước qua.
/// 
/// === WORKFLOW MỞ CỬA MƯỢT ===
/// 
/// 1. Player đi vào OUTER ZONE (vùng xa) → cửa bắt đầu mở CHẬM
/// 2. Player đi vào INNER ZONE (vùng gần) → cửa mở NHANH hơn
/// 3. Player đi qua cửa thoải mái (không bị chặn)
/// 4. Player rời OUTER ZONE → cửa đóng lại từ từ
///
/// === SETUP ===
///
/// 🚪 DoorSprite              ← SpriteRenderer + Animator
///   ├── OuterTrigger          ← BoxCollider2D (Is Trigger, rộng ~4 units)
///   │     └── DoorControllers ← SCRIPT NÀY
///   └── (không cần collider vật lý)
///
/// Hoặc dùng 1 trigger zone duy nhất nếu muốn đơn giản hơn.
/// </summary>
public class DoorControllers : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Animator của cửa. Kéo từ parent nếu script ở child.")]
    [SerializeField] private Animator doorAnimator;

    [Header("Audio (Tùy chọn)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Animation Speed")]
    [Tooltip("Tốc độ animation mở cửa (1 = bình thường, 2 = nhanh gấp đôi)")]
    [SerializeField] private float openSpeed = 1.5f;
    [Tooltip("Tốc độ animation đóng cửa")]
    [SerializeField] private float closeSpeed = 1f;

    [Header("Timing")]
    [Tooltip("Thời gian chờ trước khi đóng cửa (giây)")]
    [SerializeField] private float closeDelay = 0.8f;
    [Tooltip("Cửa có tự đóng khi người chơi rời đi không?")]
    [SerializeField] private bool autoClose = true;

    // Animator parameter & state hashes
    private static readonly int IsOpenHash = Animator.StringToHash("isOpen");
    private static readonly int SpeedHash = Animator.StringToHash("animSpeed");

    private bool isOpen = false;
    private Coroutine closeCoroutine;
    private Transform playerTransform;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerTransform = collision.transform;

        // Hủy lệnh đóng cửa nếu đang chờ
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        OpenDoor();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (!autoClose) return;

        playerTransform = null;

        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);

        closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        if (doorAnimator != null)
        {
            doorAnimator.speed = openSpeed;
            doorAnimator.SetBool(IsOpenHash, true);
        }

        PlaySound(openSound);
    }

    public void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        if (doorAnimator != null)
        {
            doorAnimator.speed = closeSpeed;
            doorAnimator.SetBool(IsOpenHash, false);
        }

        PlaySound(closeSound);
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        CloseDoor();
        closeCoroutine = null;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        // Ưu tiên dùng AudioManager tổng của Game (để đồng bộ âm lượng SFX)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else if (audioSource != null)
        {
            // Fallback nếu không có AudioManager
            audioSource.PlayOneShot(clip);
        }
    }
}
