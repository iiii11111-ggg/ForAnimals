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

    public GameObject dialogPanel;

    public event Action<int> OnIndexChanged;

    public event Action OnDialogEnded;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"<color=lime><b>[싱글톤] Dialog (ID:{GetInstanceID()})가 유일한 인스턴스로 등록되었습니다.</b></color>");
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"<color=orange><b>[싱글톤] 중복된 Dialog (ID:{GetInstanceID()})가 파괴됩니다.</b></color>");
            Destroy(gameObject);
        }
        NextActive = true;
        EndDialog = false;
    }

    void OnDestroy()
    {
        Debug.LogError($"<color=red><b>!!!!! [싱글톤] Dialog (ID:{GetInstanceID()})가 파괴되었습니다!!!!!</b></color>");

        // 만약 현재 파괴되는 놈이 유일한 인스턴스였다면, static 변수를 비워줘야 합니다.
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartDialog(List<string> _script,List<string> _name)
    {
        Debug.Log($"[Dialog] StartDialog: 대화 데이터를 받음. 전달된 script 개수: {_script.Count}");
        index = 0;
        script = _script;
        name = _name;
        text.text = script[index];
        Debug.Log($"[Dialog] StartDialog: Text 입력 확인 : {text.text}");
        Debug.Log($"[Dialog] StartDialog: index 값 확인 : {index}");
        speaker.text = name[index];
        ScriptLog.Clear();
        EndDialog = false;

        Debug.Log($"[Dialog] StartDialog: 내부 script 리스트에 데이터 할당 완료. 현재 script 개수: {script.Count}"); 
        Debug.Log($"[Dialog] StartDialog: 내부 script 리스트에 데이터 할당 완료. 현재 script 텍스트: {script[0]}"); 
}

    public void ResetDialogSystem()
    {
        EndDialog = false;
        index = 0;
        NextActive = true;

        script.Clear();
        name.Clear();
        ScriptLog.Clear();

        if (text != null)
        {
            text.text = "";
        }
        if (speaker != null)
        {
            speaker.text = "";
        }
    }


    public void RPDialog_T_Next()
    {
        print("다음을 눌렀다");
        if (index == script.Count - 1)
        {
            EndDialog = true;

            int subscriberCount = OnDialogEnded?.GetInvocationList().Length ?? 0;

            Debug.Log($"<color=yellow><b>[방송국] 이벤트 송출 직전! 현재 구독자 수: {subscriberCount} 명</b></color>");


            OnDialogEnded?.Invoke();
        }
        else if (NextActive)
        {
            NextActive = false;
            index++;
            OnIndexChanged?.Invoke(index);
            Debug.Log($"[Dialog] NextDialog: index 값 확인 :{index}");
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
