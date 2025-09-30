using UnityEngine;
using UnityEngine.UI;

public class ButtonSfx : MonoBehaviour
{
    public AudioSource audioSource;     // ȿ������ AudioSource
    public AudioClip clickSound;        // ��ư Ŭ�� ����

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

