using UnityEngine;
using System.Collections; // System.Collections는 사용되지 않아 제거 가능하지만, 원본에 있었으므로 일단 유지
using UnityEngine.UI; // UnityEngine.UI는 사용되지 않아 제거 가능하지만, 원본에 있었으므로 일단 유지

public class BackButtonManager : MonoBehaviour
{
    public static BackButtonManager Instance { get; private set; }
    public bool IsPaused { get; private set; }

    private bool isMenuLocked = false;

    private GameObject optionUI;
    private CanvasGroup optionsCanvas;

    // 💡 1. 이전 커서 상태를 저장할 변수 추가
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 유지되도록 DontDestroyOnLoad(this.gameObject);를 추가하는 것이 일반적입니다. (필요하다면 추가)
        }
        else
        {
            Debug.LogWarning("PauseManager 인스턴스가 두 개 이상입니다.");
            Destroy(this.gameObject); // 일반적으로 싱글톤 중복 시 파괴
        }
    }

    void Start()
    {
        // Start() 로직은 원본과 동일
        if (optionUI == null)
        {
            optionUI = GameObject.FindWithTag("OptionUI");
            if (optionUI != null)
            {
                optionsCanvas = optionUI.GetComponent<CanvasGroup>();
                if (optionsCanvas != null)
                {
                    optionsCanvas.alpha = 0f;
                    optionsCanvas.interactable = false;
                    optionsCanvas.blocksRaycasts = false;
                }
            }
        }
        IsPaused = false; // 초기 상태는 일시정지 아님
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isMenuLocked)
        {
            Debug.Log("메뉴가 잠겨있어 열 수 없습니다.");
            return;
        }
        if (optionsCanvas == null)
        {
            Debug.LogError("'OptionUI' 또는 그 위의 CanvasGroup을 찾을 수 없습니다!");
            return;
        }

        IsPaused = !IsPaused;

        if (IsPaused)
        {
            // 💡 2. 일시정지(메뉴 열기) 직전에 현재 커서 상태를 저장합니다.
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Time.timeScale = 0f;
            optionsCanvas.alpha = 1f;
            optionsCanvas.interactable = true;
            optionsCanvas.blocksRaycasts = true;

            // 메뉴를 켰으니 커서는 무조건 보이게 하고 잠금 해제합니다.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            optionsCanvas.alpha = 0f;
            optionsCanvas.interactable = false;
            optionsCanvas.blocksRaycasts = false;

            // 💡 3. 일시정지 해제(메뉴 닫기) 시, 저장해 둔 이전 커서 상태를 복원합니다.
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }
    }

    public void SetMenuLock(bool isLocked)
    {
        isMenuLocked = isLocked;
    }
}