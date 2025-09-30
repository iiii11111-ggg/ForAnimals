using UnityEngine;
using Unity.Collections;
using System.Collections;
using Unity.Cinemachine;

public class TutorialEvent : MonoBehaviour
{

    public GameObject player,dp,playerPosition,panda,chatEvent,finalArrow,HelpMessage;
    public PlayerController pc;
    public Animator an;
    public CinemachineCamera pCam, cCam;

    

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
                Script.enabled = false;
                pc.enabled = true;
                dp.SetActive(false);
                pCam.Priority = cCam.Priority + 1;
                chatEvent.SetActive(false);
                HelpMessage.SetActive(true);
                finalArrow.SetActive(true);
                Dialog.Instance.EndDialog = false;
            }
        }   
    }


    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            RabbitToPanda_T Script = dp.GetComponentInChildren<RabbitToPanda_T>();
            Script.enabled = true;
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
