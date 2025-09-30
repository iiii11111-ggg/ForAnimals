using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public Transform playerTransform;

    public void SaveGameData()
    {
        // DataManager���� ���� �ε�� ���� ��ȣ�� �����ɴϴ�.
        int currentSaveSlot = PlayerData.currentSlotIndex;

        // ���� ��ȣ�� ��ȿ���� Ȯ���մϴ�.
        if (currentSaveSlot == 0)
        {
            Debug.LogError("���� ������ ������ ���õ��� �ʾҽ��ϴ�. ���� �޴����� ������ �����ߴ��� Ȯ���ϼ���.");
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

        Debug.Log("���� " + currentSaveSlot + "�� ���� �����Ͱ� ����Ǿ����ϴ�.");
    }
}