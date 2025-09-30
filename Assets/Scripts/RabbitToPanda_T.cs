using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class RabbitToPanda_T : MonoBehaviour
{
    List<string> script = new List<string>();
    List<string> name = new List<string>();

    void Start()
    {
        script.Add("안녕!");
        name.Add("판다");
        script.Add("이 게임에 온 걸 환영해!");
        name.Add("판다");
        script.Add("뒤쪽에 문으로 들어가면 게임을 시작 할 수 있어!");
        name.Add("판다");
        script.Add("수많은 동물들이 너를 도와줄거야.");
        name.Add("판다");
        script.Add("하지만 인간들은 조심해.");
        name.Add("판다");
        script.Add("최근에 우리를 위협하는 사람이 있으니깐 만나거든 특히 조심하는 게 좋을거야!.");
        name.Add("판다");
        script.Add("앞으로 재밌는 게임을 즐기길 바랄게. 나중에 보자!");
        name.Add("판다");
        Dialog.Instance.StartDialog(script,name);
    }

}
