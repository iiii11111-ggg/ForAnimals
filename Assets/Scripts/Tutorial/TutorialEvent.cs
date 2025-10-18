using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.Events;
using System.Xml;

public class TutorialEvent : MonoBehaviour, IEventController
{
    [Header("Save System ID")]
    [SerializeField] private string uniqueID; // 인스펙터에서 설정

    [Header("Custom Start Actions")]
    [SerializeField] private UnityEvent onEventStart; // 인스펙터에서 설정할 동작들

    [SerializeField] private UnityEvent onEventEnd;

    [Header("Object References")]
    public GameObject player;
    public GameObject playerPosition;
    public GameObject panda;
    public GameObject chatEvent;
    public GameObject finalArrow;
    public GameObject HelpMessage;

    private GameObject dp;

    [Header("Components")]
    public PlayerController_Rabbit pc;
    public Animator an;
    public CinemachineCamera pCam, cCam;

    [Header("Save System ID")]
    public string UniqueID => uniqueID;
    public UnityEvent OnEventStart => onEventStart;

    public UnityEvent OnEventEnd => onEventEnd;

    private bool hasTriggered = false;

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
    }

    void Start()
    {
        pc = player.GetComponent<PlayerController_Rabbit>();
        an = player.GetComponent<Animator>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        EventManager.Instance.RequestEventStart(this);
    }
    public void ScriptStart() 
    {
        Debug.Log("튜토리얼 대화 시작");
        dp = Dialog.Instance.dialogPanel;
        RabbitToPanda_T Script = dp.GetComponentInChildren<RabbitToPanda_T>();
        Script.StartTutorialDialog();
        pc.enabled = false;
        HelpMessage.SetActive(false);
        an.SetBool("isRunning", false);
        pc.rb.linearVelocity = Vector3.zero;
        StartCoroutine(talkPanda());
    }
    public void ScriptEnd()
    {
        Debug.Log("튜토리얼 대화 종료");
        pc.enabled = true;
        dp.SetActive(false);
        pCam.Priority = cCam.Priority + 1;
        chatEvent.SetActive(false);
        HelpMessage.SetActive(true);
        finalArrow.SetActive(true);
    }

    IEnumerator talkPanda()
    {
        yield return new WaitForSeconds(0.2f);
        dp.SetActive(true);
        cCam.Priority = pCam.Priority + 1;
        pc.transform.position = playerPosition.transform.position;
        pc.transform.LookAt(panda.transform);
    }
}