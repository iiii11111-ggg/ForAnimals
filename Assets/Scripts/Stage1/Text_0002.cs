using System.Collections.Generic;
using UnityEngine;

public class Text_0002 : MonoBehaviour
{
    List<string> script = new List<string>();
    List<string> name = new List<string>();

    public void StartDialog_0002()
    {
        if (script.Count == 0)
        {
            script.Add("으하하 이게 얼마만의 밥이냐~~");
            name.Add("악어");
            script.Add("으아아악!!");
            name.Add("토끼");
            script.Add("갑자기 왜이래?!");
            name.Add("원숭이");
            script.Add("토끼 먹어버리겠다!!");
            name.Add("악어");
            script.Add("으아 잠깐만!!");
            name.Add("토끼");
            Debug.Log($"[RabbitToPanda_T] 대화 데이터 생성 완료. script 개수: {script.Count}");
        }

        Debug.Log("StartDialog 실행 가능상태");
        Dialog.Instance.StartDialog(script, name);
    }
}

