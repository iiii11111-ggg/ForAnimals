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
        // ESC Ű�� ������ ���� UI�� ���ư�
        if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchToUI(mainUI);
        }
    }


    // UI ��ȯ �޼���
    public void SwitchToUI(GameObject targetUI)
    {
        if (currentActiveUI != null)
            currentActiveUI.SetActive(false);

        if (startUI != null)
            startUI.SetActive(false); // startUI�� �׻� ��������

        if (targetUI != null)
        {
            targetUI.SetActive(true);
            currentActiveUI = targetUI;
        }
    }

    // ��ư���� ȣ���� �޼����
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

