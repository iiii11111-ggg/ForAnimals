using System.Collections.Generic;
using UnityEngine;

public class Text_0004 : MonoBehaviour
{
    List<string> script = new List<string>();
    List<string> name = new List<string>();

    public void StartDialog_0004()
    {
        if (script.Count == 0)
        {
            script.Add("휴.. 겨우 도망쳤어");
            name.Add("원숭이");
            script.Add("강에 쓰레기가 이렇게 많다니.. 물고기가 하나도 안보였어.");
            name.Add("토끼");
            script.Add("...");
            name.Add("원숭이");
            script.Add("토끼야. 나 사실... 인간이야.");
            name.Add("원숭이");
            script.Add("알아.");
            name.Add("토끼");
            script.Add("뭐??");
            name.Add("원숭이");
            script.Add("집으로 돌아오는 길에서 인간인 너를 봤어.");
            name.Add("토끼");
            script.Add("뭔가 번쩍하더니 원숭이로 변해있었어.");
            name.Add("토끼");
            script.Add("다 봤구나..");
            name.Add("원숭이");
            script.Add("왜 우리집을 부순거야?");
            name.Add("토끼");
            script.Add("난 그저.. 시키는대로 한 것 뿐이야. 미안.");
            name.Add("원숭이");
            script.Add("그래도 동물들 생각도 해줘야지.");
            name.Add("토끼");
            script.Add("생각이 짧았어.. 근데 넌 왜 내가 인간인걸 알면서 같이가자고 한거야?");
            name.Add("원숭이");
            script.Add("네가 억지로 하는것 같았거든.");
            name.Add("토끼");
            script.Add("...");
            name.Add("원숭이");
            script.Add("나는 사실 인간들과 어울려서 사는 게 꿈이었어. 그래서 인간인 너를 알고싶었거든.");
            name.Add("토끼");
            script.Add("난... 원숭이로 변하고나서 알았어.");
            name.Add("원숭이");
            script.Add("동물들의 세계도 인간의 세계와 다르지 않다는 걸 말이야.");
            name.Add("원숭이");
            script.Add("그저 더 약하기 때문에 피해를 받고있다는 걸 알았어.");
            name.Add("원숭이");
            script.Add("내가 원래대로 돌아갈 수 있을지 모르겠지만 나는...");
            name.Add("원숭이");
            script.Add("지금부터는 동물을 위해서 살아보고 싶어.");
            name.Add("원숭이");
            script.Add("저기.. 우리집에 가보지않을래? 인간들의 세상을 보여주고 싶어.");
            name.Add("원숭이");
            script.Add("좋아! 나 인간들의 집도 보고싶어!");
            name.Add("토끼");
            script.Add("그래. 마침 배가 있으니까 타고가자!");
            name.Add("원숭이");

            Debug.Log($"[RabbitToPanda_T] 대화 데이터 생성 완료. script 개수: {script.Count}");
        }

        Debug.Log("StartDialog 실행 가능상태");
        Dialog.Instance.StartDialog(script, name);
    }
}

