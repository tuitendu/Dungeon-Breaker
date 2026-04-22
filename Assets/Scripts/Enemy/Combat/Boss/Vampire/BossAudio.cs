using UnityEngine;

/// <summary>
/// BossAudio — Gắn vào prefab Boss Vampire.
///
/// SFX  : intro, kích hoạt skill, từng viên đạn, bị đánh, chết
/// Ambient: tiếng tornado loop trên từng TornadoZone prefab
///          (không qua BossAudio — dùng AmbientSource.cs riêng)
///
/// Được gọi từ:
///   VampireBossController → PlayIntro()
///   Skill1_BloodBurst     → PlaySkill1Start(), PlayFireOrbShoot()
///   Skill2_BloodDrainZone → PlaySkill2Start()
///   Skill3_OrbRain        → PlaySkill3Start(), PlayFireOrbBig()
///   EnemyStats            → PlayTakeDamage(), PlayDeath()
/// </summary>
public class BossAudio : MonoBehaviour
{
    [Header("── Intro / Kích hoạt boss (SFX)")]
    [Tooltip("Phát khi player bước vào tầm kích hoạt boss.")]
    public AudioClip introClip;

    [Header("── Boss Music (BGM)")]
    [Tooltip("Nhạc nền khi đang đánh Boss. Tự động đổi về nhạc cũ khi đi ra xa.")]
    public AudioClip bossBGM;

    [Header("── Skill 1 — Fire Rapid Shot (SFX)")]
    [Tooltip("Phát 1 lần khi bắt đầu skill 1.")]
    public AudioClip skill1StartClip;
    [Tooltip("Phát mỗi viên lửa nhỏ bắn ra. Nên là clip ngắn ~0.1–0.2s.")]
    public AudioClip fireOrbClip;

    [Header("── Skill 2 — Tornado Zone (SFX)")]
    [Tooltip("Phát 1 lần khi spawn các vùng tornado.")]
    public AudioClip skill2StartClip;
    // ★ Tiếng loop trong vùng tornado → dùng AmbientSource.cs gắn trên TornadoZone prefab

    [Header("── Skill 3 — Fire Big Shot (SFX)")]
    [Tooltip("Phát 1 lần khi bắt đầu skill 3.")]
    public AudioClip skill3StartClip;
    [Tooltip("Phát mỗi viên lửa to bắn ra. Clip to hơn, khác fireOrbClip.")]
    public AudioClip fireOrbBigClip;

    [Header("── Trạng thái (SFX)")]
    [Tooltip("Bị player đánh trúng.")]
    public AudioClip takeDamageClip;
    [Tooltip("Khi boss chết (HP = 0).")]
    public AudioClip deathClip;

    [Header("── Âm thanh 3D (Khoảng cách)")]
    [Tooltip("Khoảng cách lớn nhất có thể nghe thấy tiếng Boss (đặt bằng hoặc hơn tầm đánh một chút). Ngồi ngoài khoảng này sẽ không nghe thấy gì.")]
    public float maxHearDistance = 15f;

    private AudioSource _audioSource;

    private void Awake()
    {
        // Tự động tạo AudioSource 3D trên Boss
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f; // Chỉnh thành âm thanh 3D 100%
        _audioSource.maxDistance = maxHearDistance;
        _audioSource.minDistance = 3f;  // Âm lượng lớn nhất khi đúng gần < 3 đơn vị
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    // ─── Public API ────────────────────────────────────────────────────────────
    public void PlayIntro()        => Play(introClip,       0.9f);

    public void PlayBossBGM()
    {
        if (bossBGM != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(bossBGM);
    }
    public void RestoreBGM()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.RestoreDefaultBGM();
    }

    public void PlaySkill1Start()  => Play(skill1StartClip);
    public void PlayFireOrb()      => Play(fireOrbClip,     0.7f); // volume nhỏ hơn vì phát nhiều lần
    public void PlaySkill2Start()  => Play(skill2StartClip);
    public void PlaySkill3Start()  => Play(skill3StartClip);
    public void PlayFireOrbBig()   => Play(fireOrbBigClip,  0.8f);
    public void PlayTakeDamage()   => Play(takeDamageClip);
    
    public void PlayDeath()        
    {
        Play(deathClip, 1f);
        RestoreBGM(); // Trả lại nhạc nền cũ khi boss chết
    }

    // ─── Internal helper ───────────────────────────────────────────────────────
    private void Play(AudioClip clip, float vol = 1f)
    {
        if (clip == null || _audioSource == null) return;

        // Đồng bộ âm lượng tông với AudioManager
        float masterSFXVol = AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 1f;
        
        _audioSource.PlayOneShot(clip, vol * masterSFXVol);
    }
}
