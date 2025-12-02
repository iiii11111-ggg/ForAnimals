using UnityEngine;

public class AudioChanger_J : MonoBehaviour
{
    void Start()
    {
        // 씬이 시작될 때 (Start) BGM을 전환하도록 명령합니다.
        if (SoundManager.Instance != null)
        {
            // 1. 싱글턴 인스턴스에 접근
            // 2. SoundManager의 ChangeBGM 함수 호출
            SoundManager.Instance.SetBGMJungle();
        }
    }
}
