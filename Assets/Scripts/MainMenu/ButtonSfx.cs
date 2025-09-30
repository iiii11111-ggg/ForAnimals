using UnityEngine;
using UnityEngine.UI;

public class ButtonSfx : MonoBehaviour
{
    public AudioSource audioSource;     // 효과음용 AudioSource
    public AudioClip clickSound;        // 버튼 클릭 사운드

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null && audioSource != null && clickSound != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }
}

