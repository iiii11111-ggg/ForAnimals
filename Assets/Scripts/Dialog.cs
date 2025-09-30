using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System;

public class Dialog : MonoBehaviour
{

    public static Dialog Instance { get; private set;}

    public Text text;
    public Text speaker;
    List<string> ScriptLog = new List<string>();
    List<string> script = new List<string>();
    List<string> name = new List<string>();
    public int index { get; private set;  }

    private bool NextActive;
    public bool EndDialog;

    public event Action<int> OnIndexChanged;

    void Awake()
    {
        // --- 싱글톤 중복 방지 로직 ---
        if (Instance == null)
        {
            // 이 인스턴스가 최초 생성된 인스턴스
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
            // LoadCount(); // PlayerPrefs 사용할 경우 여기서 로드
        }
        NextActive = true;
        EndDialog = false;
    }

    public void StartDialog(List<string> _script,List<string> _name)
    {
        index = 0;
        script = _script;
        name = _name;
        text.text = script[index];
        speaker.text = name[index];
        ScriptLog.Clear();
        EndDialog = false;
    }


    public void RPDialog_T_Next()
    {
        if (index == script.Count - 1)
        {
            EndDialog = true;
        }
        else if (NextActive)
        {
            NextActive = false;
            index++;
            OnIndexChanged?.Invoke(index);
            text.text = script[index];
            speaker.text = name[index];
            ScriptLog.Add(script[index - 1]);
            StartCoroutine(nextTalk());
        }
    }
    public void RPDialog_T_Previous()
    {
        if (index == 0)
        {
            print("더이상 갈 수 없다.");
        }
        else if (NextActive)
        {
            NextActive = false;
            index--;
            OnIndexChanged?.Invoke(index);
            text.text = ScriptLog[index];
            speaker.text = name[index];
            ScriptLog.RemoveAt(index);
            StartCoroutine(previousTalk());
        }
    }

    IEnumerator nextTalk()
    {
        yield return new WaitForSeconds(0.2f);
        NextActive = true;
    }
    IEnumerator previousTalk()
    {
        yield return new WaitForSeconds(0.2f);
        NextActive = true;
    }


}
