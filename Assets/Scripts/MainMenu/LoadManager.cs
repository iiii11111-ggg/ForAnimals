using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public void LoadGameData(int slotIndex)
    {
        // 1. ���õ� ���� ��ȣ�� DataManager�� �����մϴ�.
        PlayerData.currentSlotIndex = slotIndex;
        Debug.Log("DataManager�� ���� ��ȣ " + slotIndex + " �����.");

        // 2. ���� ������ �̵��մϴ�.
        SceneManager.LoadScene("InGame");
    }
}