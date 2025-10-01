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
        // 1. 앞으로 사용할 슬롯 번호를 저장합니다.
        PlayerData.currentSlotIndex = slotIndex;
        Debug.Log("DataManager에 슬롯 번호 " + slotIndex + " 저장됨.");

        // 2. 씬 로딩 및 데이터 적용 코루틴을 시작합니다.
        StartCoroutine(LoadSceneAndApplyData(sceneName));
    }

    private IEnumerator LoadSceneAndApplyData(string sceneName)
    {
        // --- 씬 로딩 ---
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null; // 씬 로드가 끝날 때까지 대기
        }

        // --- 씬 로딩 직후 작업 (가장 중요!) ---

        // 1. Dialog 같은 싱글톤 시스템을 '먼저' 초기화합니다.
        if (Dialog.Instance != null)
        {
            Dialog.Instance.ResetDialogSystem();
            Debug.Log("<color=lime><b>[GameManager] Dialog System 초기화 완료.</b></color>");
        }

        // 2. 저장된 게임 데이터를 로드합니다. (기존 InGameSceneLoader의 로직)
        int slotToLoad = PlayerData.currentSlotIndex;
        string folderPath = Path.Combine(Application.dataPath, "../", "SAVE");
        string filePath = Path.Combine(folderPath, "SaveSlot" + slotToLoad + ".json");
        GameData data;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("슬롯 " + slotToLoad + " 데이터 로드 성공. 저장위치: " + data.characterPosition);
        }
        else
        {
            // 세이브 파일이 없으면 기본값으로 새로 생성
            data = new GameData();
            data.characterPosition = new Vector3(24, 0, 5); // 기본 시작 위치

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
            Debug.Log("슬롯 " + slotToLoad + "에 새로운 게임 파일 생성됨.");
        }

        // 3. 씬에 있는 플레이어를 찾아서 위치를 적용합니다.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.characterPosition;
            Debug.Log("<color=yellow>플레이어 위치 적용 완료.</color>");
        }
        else
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다! Player 태그를 확인해주세요.");
        }
    }
}