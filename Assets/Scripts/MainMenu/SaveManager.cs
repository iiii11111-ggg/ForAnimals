// SaveManager.cs (싱글톤 적용 버전)
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // 1. 싱글톤 인스턴스 변수 추가
    public static SaveManager Instance { get; private set; }

    // playerTransform은 더 이상 public일 필요가 없습니다.
    private Transform playerTransform;

    // 2. Awake() 함수에서 인스턴스 설정
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지되도록 설정
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있으면 새로 생긴 것은 파괴
        }
    }

    public void SaveGameData()
    {
        // 저장하는 시점에 플레이어를 찾아서 할당 (씬이 바뀌어도 문제없음)
        playerTransform = GameObject.FindWithTag("Player").transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        int currentSaveSlot = PlayerData.currentSlotIndex;
        if (currentSaveSlot == 0)
        {
            Debug.LogError("현재 저장할 슬롯이 선택되지 않았습니다.");
            return;
        }

        string folderPath = Path.Combine(Application.dataPath, "../", "SAVE");
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

    private string GetDestructionKey(int saveSlotIndex, string objectID)
    {
        // 예시 키: "Slot_3_Destroyed_Chest_001"
        return "Slot_" + saveSlotIndex.ToString() + "_Destroyed_" + objectID;
    }

    // 💡 2. 파괴 기록 확인 (int 인덱스 사용)
    public bool HasBeenDestroyed(int saveSlotIndex, string objectID)
    {
        string key = GetDestructionKey(saveSlotIndex, objectID);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // 💡 3. 파괴 기록 저장 (int 인덱스 사용)
    public void MarkAsDestroyed(int saveSlotIndex, string objectID)
    {
        string key = GetDestructionKey(saveSlotIndex, objectID);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log($"슬롯 인덱스 [{saveSlotIndex}]에 ID: {objectID}가 영구 파괴된 것으로 기록되었습니다.");
    }

}