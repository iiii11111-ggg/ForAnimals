using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public GameManager_Maze gameManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.Win();
            Debug.Log("Ż�� ����!");
        }
        Debug.Log("���� ����: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("�÷��̾ ����!");
            if (gameManager != null)
            {
                gameManager.Win();
            }
            else
            {
                Debug.LogError("GameManager ���� �� ��!");
            }
        }
    }

}

