using System.Xml;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class AutoTutorial : MonoBehaviour
{

    [Header("UI Components")]
    [Tooltip("튜토리얼 전체를 감싸는 캔버스 (또는 패널)")]
    public GameObject tutorialCanvas;

    [Tooltip("순서대로 1, 2, 3번 스크린을 할당 (사이즈 3)")]
    public GameObject[] screens;

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button completeButton;

    public string UniqueID;
    public GameObject InGameUI;

    // 현재 튜토리얼 단계 (0: 첫번째, 1: 두번째, 2: 세번째)
    private int currentStage = 0;

    // 튜토리얼 시작 전 원래 커서 상태 저장용 (선택 사항)
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    void Awake()
    {
        int currentSlotIndex = PlayerData.currentSlotIndex;
        if (string.IsNullOrEmpty(UniqueID))
        {
            Debug.LogError("Auto tutorial의 uniqueID가 설정되지 않았습니다!", gameObject);
            return;
        }

        if (SaveManager.Instance.HasBeenDestroyed(currentSlotIndex, UniqueID))
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작 시 튜토리얼 캔버스는 꺼둡니다.
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);

        // 버튼에 기능 연결
        nextButton.onClick.AddListener(OnNextBtn);
        prevButton.onClick.AddListener(OnPrevBtn);
        completeButton.onClick.AddListener(OnCompleteBtn);

        gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 재진입 버그 방지를 위해 콜라이더 즉시 끄기
            GetComponent<Collider>().enabled = false;

            // 2. 튜토리얼 시작
            StartTutorial();
        }
    }

    private void StartTutorial()
    {
        if (BackButtonManager.Instance != null)
        {
            BackButtonManager.Instance.SetMenuLock(true);
        }

        // 현재 커서 상태 저장 (끝나고 되돌리기 위함)
        previousLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        // 시간 정지 및 커서 보이기
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // UI 초기화
        tutorialCanvas.SetActive(true);
        currentStage = 0;

        InGameUI.SetActive(false);

        // 스크린 초기화: 0번만 켜고 나머지는 끔
        screens[0].SetActive(true);
        for (int i = 1; i < screens.Length; i++)
        {
            screens[i].SetActive(false);
        }

        UpdateButtons();
    }

    // 다음 버튼 기능
    private void OnNextBtn()
    {
        // 마지막 단계가 아니라면
        if (currentStage < screens.Length - 1)
        {
            currentStage++;
            // 누적 방식(Type B): 해당 단계 스크린을 켬 (이전 스크린은 켜진 상태 유지)
            screens[currentStage].SetActive(true);

            UpdateButtons();
        }
    }

    // 이전 버튼 기능
    private void OnPrevBtn()
    {
        // 첫 단계가 아니라면
        if (currentStage > 0)
        {
            // 누적 방식(Type B): 현재 단계 스크린을 끔 (이전 스크린이 뒤에 보임)
            screens[currentStage].SetActive(false);
            currentStage--;

            UpdateButtons();
        }
    }

    // 완료 버튼 기능
    private void OnCompleteBtn()
    {
        if (BackButtonManager.Instance != null)
        {
            BackButtonManager.Instance.SetMenuLock(false);
        }

        // 1. 시간 재개 및 커서 상태 복구
        Time.timeScale = 1f;
        Cursor.lockState = previousLockMode; // 혹은 CursorLockMode.Locked 등 강제 설정 가능
        Cursor.visible = previousCursorVisible; // 혹은 false 등 강제 설정 가능

        // 2. 튜토리얼 캔버스 끄기
        tutorialCanvas.SetActive(false);

        InGameUI.SetActive(true);

        SaveManager.Instance.RecordAndSaveEventCompletion(UniqueID);


        Debug.Log("튜토리얼 완료! 추가 로직이 실행됩니다.");

        
    }

    // 단계에 따라 버튼 상태 갱신
    private void UpdateButtons()
    {
        // 초기화: 일단 다 끔
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
        completeButton.gameObject.SetActive(false);

        if (currentStage == 0)
        {
            // 1번 스크린: 다음 버튼만
            nextButton.gameObject.SetActive(true);
        }
        else if (currentStage == 1)
        {
            // 2번 스크린: 이전, 다음 버튼
            prevButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
        }
        else if (currentStage == 2)
        {
            // 3번 스크린: 이전, 완료 버튼 (다음 버튼 숨김)
            prevButton.gameObject.SetActive(true);
            completeButton.gameObject.SetActive(true);
        }
    }


}
