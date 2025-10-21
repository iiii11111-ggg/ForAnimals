// NPC.cs
using System.Xml;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour, IInteractable, IEventController
{
    [Header("Object")]
    public GameObject PIZ, player;

    public CinemachineCamera PIC;

    private GameObject dp;

    [Header("Save System ID")]
    [SerializeField] private string uniqueID;

    [Header("Custom Start Actions")]
    [SerializeField] private UnityEvent onEventStart;

    [SerializeField] private UnityEvent onEventEnd;

    [Header("System Interface")]
    public string UniqueID => uniqueID;
    public UnityEvent OnEventStart => onEventStart;

    public UnityEvent OnEventEnd => onEventEnd;

    

    void Awake()
    {
        int currentSlotIndex = PlayerData.currentSlotIndex;
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError("TestNPC의 uniqueID가 설정되지 않았습니다!", gameObject);
            return;
        }

        if (SaveManager.Instance.HasBeenDestroyed(currentSlotIndex, uniqueID))
        {
            Destroy(gameObject);
        }
    }


    public void Interact()
    {
        Debug.Log("상호작용 시작! (NPC.cs)");

        EventManager.Instance.RequestEventStart(this);
    }

    // 대화 시스템이 종료되었을 때 호출될 콜백 함수
    private void OnDialogueFinished()
    {
        Debug.Log("대화 종료됨. (NPC.cs)");

        // 플레이어를 찾아 상호작용이 끝났다고 알려줌
        PlayerInteraction player = FindObjectOfType<PlayerInteraction>();
        if (player != null)
        {
            player.OnInteractionEnd();
        }
        else
        {
            Debug.LogWarning("PlayerInteraction 컴포넌트를 찾을 수 없습니다.");
        }
    }
    public void ScriptStart() 
    {
        dp = Dialog.Instance.dialogPanel;
        TestDialog_T Script = GetComponent<TestDialog_T>();
        Script.StartTutorialDialog();
        PlayerController_Monkey pm = player.GetComponent<PlayerController_Monkey>();
        pm.canMove = false;
        dp.SetActive(true);
        PIC.Priority = 11;
        pm.TeleportTo(PIZ.transform.position);
        pm.transform.LookAt(transform);
    }
    public void ScriptEnd()
    {
        OnDialogueFinished();
        dp = Dialog.Instance.dialogPanel;
        TestDialog_T Script = GetComponent<TestDialog_T>();
        Script.StartTutorialDialog();
        PlayerController_Monkey pm = player.GetComponent<PlayerController_Monkey>();
        pm.canMove = false;
        dp.SetActive(true);
        PIC.Priority = 11;
        pm.TeleportTo(PIZ.transform.position);
        pm.transform.LookAt(transform);
    }
}