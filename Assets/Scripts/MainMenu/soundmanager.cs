using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // 싱글톤으로 사용 가능

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip bgmClipMain; 
    public AudioClip bgmClipTutorial;
    public AudioClip bgmClipJungle;
    public AudioClip bgmClipCroco;
    public AudioClip bgmClipBug;
    public AudioClip bgmClipEnding;
    public AudioClip buttonClickClip;

    void Awake()
    {
        // 1. 이미 Instance가 존재하는 경우 (이전 씬에서 DontDestroyOnLoad로 넘어온 인스턴스가 있음)
        if (Instance != null)
        {
            // 씬에 새로운 SoundManager가 등장했으므로, 
            // 🚨 기존의 인스턴스(Instance)를 파괴합니다.
            Destroy(Instance.gameObject);
        }

        // 2. 현재 오브젝트(this)를 새로운 Instance로 지정합니다.
        Instance = this;

        // 3. 그리고 새로운 Instance도 씬 전환 시 유지되도록 설정합니다.
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 배경음악 설정 및 재생
        if (bgmSource != null && bgmClipMain != null)
        {
            bgmSource.clip = bgmClipMain;
            bgmSource.loop = true;
            bgmSource.Play();
        }

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
    public void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickClip);
    }
    public void SetBGMTutorial() 
    {
        bgmSource.clip = bgmClipTutorial;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    public void SetBGMJungle()
    {
        bgmSource.clip = bgmClipJungle;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void SetBGMCroco()
    {
        bgmSource.clip = bgmClipCroco;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    public void SetBGMBug()
    {
        bgmSource.clip = bgmClipBug;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    public void SetBGMEnding()
    {
        bgmSource.clip = bgmClipEnding;
        bgmSource.loop = true;
        bgmSource.Play();
    }

}

