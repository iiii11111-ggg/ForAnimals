using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject startUI;
    public GameObject mainUI;
    public GameObject saveUI;
    public GameObject minigameUI;
    public GameObject settingUI;
    public GameObject screenUI;
    public GameObject soundUI;

    private GameObject currentActiveUI;

    private bool gameStarted = true;

    void Update()
    {
        // ESC 키로 언제든 메인 UI로 돌아감
        if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchToUI(mainUI);
        }
    }


    // UI 전환 메서드
    public void SwitchToUI(GameObject targetUI)
    {
        if (currentActiveUI != null)
            currentActiveUI.SetActive(false);

        if (startUI != null)
            startUI.SetActive(false); // startUI는 항상 꺼지도록

        if (targetUI != null)
        {
            targetUI.SetActive(true);
            currentActiveUI = targetUI;
        }
    }

    // 버튼에서 호출할 메서드들
    public void OnStartbuttonClicked()
    {
        SwitchToUI(mainUI);
    }

    public void OnGamestartbuttonClicked()
    {
        SwitchToUI(saveUI);
    }

    public void OnMinigamebuttonClicked()
    {
        SwitchToUI(minigameUI);
    }

    public void OnSettingbuttonClicked()
    {
        SwitchToUI(settingUI);
    }

    public void OnsoundbuttonClicked()
    {
        SwitchToUI(soundUI);
    }

    public void OnscreenbuttonClicked()
    {
        SwitchToUI(screenUI);
    }

    public void OnbackbuttonClicked()
    {
        SwitchToUI(mainUI);
    }

}

