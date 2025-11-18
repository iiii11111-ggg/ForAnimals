// GameManager.cs
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public bool IsLoading { get; private set; } // 👈 1. '로딩 중' 상태를 알려줄 변수 추가

    public CanvasGroup FadeScreen;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadGameSlot(int slotIndex)
    {
        // 1. 앞으로 사용할 슬롯 번호를 저장합니다.
        PlayerData.currentSlotIndex = slotIndex;
        Debug.Log("DataManager에 슬롯 번호 " + slotIndex + " 저장됨.");

        if (Dialog.Instance != null)
        {
            Dialog.Instance.ResetDialogSystem();
            Debug.Log("<color=cyan><b>[GameManager] 씬 로드 시작 직전, Dialog 시스템을 선제적으로 초기화합니다.</b></color>");
        }

        // 2. 씬 로딩 및 데이터 적용 코루틴을 시작합니다.
        StartCoroutine(LoadSceneAndApplyData(slotIndex));
    }

    private IEnumerator LoadSceneAndApplyData(int slotIndex)
    {
        Debug.Log("<color=red><b>[GameManager] LOAD START! IsLoading을 TRUE로 설정합니다.</b></color>");
        IsLoading = true;

        string folderPath = Path.Combine(Application.dataPath, "../", "SAVE");
        string filePath = Path.Combine(folderPath, "SaveSlot" + slotIndex + ".json");
        GameData data;
        string sceneToLoad;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<GameData>(json);

            sceneToLoad = data.currentSceneName;

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError($"슬롯 {slotIndex}의 저장 파일에 씬 이름이 없습니다! 기본 씬으로 로드합니다.");
                sceneToLoad = "Tutorial";
            }
            Debug.Log("슬롯 " + slotIndex + " 데이터 로드 성공. 불러올 씬: " + sceneToLoad);
        }
        else
        {
            // 세이브 파일이 없으면 '새 게임'으로 간주합니다.
            Debug.Log("슬롯 " + slotIndex + "에 저장 파일이 없으므로 '새 게임'을 시작합니다.");

            // 새 게임을 위한 기본값 설정
            data = new GameData();
            data.characterPosition = new Vector3(1920, 32, -18); // 👈 새 게임 시작 위치
            sceneToLoad = "Tutorial"; // 👈 새 게임 시작 씬 이름
            data.currentSceneName = sceneToLoad;

            // 새 게임 데이터를 파일로 저장
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
        }

        LoadAndFadeOut();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!asyncLoad.isDone)
        {
            yield return null; // 씬 로드가 끝날 때까지 대기
        }

        // --- 씬 로딩 완료 후 ---
        Debug.Log("<color=yellow><b>[GameManager] 씬 로딩 완료. 이제 플레이어 위치를 적용합니다.</b></color>");

        // 씬에 있는 플레이어를 찾아서 위치를 적용합니다.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {

            PlayerController_Rabbit rabbit = player.GetComponent<PlayerController_Rabbit>();

            if (rabbit != null)
            {
                // Rabbit이 있으면 Rabbit의 TeleportTo 실행
                rabbit.TeleportTo(data.characterPosition);
                Debug.Log($"<color=yellow><b>[GameManager] 플레이어 위치 적용 완료: {data.characterPosition}</b></color>");
            }
            else
            {
                // Rabbit이 없으면 Monkey 컨트롤러를 가져와 봅니다.
                PlayerController_Monkey monkey = player.GetComponent<PlayerController_Monkey>();
                if (monkey != null)
                {
                    // Monkey가 있으면 Monkey의 TeleportTo 실행
                    monkey.TeleportTo(data.characterPosition);
                    Debug.Log($"<color=yellow><b>[GameManager] 플레이어 위치 적용 완료: {data.characterPosition}</b></color>");
                }
                else
                {
                    // 둘 다 없는 경우 (오류 처리)
                    Debug.LogWarning("플레이어에서 Rabbit 또는 Monkey 컨트롤러를 찾을 수 없습니다.", player);
                }
            }
            
        }
        else
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다! Player 태그를 확인해주세요.");
        }

        yield return new WaitForEndOfFrame();
        Debug.Log("<color=red><b>[GameManager] LOAD END! IsLoading을 FALSE로 설정합니다.</b></color>");
        IsLoading = false;
    }
    public void LoadAndFadeOut() 
    {
        StartCoroutine(Fade(1f,0f,1f));
    }
    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;

        FadeScreen.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            FadeScreen.alpha = newAlpha;

            yield return null;
        }

        FadeScreen.alpha = endAlpha;
    }
}