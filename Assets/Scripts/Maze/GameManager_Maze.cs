using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Cinemachine;

public class GameManager_Maze : MonoBehaviour
{
    public float timeLimit = 60f; // 제한 시간 (초)
    private float timer;

    public Camera mainCam;
    public TextMeshProUGUI timerText;    // TMP로 변경!
    public GameObject gameOverPanel,player;
    public GameObject winPanel, StartButton,RestartButton,titleText;
    public CinemachineCamera StartCam, FollowCam;

    private bool isGameOver = false;
    private bool NotStart = false;

    void Start()
    {
        timer = timeLimit;
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;
        if (NotStart)
        {
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timer).ToString();
        }

        if (timer <= 0f)
        {
            GameOver();
        }
    }
    public void GameStart()
    {
        StartButton.SetActive(false);
        titleText.SetActive(false);
        NotStart = true;
        player.GetComponent<PlayerMovement>().enabled = true;
        FollowCam.Priority = StartCam.Priority + 1;
        mainCam.cullingMask = ~(1 << LayerMask.NameToLayer("Player"));
        timerText.gameObject.SetActive(true);

    }

    public void Win()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        winPanel.SetActive(true);
        timerText.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        RestartButton.SetActive(true);
        timerText.gameObject.SetActive(false);
    }
    public void Restart() 
    {
        SceneManager.LoadScene("Maze");
    }
}
