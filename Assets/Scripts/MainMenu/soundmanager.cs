using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // �̱������� ��� ����

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public Slider bgmSlider;
    public Slider sfxSlider;

    public AudioClip bgmClip; // ���ϴ� BGM
    public AudioClip TutorialClip;
    public AudioClip testSFXClip;  // �׽�Ʈ�� ȿ���� (��ư Ŭ����)

    void Awake()
    {
        // �̱��� ���� (���� ����)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ���� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ������� ���� �� ���
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // ���� ���� �����̴� �̺�Ʈ ����
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

    // �ܺο��� ȣ���� �� �ִ� SFX ��� �Լ�
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // �̸� ������ �׽�Ʈ SFX ���
    public void PlayTestSFX()
    {
        PlaySFX(testSFXClip);
    }
}

