using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource bgmSource;   // AudioManager ���� BGMSource�� ���� AudioSource
    public AudioSource sfxSource;   // AudioManager ���� SFXSource�� ���� AudioSource

    [Header("Default Clips")]
    public AudioClip uiClick;       // UI �⺻ Ŭ����

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);                // ��Ʈ��
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayUIClick() => PlaySFX(uiClick, 1f);

    // 3D ��ġ���� ����ϰ� ���� ��
    public void PlayAt(AudioClip clip, Vector3 worldPos, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPos, volume);
    }
}
