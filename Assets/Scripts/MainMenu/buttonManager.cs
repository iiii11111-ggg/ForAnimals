using UnityEngine;
using UnityEngine.SceneManagement;
public class buttonManager : MonoBehaviour
{
    public GameObject optionUiActive;
    private GameObject dialog;
    public GameObject CompleteExplainScreen;

    public void Startbutton()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("main ui");
    }
    public void Exitbutton()
    {
        Application.Quit();//프로그램 종료
    }
    
    public void backbutton()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("Main");
    }
    public void minigame1()//public이있어야 함수 연결이 가능함
    {
        optionUiActive.SetActive(true);
        SceneManager.LoadScene("BugCrash");
        
    }
    public void minigame2()//public이있어야 함수 연결이 가능함
    {
        optionUiActive.SetActive(true);
        SceneManager.LoadScene("Maze");

    }
    public void minigame3()//public이있어야 함수 연결이 가능함
    {
        optionUiActive.SetActive(true);
        SceneManager.LoadScene("Croco");
    }
    public void minigame4()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("index");
    }
    public void savebutton()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("InGame");
    }
    public void savebutton2()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void savebutton3()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void mainexitbtn()//public이있어야 함수 연결이 가능함
    {
        EventManager.Instance.OnDialogQuit();
        dialog = Dialog.Instance.dialogPanel;
        dialog.SetActive(false);
        BackButtonManager.Instance.TogglePause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        optionUiActive.SetActive(false);
        SceneManager.LoadScene("Main");
    }
    public void exitbtn()//public이있어야 함수 연결이 가능함
    {
        Application.Quit();
    }
    public void ControllerOpen() 
    {
        CompleteExplainScreen.SetActive(true);
    }
    public void ControllerClose() 
    {
        CompleteExplainScreen.SetActive(false);
    }
}