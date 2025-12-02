using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    // 이 슬라이더가 BGM용인지 SFX용인지 선택하기 위한 열거형
    public enum AudioType { BGM, SFX }
    public AudioType audioType;

    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();

        // SoundManager가 없다면 리턴 (에러 방지)
        if (SoundManager.Instance == null) return;

        // 1. 현재 사운드 매니저의 볼륨 값을 슬라이더에 반영 (초기화)
        if (audioType == AudioType.BGM)
        {
            // SoundManager의 AudioSource에서 현재 볼륨을 가져와 슬라이더 값을 맞춤
            if (SoundManager.Instance.bgmSource != null)
                slider.value = SoundManager.Instance.bgmSource.volume;

            // 2. 슬라이더 이벤트 동적 연결 (코드상으로 연결)
            slider.onValueChanged.AddListener((value) => SoundManager.Instance.SetBGMVolume(value));
        }
        else // SFX
        {
            if (SoundManager.Instance.sfxSource != null)
                slider.value = SoundManager.Instance.sfxSource.volume;

            slider.onValueChanged.AddListener((value) => SoundManager.Instance.SetSFXVolume(value));
        }
    }
}