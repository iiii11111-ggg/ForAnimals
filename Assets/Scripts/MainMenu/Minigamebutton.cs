using UnityEngine;
using UnityEngine.UI;

public class Minigamebutton : MonoBehaviour
{
    public Button Button1;
    public Button Button2;
    public Button Button3;
    void Start()
    {
        Button1.onClick.RemoveAllListeners();
        Button2.onClick.RemoveAllListeners();
        Button3.onClick.RemoveAllListeners();

        if (OptionManager.instance != null)
        {

            buttonManager btnm = OptionManager.instance.buttonM.GetComponent<buttonManager>();
            if (btnm != null) 
            {
                Button1.onClick.AddListener(btnm.minigame1);
                Button2.onClick.AddListener(btnm.minigame2);
                Button3.onClick.AddListener(btnm.minigame3);
            } 
        }
        else
        {
            Debug.LogError("싱글톤 인스턴스를 찾을 수 없습니다!");
        }
    }
}
