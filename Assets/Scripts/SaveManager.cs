// SaveManager.cs (싱글톤 적용 버전)
using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    // 1. 싱글톤 인스턴스 변수 추가
    public static SaveManager Instance { get; private set; }

    // playerTransform은 더 이상 public일 필요가 없습니다.
    private Transform playerTransform;

    public CanvasGroup SaveCompleteText;

    private bool isTransitioning = false;

    // 2. Awake() 함수에서 인스턴스 설정
    void Awake()
    {

        if (Instance != null && Instance != this)
        {

            Debug.LogWarning($"중복 SaveManager 파괴: '{this.gameObject.name}'. 원본은 '{Instance.gameObject.name}' 입니다.");
            Destroy(this.gameObject);
            return; 
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

    }
   
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 3. 씬 로드 이벤트 구독 해제(Unsubscribe) (메모리 누수 방지)
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 4. 씬 로드가 완료될 때마다 이 함수가 자동 호출됨
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isLoading = (GameManager.Instance != null && GameManager.Instance.IsLoading);

        Debug.Log($"<color=cyan><b>[SaveManager] OnSceneLoaded 실행됨. 현재 로딩 중인가? => {isLoading}</b></color>");
        if (isLoading)
        {
            Debug.Log("<color=cyan><b>[SaveManager] 게임 로딩 중이므로 자동 저장을 건너뜁니다.</b></color>");
            return;
        }
        // 메인 메뉴나 타이틀 씬 등 저장할 필요 없는 씬은 제외
        if (scene.name == "Main" || scene.name == "BugCrash"||scene.name == "Croco"|| scene.name == "Croco_InGame" || scene.name == "Maze")
        {
            return;
        }

        if (isTransitioning)
        {
            Debug.Log("[SaveManager] 씬 전환 완료. 저장 잠금을 해제하지만, 씬 로드 직후 자동 저장은 건너뜁니다.");
            isTransitioning = false;
            return; // 씬 로드 직후, 초기 위치로 덮어쓰는 것도 방지
        }

        // 씬이 로드된 후 플레이어를 찾음 (?. 연산자로 null 체크)
        playerTransform = GameObject.FindWithTag("Player")?.transform;

        if (playerTransform != null)
        {
            // 씬 로드 완료 시, '현재 씬 이름'과 '플레이어 위치'를 자동 저장
            // SaveCurrentProgress(scene.name, playerTransform.position);
            Debug.Log($"[SaveManager] {scene.name} 씬 로드됨. 플레이어 참조 갱신 완료.");
        }
        else
        {
            Debug.LogWarning(scene.name + " 씬에 'Player' 태그 오브젝트가 없습니다.");
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

        // 💡 핵심: 현재 씬 이름과 플레이어 위치를 가져와서 범용 저장 함수를 호출합니다.
        string currentScene = SceneManager.GetActiveScene().name;
        SaveCurrentProgress(currentScene, playerTransform.position);

        Debug.Log("슬롯 " + PlayerData.currentSlotIndex + "에 게임 데이터가 수동 저장되었습니다.");

    }
    public void SaveGameData(Vector3 specificPosition)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SaveCurrentProgress(currentScene, specificPosition);

        Debug.Log("지정된 위치(" + specificPosition + ")를 수동 저장했습니다.");
        
    }
    public void SaveGameData(string specificScene, Vector3 specificPosition)
    {
        // 1. 매개변수로 받은 씬 이름과 위치를 핵심 저장 함수로 넘겨줍니다.
        SaveCurrentProgress(specificScene, specificPosition);

        // 2. (선택) 일관성을 위해 로그 및 UI 피드백을 동일하게 호출합니다.
        Debug.Log("지정된 씬(" + specificScene + ")과 위치(" + specificPosition + ")를 수동 저장했습니다.");
    }
    public void RecordAndSaveEventCompletion(string eventUniqueID)
    {
        if (string.IsNullOrEmpty(eventUniqueID))
        {
            Debug.LogWarning("[SaveManager] eventUniqueID가 null이거나 비어있어, 이벤트 완료 저장을 건너뜁니다.");
            return;
        }

        Debug.Log($"[SaveManager] 이벤트 완료 저장 시작: {eventUniqueID}");

        // 1. 이벤트 완료 상태 저장 (PlayerPrefs 사용)
        int currentSlotIndex = PlayerData.currentSlotIndex;
        MarkAsDestroyed(currentSlotIndex, eventUniqueID);

        // 2. 이벤트 ID에 따른 분기 저장
        if (eventUniqueID == "0001")
        {
            // 튜토리얼 완료 등 특별한 저장 지점이 필요할 때
            Debug.Log($"[SaveManager] 특수 이벤트 '{eventUniqueID}' 완료. Jungle 위치로 저장합니다.");
            SaveGameData("Jungle", new Vector3(334, 1, 172));

            isTransitioning = true;
        }
        else if (eventUniqueID == "0002")
        {
            Debug.Log($"[SaveManager] 특수 이벤트 '{eventUniqueID}' 완료. Jungle 위치로 저장합니다.");
            SaveGameData("Jungle", new Vector3(313, 18, 380));
        }
        else if (eventUniqueID == "0003")
        {
            Debug.Log($"[SaveManager] 특수 이벤트 '{eventUniqueID}' 완료. Jungle 위치로 저장합니다.");
        }
        else if (eventUniqueID == "0004")
        {
            Debug.Log($"[SaveManager] 특수 이벤트 '{eventUniqueID}' 완료. Jungle 위치로 저장합니다.");
            SaveGameData("Jungle", new Vector3(253, 15 , 228));
        }
        else if (eventUniqueID == "1000") 
        {
            Debug.Log($"[SaveManager] 특수 이벤트 '{eventUniqueID}' 완료. Jungle 위치로 저장합니다.");
            SaveGameData("Jungle", new Vector3(330, 0, 177));
        }
        else
        {
            // 일반적인 이벤트 완료 시 자동 저장
            Debug.Log($"[SaveManager] 일반 이벤트 '{eventUniqueID}' 완료. 현재 상태로 게임을 저장합니다.");
            SaveGameData();
        }
    }

    public void SaveCurrentProgress(string sceneName, Vector3 position)
    {
        int currentSaveSlot = PlayerData.currentSlotIndex;
        if (currentSaveSlot == 0)
        {
            Debug.LogError("현재 저장할 슬롯이 선택되지 않았습니다.");
            return;
        }

        string folderPath = Path.Combine(Application.persistentDataPath, "../", "SAVE");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string filePath = Path.Combine(folderPath, "SaveSlot" + currentSaveSlot + ".json");

        GameData data = new GameData();
        data.characterPosition = position;
        data.currentSceneName = sceneName; // 👈 확장된 GameData에 씬 이름 저장

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);

        StartCoroutine(FadeInOut(0.5f,2f));

        Debug.Log($"슬롯 {currentSaveSlot}에 자동 저장 완료 (씬: {sceneName}, 위치: {position})");

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

        // 이미 저장된 적이 있다면 중복 저장 방지
        if (PlayerPrefs.GetInt(key, 0) == 1) return;

        // 1. 해당 오브젝트 파괴 상태 저장 (기존 로직)
        PlayerPrefs.SetInt(key, 1);

        // 2. [추가된 로직] 이 슬롯이 사용하는 '키 목록'에 방금 저장한 키 이름을 추가
        RegisterKeyToSlot(saveSlotIndex, key);

        PlayerPrefs.Save();
        Debug.Log($"슬롯 [{saveSlotIndex}]에 ID: {objectID} 파괴 기록 및 키 등록 완료.");
    }

    private void RegisterKeyToSlot(int slotIndex, string newKey)
    {
        string listKey = GetSlotKeyListKey(slotIndex);
        string currentList = PlayerPrefs.GetString(listKey, "");

        if (string.IsNullOrEmpty(currentList))
        {
            currentList = newKey;
        }
        else
        {
            currentList += "," + newKey; // 콤마로 구분해서 이어붙임
        }

        PlayerPrefs.SetString(listKey, currentList);
    }

    private string GetSlotKeyListKey(int slotIndex)
    {
        // 예: "Slot_1_KeyList" -> 이 안에 "Chest01,Event02,Boss01" 처럼 저장됨
        return $"Slot_{slotIndex}_KeyList";
    }
    public void ClearDestroyedObjectHistory()
    {
        Debug.Log("모든 파괴된 오브젝트 기록을 삭제합니다.");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
    // 📌 특정 슬롯의 데이터(JSON + PlayerPrefs)를 삭제하는 함수
    public void DeleteSaveData(int slotIndex)
    {
        // 1. JSON 파일 삭제
        string folderPath = Path.Combine(Application.persistentDataPath, "../", "SAVE");
        string filePath = Path.Combine(folderPath, "SaveSlot" + slotIndex + ".json");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"슬롯 {slotIndex}의 JSON 파일 삭제 완료.");
        }

        // 2. PlayerPrefs 삭제 (저장해둔 장부를 보고 삭제)
        string listKey = GetSlotKeyListKey(slotIndex);
        if (PlayerPrefs.HasKey(listKey))
        {
            string allKeys = PlayerPrefs.GetString(listKey);
            string[] keys = allKeys.Split(','); // 콤마로 쪼개기

            foreach (string key in keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    PlayerPrefs.DeleteKey(key); // 기록된 키 개별 삭제
                }
            }

            // 마지막으로 '키 목록' 자체도 삭제
            PlayerPrefs.DeleteKey(listKey);
            PlayerPrefs.Save();

            Debug.Log($"슬롯 {slotIndex}과 관련된 모든 PlayerPrefs 데이터를 삭제했습니다.");
        }
        else
        {
            Debug.LogWarning($"슬롯 {slotIndex}의 PlayerPrefs 기록이 없거나, 구버전 데이터라 키 목록이 없습니다.");
        }
    }
    public bool CheckSaveFileExists(int slotIndex, out string sceneName)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "../", "SAVE");
        string filePath = Path.Combine(folderPath, "SaveSlot" + slotIndex + ".json");

        sceneName = "";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // 씬 이름이 비어있으면 기본값, 아니면 저장된 값 반환
            sceneName = string.IsNullOrEmpty(data.currentSceneName) ? "Unknown" : data.currentSceneName;
            return true;
        }

        return false;
    }

    IEnumerator FadeInOut(float fadeDuration, float holdDuration)
    {
        // 1. 페이드 인 (Alpha 0 -> 1)
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        yield return StartCoroutine(WaitForRealSeconds(holdDuration));

        // 3. 페이드 아웃 (Alpha 1 -> 0)
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;

        SaveCompleteText.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;

            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            SaveCompleteText.alpha = newAlpha;

            yield return null;
        }

        SaveCompleteText.alpha = endAlpha;
    }

    IEnumerator WaitForRealSeconds(float time)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + time)
        {
            yield return null;
        }
    }
}