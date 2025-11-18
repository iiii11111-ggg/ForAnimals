using System.Collections.Generic;
using UnityEngine;

public class Text_0003 : MonoBehaviour
{
    List<string> script = new List<string>();
    List<string> name = new List<string>();

    public void StartDialog_0003()
    {
        if (script.Count == 0)
        {
            script.Add("왜 먹이를 못먹은거야..?");
            name.Add("토끼");
            script.Add("... 인간들이 여기에 쓰레기를 계속 버려서 물고기가 다 죽어버렸어..");
            name.Add("악어");
            script.Add("우리집도 인간들이 부쉈어!");
            name.Add("토기");
            script.Add("여기선 못살아.. 얼른 도망가!!");
            name.Add("악어");
            Debug.Log($"[RabbitToPanda_T] 대화 데이터 생성 완료. script 개수: {script.Count}");
        }

        Debug.Log("StartDialog 실행 가능상태");
        Dialog.Instance.StartDialog(script, name);
    }
}

