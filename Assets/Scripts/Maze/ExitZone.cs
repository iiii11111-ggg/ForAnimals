using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public GameManager_Maze gameManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.Win();
            Debug.Log("탈출 성공!");
        }
        Debug.Log("무언가 닿음: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 닿음!");
            if (gameManager != null)
            {
                gameManager.Win();
            }
            else
            {
                Debug.LogError("GameManager 연결 안 됨!");
            }
        }
    }

}

