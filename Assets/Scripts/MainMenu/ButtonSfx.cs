using UnityEngine;
using UnityEngine.UI;

public class ButtonSfx : MonoBehaviour
{ 

    void Start()
    {
        // 1. 현재 오브젝트에서 Button 컴포넌트를 가져옵니다.
        Button button = GetComponent<Button>();

        // 2. Button 컴포넌트와 SoundManager 인스턴스가 있는지 확인합니다.
        if (button != null && SoundManager.Instance != null)
        {
            button.onClick.AddListener(SoundManager.Instance.PlayButtonClickSFX);
        }
    }
}
    
