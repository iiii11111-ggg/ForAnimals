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
        script.Add("�ȳ�!");
        name.Add("�Ǵ�");
        script.Add("�� ���ӿ� �� �� ȯ����!");
        name.Add("�Ǵ�");
        script.Add("���ʿ� ������ ���� ������ ���� �� �� �־�!");
        name.Add("�Ǵ�");
        script.Add("������ �������� �ʸ� �����ٰž�.");
        name.Add("�Ǵ�");
        script.Add("������ �ΰ����� ������.");
        name.Add("�Ǵ�");
        script.Add("�ֱٿ� �츮�� �����ϴ� ����� �����ϱ� �����ŵ� Ư�� �����ϴ� �� �����ž�!.");
        name.Add("�Ǵ�");
        script.Add("������ ��մ� ������ ���� �ٶ���. ���߿� ����!");
        name.Add("�Ǵ�");
        Dialog.Instance.StartDialog(script,name);
    }

}
