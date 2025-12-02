using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class modemanager : MonoBehaviour
{
    public TMP_Dropdown modeDropdown;

    // 윈도우 모드 시 사용할 고정 해상도
    private readonly int WINDOW_WIDTH = 1280;
    private readonly int WINDOW_HEIGHT = 720;

    void Start()
    {
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            modeDropdown.value = 0; 
        }
        else if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            modeDropdown.value = 1; 

            Screen.SetResolution(WINDOW_WIDTH, WINDOW_HEIGHT, FullScreenMode.Windowed);
        }

        modeDropdown.onValueChanged.AddListener(OnModeChanged);
    }

    public void OnModeChanged(int index)
    {
        switch (index)
        {
            case 0: 
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                break;

            case 1: 
                Screen.SetResolution(WINDOW_WIDTH, WINDOW_HEIGHT, FullScreenMode.Windowed);

                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;

                Debug.Log($"창 모드 변경: {WINDOW_WIDTH}x{WINDOW_HEIGHT} 해상도로 고정");
                break;
        }
    }
}