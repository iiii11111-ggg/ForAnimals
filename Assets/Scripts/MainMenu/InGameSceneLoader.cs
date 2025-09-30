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
        // DataManager�� ����� ���� ��ȣ�� �����ɴϴ�.
        int slotToLoad = PlayerData.currentSlotIndex;

        string folderPath = Path.Combine(Application.persistentDataPath, "SAVE");
        string filePath = Path.Combine(folderPath, "SaveSlot" + slotToLoad + ".json");

        GameData data;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("���� " + slotToLoad + "�� ĳ���� ��ġ �ε� ����.");
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
            Debug.Log("���� " + slotToLoad + "�� ���ο� ���� ������ �����Ǿ����ϴ�.");
        }

        playerTransform.position = data.characterPosition;
    }
}