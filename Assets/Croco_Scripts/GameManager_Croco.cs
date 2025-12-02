using UnityEngine;
using TMPro; // TMP 사용시
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager_Croco : MonoBehaviour
{
    public static GameManager_Croco Instance { get; private set; }

    public float surviveTime = 20f; // 20초 버티면 승리
    private float timer;
    public TextMeshProUGUI timerText; // TMPro를 사용안하면 UnityEngine.UI.Text 사용
    public TextMeshProUGUI resultText;
    private bool gameEnded = false;
    public GameObject restartBtn,finishBtn;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        Time.timeScale = 1f; // 혹시 일시정지 상태인지 방지
        if (timerText == null) Debug.LogError("TimerText가 GameManager에 연결되지 않았습니다.");
        if (resultText == null) Debug.LogWarning("ResultText가 비어있습니다.");
        timer = surviveTime;
        UpdateTimerUI();
        if (resultText) resultText.text = "";
    }

    void Update()
    {
        if (gameEnded) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            PlayerSurvived();
        }
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText)
        {
            timerText.text = $"Time: {timer:0.0}s";
        }
    }

    public void PlayerCaught()
    {
        if (gameEnded) return;
        gameEnded = true;
        if (resultText) resultText.text = "Game Over\n잡혔습니다!";
        restartBtn.SetActive(true);
        // 멈추기
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayerSurvived()
    {
        if (gameEnded) return;
        gameEnded = true;
        if (resultText) resultText.text = "Victory!\n30초 생존 성공!";
        Time.timeScale = 0f;
        finishBtn.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    public void Restart() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void finish() 
    {
        SaveManager.Instance.RecordAndSaveEventCompletion("0002");
        Time.timeScale = 1f;
        GameManager.Instance.LoadGameSlot(PlayerData.currentSlotIndex);
    }
}

