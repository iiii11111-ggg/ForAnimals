using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class TutorialEvent : MonoBehaviour
{
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
    public string uniqueID;

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

    void OnEnable()
    {
        Debug.Log($"<color=cyan><b>[청취자] OnEnable: '{this.gameObject.name}'이(가) Dialog 방송을 구독합니다.</b></color>");
        if (Dialog.Instance != null)
        {
            Dialog.Instance.OnDialogEnded += HandleDialogEnd;
        }
    }

    void OnDisable()
    {
        // 이 스크립트가 비활성화되는 바로 그 순간, 콘솔에 경고 로그와 함께 호출 스택 전체를 출력합니다.
        Debug.LogWarning($"!!! '{this.gameObject.name}' 오브젝트의 TutorialEvent 스크립트가 비활성화되었습니다. 범인은 아래 호출 스택에 있습니다:", this.gameObject);

        // System.Diagnostics.StackTrace는 누가 이 코드를 실행시켰는지에 대한 아주 상세한 "CCTV 영상"을 제공합니다.
        Debug.Log(new System.Diagnostics.StackTrace().ToString());

        Debug.Log($"<color=red><b>[청취자] OnDisable: '{this.gameObject.name}'이(가) Dialog 방송 구독을 해지합니다.</b></color>");
        if (Dialog.Instance != null)
        {
            Dialog.Instance.OnDialogEnded -= HandleDialogEnd;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        dp = Dialog.Instance.dialogPanel;

        RabbitToPanda_T Script = dp.GetComponentInChildren<RabbitToPanda_T>();
        Script.StartTutorialDialog();
        pc.enabled = false;
        HelpMessage.SetActive(false);
        an.SetBool("isRunning", false);
        pc.rb.linearVelocity = Vector3.zero;
        StartCoroutine(talkPanda());
    }

    private void HandleDialogEnd()
    {
        Debug.LogError("이벤트 핸들러 호출 완료");
        pc.enabled = true;
        dp.SetActive(false);
        pCam.Priority = cCam.Priority + 1;
        chatEvent.SetActive(false);
        HelpMessage.SetActive(true);
        finalArrow.SetActive(true);

        int currentSlotIndex = PlayerData.currentSlotIndex;
        SaveManager.Instance.MarkAsDestroyed(currentSlotIndex, uniqueID);
        SaveManager.Instance.SaveGameData();

        gameObject.SetActive(false);
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