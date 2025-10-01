// GameManager.cs
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadGameSlot(int slotIndex, string sceneName)
    {
        // 1. ������ ����� ���� ��ȣ�� �����մϴ�.
        PlayerData.currentSlotIndex = slotIndex;
        Debug.Log("DataManager�� ���� ��ȣ " + slotIndex + " �����.");

        // 2. �� �ε� �� ������ ���� �ڷ�ƾ�� �����մϴ�.
        StartCoroutine(LoadSceneAndApplyData(sceneName));
    }

    private IEnumerator LoadSceneAndApplyData(string sceneName)
    {
        // --- �� �ε� ---
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null; // �� �ε尡 ���� ������ ���
        }

        // --- �� �ε� ���� �۾� (���� �߿�!) ---

        // 1. Dialog ���� �̱��� �ý����� '����' �ʱ�ȭ�մϴ�.
        if (Dialog.Instance != null)
        {
            Dialog.Instance.ResetDialogSystem();
            Debug.Log("<color=lime><b>[GameManager] Dialog System �ʱ�ȭ �Ϸ�.</b></color>");
        }

        // 2. ����� ���� �����͸� �ε��մϴ�. (���� InGameSceneLoader�� ����)
        int slotToLoad = PlayerData.currentSlotIndex;
        string folderPath = Path.Combine(Application.dataPath, "../", "SAVE");
        string filePath = Path.Combine(folderPath, "SaveSlot" + slotToLoad + ".json");
        GameData data;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("���� " + slotToLoad + " ������ �ε� ����. ������ġ: " + data.characterPosition);
        }
        else
        {
            // ���̺� ������ ������ �⺻������ ���� ����
            data = new GameData();
            data.characterPosition = new Vector3(24, 0, 5); // �⺻ ���� ��ġ

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
            Debug.Log("���� " + slotToLoad + "�� ���ο� ���� ���� ������.");
        }

        // 3. ���� �ִ� �÷��̾ ã�Ƽ� ��ġ�� �����մϴ�.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.characterPosition;
            Debug.Log("<color=yellow>�÷��̾� ��ġ ���� �Ϸ�.</color>");
        }
        else
        {
            Debug.LogError("�÷��̾� ������Ʈ�� ã�� �� �����ϴ�! Player �±׸� Ȯ�����ּ���.");
        }
    }
}