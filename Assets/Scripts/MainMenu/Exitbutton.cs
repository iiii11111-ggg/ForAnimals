using UnityEngine;
using UnityEngine.SceneManagement;
public class Gamebuttonmanager : MonoBehaviour
{
    public void Exitbutton()//public이있어야 함수 연결이 가능함
    {
        SceneManager.LoadScene("index");//게임씬으로 전환
    }
}
