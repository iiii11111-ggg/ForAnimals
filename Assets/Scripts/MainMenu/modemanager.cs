using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class modemanager : MonoBehaviour
{
    public TMP_Dropdown modeDropdown;

    void Start()
    {
        // 현재 모드에 따라 Dropdown 기본값 설정
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            modeDropdown.value = 0; // Full Screen
        }
        else if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            modeDropdown.value = 1; // Windowed
        }

        // Dropdown 값 변경 이벤트 등록
        modeDropdown.onValueChanged.AddListener(OnModeChanged);
    }

    public void OnModeChanged(int index)
    {
        switch (index)
        {
            case 0: // Full Screen
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                break;

            case 1: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                break;
        }
    }
}



