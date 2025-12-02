using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BugManager : MonoBehaviour
{
    public static BugManager Instance;

    public GameObject bugPrefab;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public Button startButton;

    // Inspector에서 할당해야 합니다.
    public GameObject RestartBtn, FinishBtn;

    // 💡 1. 초기 점수를 25로 설정 (잡아야 할 벌레 수)
    private int score = 25;
    private float gameTime = 15f;
    private bool isGameActive = false;

    // 💡 수정: 총 생성될 최대 벌레 수 (초기 점수와 동일하게 설정)
    private int maxBugsToSpawn;
    // 💡 수정: 현재까지 생성된 벌레 수
    private int currentBugsSpawned = 0;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // 💡 수정: 최대 스폰 횟수를 초기 점수와 동일하게 설정 (25마리)
        maxBugsToSpawn = score;

        // 게임 시작 전에는 UI 숨김 (StartButton만 활성화)
        scoreText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        RestartBtn.SetActive(false);
        FinishBtn.SetActive(false);

        // 💡 2. "남은 마리수"로 UI 업데이트
        UpdateScoreUI();
        timerText.text = "Time: 15";
        startButton.onClick.AddListener(StartGame);

        // Restart 및 Finish 버튼 리스너 추가
        if (RestartBtn != null) RestartBtn.GetComponent<Button>().onClick.AddListener(RestartGame);
        if (FinishBtn != null) FinishBtn.GetComponent<Button>().onClick.AddListener(FinishGame);

        // TimeScale이 0으로 고정되어 있을 경우를 대비하여 1로 설정
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!isGameActive) return;

        gameTime -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(gameTime).ToString();

        // 💡 3. 종료 조건 수정: 시간이 0 이하가 되거나 점수가 0 이하가 되면 종료
        if (gameTime <= 0 || score <= 0)
        {
            EndGame();
        }
    }

    void StartGame()
    {
        isGameActive = true;
        startButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        Time.timeScale = 1f; // 혹시 0으로 되어있을 경우 다시 설정
        StartCoroutine(SpawnBug());
    }

    void EndGame()
    {
        isGameActive = false;
        StopAllCoroutines();
        // EndGame 진입 시 시간 정지 (버튼 클릭 전까지 게임 화면 고정)
        Time.timeScale = 0f;

        bool win = score <= 0;

        // 💡 4. 승리 조건에 따라 텍스트 및 버튼 활성화
        if (win)
        {
            timerText.text = "You Win!";
            FinishBtn.SetActive(true); // 승리 시 Finish 버튼 활성화
        }
        else
        {
            timerText.text = "Game Over!";
            RestartBtn.SetActive(true); // 패배 시 Restart 버튼 활성화
        }
    }

    // 💡 5. 재시작 함수
    void RestartGame()
    {
        Time.timeScale = 1f; // 게임 시간을 다시 정상화
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬을 다시 로드
    }

    // 💡 6. 메인 씬 이동 함수
    void FinishGame()
    {
        Time.timeScale = 1f; // 게임 시간을 다시 정상화
        // "Main" 씬으로 이동합니다. (Build Settings에 "Main" 씬이 추가되어 있어야 합니다.)
        SceneManager.LoadScene("Main");
    }

    // "남은 마리수" UI 업데이트를 위한 헬퍼 함수
    void UpdateScoreUI()
    {
        scoreText.text = "남은 마리수: " + score.ToString();
    }


    IEnumerator SpawnBug()
    {
        while (isGameActive)
        {
            // 💡 수정: 스폰된 벌레 수가 최대치를 초과하면 코루틴 종료
            if (currentBugsSpawned >= maxBugsToSpawn)
            {
                yield break; // 코루틴 종료
            }
            int remainingSpawns = maxBugsToSpawn - currentBugsSpawned;
            int bugCount = Mathf.Min(Random.Range(1, 4), remainingSpawns); // Random.Range(1, 3)은 1 또는 2 반환

            for (int i = 0; i < bugCount; i++)
            {
                float x = Random.Range(0.1f, 0.9f);
                float y = Random.Range(0.1f, 0.9f);
                Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(x, y, 0));
                worldPos.z = 0;
                Instantiate(bugPrefab, worldPos, bugPrefab.transform.rotation);

                // 💡 수정: 벌레를 생성할 때마다 스폰된 수 증가
                currentBugsSpawned++;
            }

            // 💡 수정: 스폰된 벌레 수가 최대치를 초과하면, 루프를 종료하기 전에 남은 시간이 0이 될 때까지 기다리지 않고
            // 다음 루프에서 `yield break`로 코루틴이 종료되도록 설정
            // 즉시 스폰이 멈추지 않고, 다음 딜레이 후에 확인됩니다. (시간 차이가 거의 없음)
            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
        }
    }


    public void AddScore(int amount)
    {
        if (!isGameActive) return;

        // 💡 7. 점수 감소 (역순)
        score -= amount;

        // 점수가 음수가 되는 것을 방지
        if (score < 0) score = 0;

        // 💡 8. "남은 마리수" UI 업데이트
        UpdateScoreUI();

        // 💡 9. 점수가 0 이하가 되면 승리 처리
        if (score <= 0)
        {
            EndGame();
        }
    }
}