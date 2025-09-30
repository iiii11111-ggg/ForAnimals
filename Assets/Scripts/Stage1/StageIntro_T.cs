using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;


public class StageIntro_T : MonoBehaviour
{
    List<string> script = new List<string>();
    List<string> name = new List<string>();

    public void intro1() 
    {
        script.Clear();
        name.Clear();
        name.Add("정체불명의 누군가");
        script.Add("너에게 벌을 내리노라!!!");
        name.Add("정체불명의 누군가");
        script.Add("진심으로 반성한다면 돌려주지.");
        name.Add("???");
        script.Add("누구세요!? 전 잘못없어요... 으악!!!");
        name.Add("???");
        script.Add("아니 내몸이..");
        Dialog.Instance.StartDialog(script, name);
    }
    public void intro2() 
    {
        script.Clear();
        name.Clear();
        name.Add("원숭이");
        script.Add("으윽...");
        name.Add("토끼");
        script.Add("원숭아 왜그래?");
        name.Add("원숭이");
        script.Add("내 몸이 이상해..");
        name.Add("토끼");
        script.Add("뭐가? 멀쩡한데? ");     
        name.Add("원숭이");
        script.Add("...몰라 누가 괴롭혔어");
        name.Add("토끼");
        script.Add("아니 누가 그런거지?? 우리집도 날아갔어... 이대론 못살아! ");
        name.Add("토끼");
        script.Add("원숭아. 복수하러 가자!!");
        name.Add("원숭이");
        script.Add("어..?");
        name.Add("나레이션");
        script.Add("(얼떨결에 끌려간다.)");
        Dialog.Instance.StartDialog(script, name);
    }
}
