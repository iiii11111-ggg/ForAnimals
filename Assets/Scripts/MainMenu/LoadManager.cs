using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public void LoadGameData(int slotIndex)
    {
        // 1. 선택된 슬롯 번호를 DataManager에 저장합니다.
        PlayerData.currentSlotIndex = slotIndex;
        Debug.Log("DataManager에 슬롯 번호 " + slotIndex + " 저장됨.");

        // 2. 게임 씬으로 이동합니다.
        SceneManager.LoadScene("InGame");
    }
}