using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Vector3 characterPosition;
}

public class InGameSceneLoader : MonoBehaviour
{
    public Transform playerTransform;

    void Start()
    {
        // DataManager에 저장된 슬롯 번호를 가져옵니다.
        int slotToLoad = PlayerData.currentSlotIndex;

        string folderPath = Path.Combine(Application.persistentDataPath, "SAVE");
        string filePath = Path.Combine(folderPath, "SaveSlot" + slotToLoad + ".json");

        GameData data;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("슬롯 " + slotToLoad + "의 캐릭터 위치 로드 성공.");
        }
        else
        {
            data = new GameData();
            data.characterPosition = new Vector3(24, 0, 5);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
            Debug.Log("슬롯 " + slotToLoad + "에 새로운 게임 파일이 생성되었습니다.");
        }

        playerTransform.position = data.characterPosition;
    }
}