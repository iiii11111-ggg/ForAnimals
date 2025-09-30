using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // 싱글톤으로 사용 가능

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public Slider bgmSlider;
    public Slider sfxSlider;

    public AudioClip bgmClip; // 원하는 BGM
    public AudioClip TutorialClip;
    public AudioClip testSFXClip;  // 테스트용 효과음 (버튼 클릭용)

    void Awake()
    {
        // 싱글톤 패턴 (선택 사항)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 배경음악 설정 및 재생
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // 볼륨 조절 슬라이더 이벤트 연결
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetBGMVolume(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        if (sfxSource != null)
            sfxSource.volume = value;
    }

    // 외부에서 호출할 수 있는 SFX 재생 함수
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // 미리 지정된 테스트 SFX 재생
    public void PlayTestSFX()
    {
        PlaySFX(testSFXClip);
    }
}

