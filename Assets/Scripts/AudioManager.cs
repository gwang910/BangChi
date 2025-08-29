using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource bgmSource;   // AudioManager 하위 BGMSource에 붙은 AudioSource
    public AudioSource sfxSource;   // AudioManager 하위 SFXSource에 붙은 AudioSource

    [Header("Default Clips")]
    public AudioClip uiClick;       // UI 기본 클릭음

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);                // 루트로
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayUIClick() => PlaySFX(uiClick, 1f);

    // 3D 위치에서 재생하고 싶을 때
    public void PlayAt(AudioClip clip, Vector3 worldPos, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPos, volume);
    }
}
