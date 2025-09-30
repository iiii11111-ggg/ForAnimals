using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public Transform playerTransform;

    public void SaveGameData()
    {
        // DataManager에서 현재 로드된 슬롯 번호를 가져옵니다.
        int currentSaveSlot = PlayerData.currentSlotIndex;

        // 슬롯 번호가 유효한지 확인합니다.
        if (currentSaveSlot == 0)
        {
            Debug.LogError("현재 저장할 슬롯이 선택되지 않았습니다. 메인 메뉴에서 슬롯을 선택했는지 확인하세요.");
            return;
        }

        string folderPath = Path.Combine(Application.persistentDataPath, "SAVE");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "SaveSlot" + currentSaveSlot + ".json");

        GameData data = new GameData();
        data.characterPosition = playerTransform.position;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);

        Debug.Log("슬롯 " + currentSaveSlot + "에 게임 데이터가 저장되었습니다.");
    }
}