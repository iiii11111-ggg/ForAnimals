using System.Xml;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class Dialogue0003 : MonoBehaviour, IEventController
{
    [Header("Save System ID")]
    [SerializeField] private string uniqueID; // 인스펙터에서 설정
    [SerializeField] private UnityEvent onEventStart;
    [SerializeField] private UnityEvent onEventEnd;

    public string UniqueID => uniqueID;
    public UnityEvent OnEventStart => onEventStart;
    public UnityEvent OnEventEnd => onEventEnd;

    private bool hasTriggered = false;

    [Header("Object Refer")]
    public GameObject Rabbit;
    public GameObject Monkey;
    public GameObject Croco;
    public GameObject Player_Rabbit;
    public GameObject Player_Monkey;
    public CanvasGroup FadeScreen;
    public Transform CP, RP, MP;
    public Transform SavePoint;

    private Animator CA, RA, MA;


    public CinemachineCamera Cam1,Cam2,Cam3;

    [Header("Others")]
    private GameObject dp;
    CinemachineBrain brain;

    void Awake()
    {
        int currentSlotIndex = PlayerData.currentSlotIndex;
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError("TutorialEvent의 uniqueID가 설정되지 않았습니다!", gameObject);
            return;
        }

        if (SaveManager.Instance.HasBeenDestroyed(currentSlotIndex, uniqueID))
        {
            Destroy(gameObject);
        }

        brain = Object.FindAnyObjectByType<CinemachineBrain>();
        CA = Croco.GetComponent<Animator>();
        MA = Monkey.GetComponent<Animator>();
        RA = Rabbit.GetComponent<Animator>();

    }
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        StartCoroutine(FadeOut(1f, 1f));

        EventManager.Instance.RequestEventStart(this);
        gameObject.GetComponent<Collider>().enabled = false;
    }
    public void ScriptStart()
    {
        dp = Dialog.Instance.dialogPanel;
        Text_0003 Script = GetComponent<Text_0003>();
        Script.StartDialog_0003();
        Dialog.Instance.OnIndexChanged += IndexChanged;

        // 첫 대화 씬 

        Rabbit.SetActive(true);
        Monkey.SetActive(true);

        Player_Rabbit.SetActive(false);
        Player_Monkey.SetActive(false);

        Croco.transform.position = CP.transform.position;
        Croco.transform.LookAt(Rabbit.transform);
        Rabbit.transform.position = RP.transform.position;
        Rabbit.transform.LookAt(Croco.transform);
        Monkey.transform.position = MP.transform.position;
        Monkey.transform.LookAt(Croco.transform);

        StartCoroutine(AnimatorTimeControl(0.7f,CA,"isDead"));

        brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
        Cam1.Priority = 11;
        StartCoroutine(DialogOpen(3.5f));


    }
    public void ScriptEnd()
    {
        SaveManager.Instance.SaveGameData(SavePoint.transform.position);

        dp = Dialog.Instance.dialogPanel;
        dp.SetActive(false);
        Dialog.Instance.OnIndexChanged -= IndexChanged;

        SaveManager.Instance.RecordAndSaveEventCompletion("0003");

        Rabbit.SetActive(false);
        Monkey.SetActive(false);

        Player_Rabbit.SetActive(true);
    }

    void IndexChanged(int index)
    {
        if (index == 0)
        {
            Cam1.Priority = 11;
        }
        else if (index == 2)
        {
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            Cam1.Priority = 9;
            Cam2.Priority = 11;
        }
        else if (index == 3)
        {
            Cam2.Priority = 9;
            Cam3.Priority = 11;
        }
        else if (index == 4) 
        {
            Cam3.Priority = 9;
        }

    }
    IEnumerator DialogOpen(float t)
    {
        yield return new WaitForSeconds(t);
        dp.SetActive(true);
    }

    IEnumerator AnimatorTimeControl(float WatingTime, Animator An,string AnName) 
    {
        yield return new WaitForSeconds(WatingTime);
        An.SetBool(AnName, true);
    }


    IEnumerator FadeOut(float fadeDuration, float holdDuration)
    {
        CanvasGroup fadeScreen = FadeScreen;

        FadeScreen.alpha = 1;

        yield return new WaitForSeconds(holdDuration);

        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;

        FadeScreen.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            FadeScreen.alpha = newAlpha;

            yield return null;
        }

        FadeScreen.alpha = endAlpha;
    }
}
