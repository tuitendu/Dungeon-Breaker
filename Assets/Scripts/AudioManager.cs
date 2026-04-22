using UnityEngine;

/// <summary>
/// AudioManager — Singleton trung tâm quản lý 3 loại âm thanh:
///
///   1. SFX   : Ngắn, phát 1 lần (đánh, skill, damage, chết...)
///              → Pool 8 AudioSource dùng chung, không bao giờ bị thiếu
///
///   2. BGM   : Nhạc nền, loop liên tục
///              → 1 AudioSource riêng, chuyển bài mượt qua CrossFade
///
///   3. Ambient: Tiếng môi trường loop (vùng tornado, tiếng gió...)
///              → AudioSource gắn trực tiếp trên prefab (không qua Manager)
///              → Manager chỉ cung cấp volume reference
///
/// Cách dùng từ bất kỳ script nào:
///   AudioManager.Instance.PlaySFX(clip);
///   AudioManager.Instance.PlaySFX(clip, 0.8f);
///   AudioManager.Instance.PlayBGM(bgmClip);
///   AudioManager.Instance.StopBGM();
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ─── Volume Settings ──────────────────────────────────────────────────────
    [Header("Volume (0–1)")]
    [Range(0f, 1f)] public float sfxVolume     = 0.5f;
    [Range(0f, 1f)] public float bgmVolume     = 0.25f;
    [Range(0f, 1f)] public float ambientVolume = 0.35f;

    [Header("Main BGM")]
    [Tooltip("Nhạc nền tự động phát ngay khi vào game (nếu có).")]
    public AudioClip startingBGM;

    // ─── Internal ─────────────────────────────────────────────────────────────
    private const int POOL_SIZE = 8;
    private AudioSource[] _sfxPool;   // Pool cho SFX
    private AudioSource   _bgmSource; // Source cho BGM
    private int           _nextPool  = 0;

    // Lưu lại nhạc nền cơ bản để trả về sau khi đánh boss
    private AudioClip     _defaultBGM;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────
    void Awake()
    {
        // Singleton: chỉ tồn tại 1 instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // sống qua scene

        BuildPool();
    }

    void Start()
    {
        if (startingBGM != null)
        {
            _defaultBGM = startingBGM; // Lưu lại làm nhạc mặc định
            PlayBGM(startingBGM);
        }
    }

    // ─── Build pool AudioSource ───────────────────────────────────────────────
    private void BuildPool()
    {
        // SFX Pool — 8 AudioSource dùng chung
        _sfxPool = new AudioSource[POOL_SIZE];
        for (int i = 0; i < POOL_SIZE; i++)
        {
            _sfxPool[i]        = gameObject.AddComponent<AudioSource>();
            _sfxPool[i].loop   = false;
            _sfxPool[i].playOnAwake = false;
        }

        // BGM Source — 1 source riêng để loop nhạc nền
        _bgmSource           = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop      = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume    = bgmVolume;
    }

    // =========================================================================
    // 1. SFX — Phát 1 lần, ngắn
    // =========================================================================

    /// <summary>Phát SFX (attack, skill, damage...). Tự tìm AudioSource rảnh.</summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        // Tìm source đang rảnh
        for (int i = 0; i < POOL_SIZE; i++)
        {
            if (!_sfxPool[i].isPlaying)
            {
                PlayOnSource(_sfxPool[i], clip, volumeScale);
                return;
            }
        }

        // Tất cả bận → dùng round-robin (đẩy clip cũ nhất ra)
        PlayOnSource(_sfxPool[_nextPool], clip, volumeScale);
        _nextPool = (_nextPool + 1) % POOL_SIZE;
    }

    private void PlayOnSource(AudioSource src, AudioClip clip, float volumeScale)
    {
        src.clip   = clip;
        src.volume = sfxVolume * Mathf.Clamp01(volumeScale);
        src.Play();
    }

    // =========================================================================
    // 2. BGM — Nhạc nền, loop
    // =========================================================================

    /// <summary>Chuyển sang bài BGM mới. Nếu đang phát cùng bài thì bỏ qua.</summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) { StopBGM(); return; }
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

        _bgmSource.clip   = clip;
        _bgmSource.volume = bgmVolume;
        _bgmSource.Play();
    }

    /// <summary>Dừng nhạc nền.</summary>
    public void StopBGM()
    {
        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    /// <summary>Khôi phục lại nhạc nền mặc định (startingBGM).</summary>
    public void RestoreDefaultBGM()
    {
        if (_defaultBGM != null)
        {
            PlayBGM(_defaultBGM);
        }
        else
        {
            StopBGM();
        }
    }

    // =========================================================================
    // 3. Ambient — Dùng qua AmbientSource helper (không phát qua Manager)
    //    Manager chỉ cung cấp volume để các AmbientSource tự đồng bộ
    // =========================================================================

    /// <summary>
    /// Trả về volume ambient hiện tại.
    /// AmbientSource gọi hàm này để đồng bộ khi volume thay đổi.
    /// </summary>
    public float GetAmbientVolume() => ambientVolume;

    // ─── Cập nhật volume runtime (dùng cho Settings UI) ─────────────────────
    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
    }

    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        _bgmSource.volume = bgmVolume;
    }

    public void SetAmbientVolume(float v)
    {
        ambientVolume = Mathf.Clamp01(v);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Khi bạn kéo thanh Slider trên màn hình Inspector ở chế độ Play, nó sẽ cập nhật thẳng xuống các AudioSource ngay.
        if (Application.isPlaying)
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = bgmVolume;
            }
            if (_sfxPool != null)
            {
                for (int i = 0; i < _sfxPool.Length; i++)
                {
                    if (_sfxPool[i] != null)
                    {
                        _sfxPool[i].volume = sfxVolume;
                    }
                }
            }
        }
    }
#endif
}
