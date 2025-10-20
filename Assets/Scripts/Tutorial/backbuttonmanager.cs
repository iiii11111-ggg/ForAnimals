using UnityEngine;

public class BackButtonManager : MonoBehaviour
{
    public static BackButtonManager Instance { get; private set; }
    public bool IsPaused { get; private set; }

    private GameObject optionUI;
    private CanvasGroup optionsCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("PauseManager 인스턴스가 두 개 이상입니다.");
        }
    }

    void Start()
    {
        // 시작할 때 UI를 찾아오고, 비활성화 해둡니다.
        if (optionUI == null)
        {
            optionUI = GameObject.FindWithTag("OptionUI");
            optionsCanvas = optionUI.GetComponent<CanvasGroup>();
            if (optionUI != null)
            {
                // CanvasGroup 컴포넌트를 가져옵니다.
                optionsCanvas = optionUI.GetComponent<CanvasGroup>();
                // 게임 시작 시에는 보이지 않도록 설정합니다.
                optionsCanvas.alpha = 0f;
                optionsCanvas.interactable = false;
                optionsCanvas.blocksRaycasts = false;
            }
        }
        IsPaused = false; // 초기 상태는 일시정지 아님
    }

    void Update()
    {
        // ESC 키 입력을 여기서 전담합니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // public으로 선언하여 UI 버튼 등에서도 호출할 수 있습니다.
    public void TogglePause()
    {
        if (optionsCanvas == null)
        {
            Debug.LogError("'OptionUI' 또는 그 위의 CanvasGroup을 찾을 수 없습니다!");
            return;
        }

        IsPaused = !IsPaused;

        if (IsPaused)
        {
            Time.timeScale = 0f;
            optionsCanvas.alpha = 1f;
            optionsCanvas.interactable = true;
            optionsCanvas.blocksRaycasts = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            optionsCanvas.alpha = 0f;
            optionsCanvas.interactable = false;
            optionsCanvas.blocksRaycasts = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
