using UnityEngine;
using UnityEngine.SceneManagement;
public class Gamebuttonmanager : MonoBehaviour
{
    public void Exitbutton()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("index");//���Ӿ����� ��ȯ
    }
}
