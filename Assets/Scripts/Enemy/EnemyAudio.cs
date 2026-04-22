using UnityEngine;

/// <summary>
/// EnemyAudio — Gắn vào tất cả enemy prefab (Slime, Plant, Vampire, Orc).
/// Cùng 1 script, nhưng mỗi prefab kéo clip khác nhau trong Inspector.
///
/// Tất cả clip thuộc loại SFX (phát 1 lần, ngắn).
/// Được gọi từ EnemyStats (damage/death) và EnemyHitbox (attack).
/// </summary>
public class EnemyAudio : MonoBehaviour
{
    [Header("── Tấn công (SFX)")]
    [Tooltip("Tiếng chém / bắn khi enemy tấn công player.\n" +
             "Gọi từ EnemyHitbox khi EnableHitbox().")]
    public AudioClip attackClip;

    [Header("── Nhận sát thương (SFX)")]
    [Tooltip("Tiếng bị đánh (grunt). Phát khi TakeDamage() > 0.")]
    public AudioClip takeDamageClip;

    [Header("── Chết (SFX)")]
    [Tooltip("Tiếng chết. Phát ngay khi HP = 0.")]
    public AudioClip deathClip;

    [Header("── Di chuyển (SFX, Optional)")]
    [Tooltip("Bước chân. Để trống nếu không cần.\n" +
             "Gọi thủ công từ Animation Event trong clip Walk.")]
    public AudioClip footstepClip;

    // ─── Public API ────────────────────────────────────────────────────────────
    public void PlayAttack()     => Play(attackClip);
    public void PlayTakeDamage() => Play(takeDamageClip);
    public void PlayDeath()      => Play(deathClip);
    public void PlayFootstep()   => Play(footstepClip, 0.5f);

    // ─── Internal helper ───────────────────────────────────────────────────────
    private void Play(AudioClip clip, float vol = 1f)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip, vol);
    }
}
