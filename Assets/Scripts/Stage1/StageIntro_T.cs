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

    public void intro() 
    {
        script.Clear();
        name.Clear();
        name.Add("원숭이");
        script.Add("으아아아아!! 내 몸이 왜이래?! ");
        name.Add("???");
        script.Add("너의 만행들을 보고 벌을 내리노라!");
        name.Add("원숭이");
        script.Add("내 몸 돌려줘!!");
        name.Add("???");
        script.Add("그동안 동물들을 괴롭혔으니 진심으로 늬우친다면 생각해보지.. 후후");
        name.Add("원숭이");
        script.Add("장난해?! 으윽");
        name.Add("토끼");
        script.Add("원숭아 왜 무슨일이야?");
        name.Add("토끼");
        script.Add("어라? 우리집은 왜이래?!");
        name.Add("원숭이");
        script.Add("하.. 어떤 사람이... ");     
        name.Add("토끼");
        script.Add("뭐라고? 그 놈 당장 찾으러 가자!");
        Dialog.Instance.StartDialog(script, name);
    }
}
