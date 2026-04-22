using UnityEngine;

/// <summary>
/// PlayerAudio — Gắn vào Player prefab.
/// Footstep dùng AudioSource riêng (không qua pool) → không chồng lằn.
/// Các SFX khác đi qua AudioManager pool.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("── Di chuyển (SFX)")]
    [Tooltip("Tiếng bước chân. Phát mỗi 0.35s khi di chuyển.")]
    public AudioClip footstepClip;

    [Header("── Chiến đấu (SFX)")]
    [Tooltip("Đánh thường (kiếm / cung / phép).")]
    public AudioClip basicAttackClip;
    [Tooltip("Khi dùng Skill 1.")]
    public AudioClip skill1Clip;
    [Tooltip("Khi dùng Skill 2.")]
    public AudioClip skill2Clip;
    [Tooltip("Khi dùng Skill 3.")]
    public AudioClip skill3Clip;
    [Tooltip("Khi dùng Skill 4.")]
    public AudioClip skill4Clip;

    [Header("── Trạng thái (SFX)")]
    [Tooltip("Bị quái đánh trúng.")]
    public AudioClip takeDamageClip;
    [Tooltip("Khi HP = 0, player chết.")]
    public AudioClip deathClip;
    [Tooltip("Dùng potion / hồi máu.")]
    public AudioClip healClip;
    [Tooltip("Nhặt item rơi.")]
    public AudioClip pickupClip;
    [Tooltip("Trang bị item.")]
    public AudioClip equipClip;

    // ─── Footstep AudioSource riêng ───────────────────────────────────────────
    private AudioSource _footstepSrc;

    // ─── Skill3 Loop AudioSource riêng ───────────────────────────────────────
    // loop = true → phát đến khi StopSkill3() được gọi (hết spinDuration)
    private AudioSource _skill3Src;

    void Awake()
    {
        // Footstep source (AudioSource do [RequireComponent] đã tạo)
        _footstepSrc              = GetComponent<AudioSource>();
        _footstepSrc.loop         = false;
        _footstepSrc.playOnAwake  = false;
        _footstepSrc.spatialBlend = 0f;

        // Skill3 source — thêm 1 AudioSource mới trên cùng GameObject
        _skill3Src              = gameObject.AddComponent<AudioSource>();
        _skill3Src.loop         = true;   // ← loop đến khi Stop
        _skill3Src.playOnAwake  = false;
        _skill3Src.spatialBlend = 0f;
    }

    // ─── Public API — gọi từ Player_Combat, PlayerStats, Playermove ──────────

    // Di chuyển
    /// <summary>
    /// Gọi từ Playermove.Update() mỗi frame khi đang di chuyển.
    /// Guard isPlaying → clip 3-nhịp (0.3s) tự xong rồi mới phát lại.
    /// Không bao giờ chồng lằn.
    /// </summary>
    public void TickFootstep()
    {
        if (footstepClip == null || _footstepSrc.isPlaying) return;

        _footstepSrc.clip   = footstepClip;
        _footstepSrc.volume = AudioManager.Instance != null
            ? AudioManager.Instance.sfxVolume * 0.7f
            : 0.7f;
        _footstepSrc.Play();
    }

    public void StopFootstep()
    {
        if (_footstepSrc != null && _footstepSrc.isPlaying)
            _footstepSrc.Stop();
    }

    // Chiến đấu
    public void PlayBasicAttack() => Play(basicAttackClip);
    public void PlaySkill1()      => Play(skill1Clip);
    public void PlaySkill2()      => Play(skill2Clip);
    public void PlaySkill4()      => Play(skill4Clip);

    /// <summary>Bắt đầu loop clip skill3 — gọi khi Skill3 kích hoạt.</summary>
    public void PlaySkill3()
    {
        if (skill3Clip == null) return;
        _skill3Src.clip   = skill3Clip;
        _skill3Src.volume = AudioManager.Instance != null
            ? AudioManager.Instance.sfxVolume
            : 1f;
        _skill3Src.Play();
    }

    /// <summary>Dừng loop skill3 — gọi khi SpinCooldownRoutine kết thúc.</summary>
    public void StopSkill3()
    {
        if (_skill3Src.isPlaying) _skill3Src.Stop();
    }

    // Trạng thái
    public void PlayTakeDamage()  => Play(takeDamageClip);
    public void PlayDeath()       => Play(deathClip);
    public void PlayHeal()        => Play(healClip);
    public void PlayPickup()      => Play(pickupClip);
    public void PlayEquip()       => Play(equipClip);

    // ─── Internal helper ──────────────────────────────────────────────────────
    private void Play(AudioClip clip, float vol = 1f)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip, vol);
    }
}
