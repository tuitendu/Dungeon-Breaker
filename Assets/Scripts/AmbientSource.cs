using UnityEngine;

/// <summary>
/// AmbientSource — Loại 3 (Ambient Loop).
/// Gắn vào TornadoZone prefab (hoặc bất kỳ object nào cần tiếng loop).
///
/// Tự phát khi Start(), đồng bộ volume với AudioManager.ambientVolume,
/// tự tắt khi GameObject bị Destroy (theo Skill2's Destroy(zone, duration)).
///
/// Cách dùng:
///   1. Add component AmbientSource lên TornadoZone prefab
///   2. Kéo clip tornado_loop.wav vào field 'clip'
///   3. Không cần gọi gì thêm — tự phát và tự dừng
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AmbientSource : MonoBehaviour
{
    [Tooltip("Clip ambient loop. Nên là file ngắn ~1-3s, seamless loop.")]
    public AudioClip clip;

    [Range(0f, 1f)]
    [Tooltip("Hệ số volume riêng của source này (nhân với ambientVolume của AudioManager).")]
    public float localVolume = 1f;

    private AudioSource _src;

    void Awake()
    {
        _src              = GetComponent<AudioSource>();
        _src.clip         = clip;
        _src.loop         = true;
        _src.playOnAwake  = false;
        _src.spatialBlend = 0f; // 2D sound
    }

    void Start()
    {
        if (clip == null)
        {
            Debug.LogWarning($"[AmbientSource] {gameObject.name}: Chưa gán clip!");
            return;
        }

        // Đồng bộ volume với AudioManager
        float vol = AudioManager.Instance != null
            ? AudioManager.Instance.GetAmbientVolume()
            : 0.7f;

        _src.volume = vol * localVolume;
        _src.Play();
    }

    // Cập nhật volume nếu người dùng chỉnh trong Settings runtime
    void Update()
    {
        if (AudioManager.Instance != null)
            _src.volume = AudioManager.Instance.GetAmbientVolume() * localVolume;
    }
}
