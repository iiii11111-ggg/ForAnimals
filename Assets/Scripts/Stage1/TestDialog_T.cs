using UnityEngine;
using System.Collections.Generic;


public class TestDialog_T : MonoBehaviour
{
    List<string> script = new List<string>();
    List<string> name = new List<string>();

    public void StartTutorialDialog()
    {
        // ★★★ 함수가 호출될 때 데이터가 비어있으면 그때 채워줍니다 ★★★
        if (script.Count == 0)
        {
            script.Add("안녕!");
            name.Add("판다");
            script.Add("또 만났네. 이번엔 미니게임을 설명해줄거야.");
            name.Add("판다");
            script.Add("미니게임은 들어가서 주어진 과제를 수행하면 돼.");
            name.Add("판다");
            script.Add("그럼 미니게임을 해볼래?");
            name.Add("판다");
            Debug.Log($"[RabbitToPanda_T] 대화 데이터 생성 완료. script 개수: {script.Count}");
        }

        Debug.Log("StartDialog 실행 가능상태");
        Dialog.Instance.StartDialog(script, name);
    }
}
