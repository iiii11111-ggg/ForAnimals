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
        name.Add("��ü�Ҹ��� ������");
        script.Add("�ʿ��� ���� �������!!!");
        name.Add("��ü�Ҹ��� ������");
        script.Add("�������� �ݼ��Ѵٸ� ��������.");
        name.Add("???");
        script.Add("��������!? �� �߸������... ����!!!");
        name.Add("???");
        script.Add("�ƴ� ������..");
        Dialog.Instance.StartDialog(script, name);
    }
    public void intro2() 
    {
        script.Clear();
        name.Clear();
        name.Add("������");
        script.Add("����...");
        name.Add("�䳢");
        script.Add("������ �ֱ׷�?");
        name.Add("������");
        script.Add("�� ���� �̻���..");
        name.Add("�䳢");
        script.Add("����? �����ѵ�? ");     
        name.Add("������");
        script.Add("...���� ���� ��������");
        name.Add("�䳢");
        script.Add("�ƴ� ���� �׷�����?? �츮���� ���ư���... �̴�� �����! ");
        name.Add("�䳢");
        script.Add("������. �����Ϸ� ����!!");
        name.Add("������");
        script.Add("��..?");
        name.Add("�����̼�");
        script.Add("(�󶳰ῡ ��������.)");
        Dialog.Instance.StartDialog(script, name);
    }
}
