using UnityEngine;
using UnityEngine.SceneManagement;

public class screenbtnmanager : MonoBehaviour
{
    public void screenbutton()
    {
        SceneManager.LoadScene("screen ui");
    }
    public void soundbutton()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("sound ui");
    }
    public void exitbutton()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("main ui");
    }
}
