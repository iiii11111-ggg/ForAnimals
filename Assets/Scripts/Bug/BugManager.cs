using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BugManager : MonoBehaviour
{
    public static BugManager Instance;

    public GameObject bugPrefab;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public Button startButton;

    private int score = 0;
    private float gameTime = 30f;
    private bool isGameActive = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        scoreText.text = "Score: 0";
        timerText.text = "Time: 30";
        startButton.onClick.AddListener(StartGame);
    }

    void Update()
    {
        if (!isGameActive) return;

        gameTime -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(gameTime).ToString();

        if (gameTime <= 0 || score >= 30)
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
        StartCoroutine(SpawnBug());
    }

    void EndGame()
    {
        isGameActive = false;
        StopAllCoroutines();
        timerText.text = score >= 30 ? "You Win!" : "Game Over!";
    }

    IEnumerator SpawnBug()
    {
        while (isGameActive)
        {
            int bugCount = Random.Range(2, 3); // 3~5마리 한 번에 생성

            for (int i = 0; i < bugCount; i++)
            {
                float x = Random.Range(0.1f, 0.9f);
                float y = Random.Range(0.1f, 0.9f);
                Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(x, y, 0));
                worldPos.z = 0;
                Instantiate(bugPrefab, worldPos, bugPrefab.transform.rotation);
            }

            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
        }
    }



    public void AddScore(int amount)
    {
        if (!isGameActive) return;
        score += amount;
        scoreText.text = "Score: " + score.ToString();

        if (score >= 30)
        {
            EndGame();
        }
    }
}
