using UnityEngine;

public class BackButtonManager : MonoBehaviour
{
    public GameObject optionUI; // 옵션 UI GameObject 할당

    void Update()
    {
        // 안드로이드의 Back 버튼 입력 감지 (또는 에디터에선 Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 옵션 UI가 비활성화 상태면 활성화
            if (!optionUI.activeSelf)
            {
                optionUI.SetActive(true);
            }
            else
            {
                // 옵션 UI가 이미 켜져 있으면 끄기
                optionUI.SetActive(false);
            }
        }
    }
}

