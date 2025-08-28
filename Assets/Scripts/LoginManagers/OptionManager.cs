using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionManager : MonoBehaviour
{
    [Header("UI")]
    public Slider bgmSlider;          // BGMSlider
    public Slider sfxSlider;          // SFXSlider

    [Header("Mixer")]
    public AudioMixer mixer;          // Master.mixer 에셋 드래그
    public string bgmParam = "BGM_VOL";
    public string sfxParam = "SFX_VOL";

    const string KEY_BGM = "opt_bgm_norm";
    const string KEY_SFX = "opt_sfx_norm";

    void Awake()
    {
        // 저장값 로드(없으면 기본값)
        float bgm = PlayerPrefs.GetFloat(KEY_BGM, 0.8f);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, 1.0f);

        if (bgmSlider) bgmSlider.value = bgm;
        if (sfxSlider) sfxSlider.value = sfx;

        ApplyBgm(bgm);
        ApplySfx(sfx);

        if (bgmSlider) bgmSlider.onValueChanged.AddListener(v => { ApplyBgm(v); PlayerPrefs.SetFloat(KEY_BGM, v); });
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(v => { ApplySfx(v); PlayerPrefs.SetFloat(KEY_SFX, v); });
    }

    // 0~1 선형값 → 데시벨 변환(오디오 표준)
    float LinearToDB(float v)
    {
        if (v <= 0.0001f) return -80f;   // 사실상 무음
        return Mathf.Log10(v) * 20f;     // 20*log10(v)
    }

    void ApplyBgm(float v)
    {
        if (mixer) mixer.SetFloat(bgmParam, LinearToDB(v));
    }

    void ApplySfx(float v)
    {
        if (mixer) mixer.SetFloat(sfxParam, LinearToDB(v));
    }
}
