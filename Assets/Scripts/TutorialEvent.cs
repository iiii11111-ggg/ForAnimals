using UnityEngine;
using Unity.Collections;
using System.Collections;
using Unity.Cinemachine;
using System.Xml;

public class TutorialEvent : MonoBehaviour
{

    public GameObject player,dp,playerPosition,panda,chatEvent,finalArrow,HelpMessage;
    public PlayerController pc;
    public Animator an;
    public CinemachineCamera pCam, cCam;

    public string uniqueID;

    void Awake()
    {
        int currentSlotIndex = PlayerData.currentSlotIndex;

        // 1. ID가 설정되었는지 확인
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError("PermanentObject 스크립트의 uniqueID가 설정되지 않았습니다! 오브젝트 이름: " + gameObject.name);
            return;
        }

        // 2. 관리자를 통해 파괴 기록이 있는지 확인
        if (SaveManager.Instance.HasBeenDestroyed(currentSlotIndex, uniqueID))
        {
            // 3. 기록이 있으면 즉시 자신을 파괴
            Destroy(gameObject);
        }
    }
    public void DestroySelfPermanently()
    {
        // 💡 1. 싱글톤에서 현재 슬롯 인덱스(int)를 직접 가져옵니다.
        int currentSlotIndex = PlayerData.currentSlotIndex;

        // 2. 관리자에게 현재 슬롯 인덱스와 함께 파괴 기록 요청
        SaveManager.Instance.MarkAsDestroyed(currentSlotIndex, uniqueID);

        // 3. 현재 게임에서 자신을 파괴
        Destroy(gameObject);
    }

    void Start()
    {
        pc = player.GetComponent<PlayerController>();   
        an = player.GetComponent<Animator>();
    }

    void Update()
    {
        if (Dialog.Instance != null) 
        {
            if (Dialog.Instance.EndDialog)
            {
                RabbitToPanda_T Script = dp.GetComponentInChildren<RabbitToPanda_T>();
                pc.enabled = true;
                dp.SetActive(false);
                pCam.Priority = cCam.Priority + 1;
                chatEvent.SetActive(false);
                HelpMessage.SetActive(true);
                finalArrow.SetActive(true);
                SaveManager.Instance.SaveGameData();
                DestroySelfPermanently();
            }
        }   
    }


    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player tag confirmed."); 
            RabbitToPanda_T Script = dp.GetComponentInChildren<RabbitToPanda_T>();
            Script.StartTutorialDialog(); 
            pc.enabled = false;
            HelpMessage.SetActive(false);
            an.SetBool("isRunning", false);
            pc.rb.linearVelocity = Vector3.zero;
            StartCoroutine(talkPanda());
        }
    }

    IEnumerator talkPanda() {

        yield return new WaitForSeconds(0.2f);
        dp.SetActive(true);
        cCam.Priority = pCam.Priority + 1;
        pc.transform.position = playerPosition.transform.position;
        pc.transform.LookAt(panda.transform);
    }
}
