using UnityEngine;

public class OptionManager : MonoBehaviour
{
    public static OptionManager instance = null;

    public GameObject optionUi;
    public GameObject buttonM;

    private void Awake()
    {
        // --- 싱글톤 패턴 구현 ---
        // 인스턴스가 아직 생성되지 않았다면
        if (instance == null)
        {
            // 이 인스턴스를 static instance 변수에 할당합니다.
            instance = this;

            // 씬이 전환되어도 이 게임 오브젝트와 그 자식(EventSystem 등)이
            // 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
        }
        // 인스턴스가 이미 존재하고, 이 인스턴스가 아니라면
        else if (instance != this)
        {
            // 이미 씬에 SingletonManager가 존재하므로, 이 게임 오브젝트는
            // 중복 생성을 막기 위해 파괴합니다.
            Destroy(gameObject);
        }
    }

}
