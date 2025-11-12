using System.Xml;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class Dialogue0002 : MonoBehaviour, IEventController
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
    public Transform CP, RP, MP, SavePoint;

    private Animator CA, RA, MA;


    public CinemachineCamera Cam1,Cam2,Cam3,Cam4,Cam5;

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

        EventManager.Instance.RequestEventStart(this);
        gameObject.GetComponent<Collider>().enabled = false;
    }
    public void ScriptStart()
    {
        dp = Dialog.Instance.dialogPanel;
        Text_0002 Script = GetComponent<Text_0002>();
        Script.StartDialog_0002();
        Dialog.Instance.OnIndexChanged += IndexChanged;

        // 첫 대화 씬 
        
        Rabbit.SetActive(true);
        Monkey.SetActive(true);

        Player_Rabbit.SetActive(false);
        Player_Monkey.SetActive(false);

        Croco.transform.position = CP.transform.position;
        Rabbit.transform.position = RP.transform.position;
        Rabbit.transform.LookAt(Croco.transform);
        Monkey.transform.position = MP.transform.position;
        Monkey.transform.LookAt(Rabbit.transform);

        StartCoroutine(Fade(1f, 0f,2f));
        brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
        Cam1.Priority = 11;
        StartCoroutine(DialogOpen(2f));


    }
    public void ScriptEnd()
    {
        dp = Dialog.Instance.dialogPanel;
        dp.SetActive(false);
        Dialog.Instance.OnIndexChanged -= IndexChanged;
    }

    void IndexChanged(int index) 
    {
        if (index == 0) 
        {
            Cam1.Priority = 11;
            Cam2.Priority = 9;
        }
        if (index == 1)
        {
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            RA.SetBool("isLookingOut", true);
            Cam1.Priority = 9;
            Cam2.Priority = 11;

        }
        else if (index == 2)
        {
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
            MA.SetBool("isTalking", true);
            Cam2.Priority = 9;
            Cam3.Priority = 11;
        }
        else if (index == 3)
        {
            CA.SetBool("isIdle", false);
            CA.SetBool("isAttacking_1", true);
            Cam3.Priority = 9;
            Cam4.Priority = 11;
        }
        else if (index == 4)
        {
            RA.SetBool("isLookingOut", false);
            RA.SetBool("isJumping_Dubble", true);

            StartCoroutine(AnimationReturn(RA, "isLookingOut","isJumping_Dubble", 1f));
            Cam4.Priority = 9;
            Cam5.Priority = 11;
        }
        else if (index == 5)
        {
            MA.SetBool("isTalking", false);
            CA.SetBool("isAttacking_1", false);
            RA.SetBool("isJumping_Dubble", false);
            Cam4.Priority = 9;
            Cam5.Priority = 9;

            SaveManager.Instance.SaveGameData(SavePoint.transform.position);
            SceneManager.LoadScene("Croco_InGame");
        }
    }
    IEnumerator DialogOpen(float t) 
    {
        yield return new WaitForSeconds(t);
        dp.SetActive(true);
    }
    IEnumerator AnimationReturn(Animator An,string Active, string NonActive, float Duration) 
    {
        yield return new WaitForSeconds(Duration);
        An.SetBool(NonActive, false);
        An.SetBool(Active, true);
        yield return null;
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
