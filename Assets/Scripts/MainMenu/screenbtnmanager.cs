using UnityEngine;
using UnityEngine.SceneManagement;

public class screenbtnmanager : MonoBehaviour
{
    public void screenbutton()
    {
        SceneManager.LoadScene("screen ui");
    }
    public void soundbutton()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("sound ui");
    }
    public void exitbutton()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("main ui");
    }
}
