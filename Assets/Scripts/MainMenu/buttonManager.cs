using UnityEngine;
using UnityEngine.SceneManagement;
public class buttonManager : MonoBehaviour
{
   

    public void Startbutton()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("main ui");
    }
    public void Exitbutton()
    {
        Application.Quit();//���α׷� ����
    }
    
    public void backbutton()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("Main");
    }
    public void minigame1()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("BugCrash");
    }
    public void minigame2()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("Maze");
    }
    public void minigame3()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("index");
    }
    public void minigame4()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("index");
    }
    public void savebutton()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("InGame");
    }
    public void savebutton2()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void savebutton3()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void mainexitbtn()//public���־�� �Լ� ������ ������
    {
        SceneManager.LoadScene("Main");
    }
    public void exitbtn()//public���־�� �Լ� ������ ������
    {
        Application.Quit();
    }
}