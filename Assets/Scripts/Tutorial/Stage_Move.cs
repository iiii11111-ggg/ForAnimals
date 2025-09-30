using Unity.Cinemachine;
using UnityEngine;

public class Stage_Move : MonoBehaviour
{
       public GameObject player,position,tutorial,startEvent;
    public CinemachineCamera FollowCam, StartCam;


    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            tutorial.SetActive(false);
            startEvent.SetActive(true);
            player.transform.position = position.transform.position;
            StartCam.Priority = FollowCam.Priority + 1;
        }
    }
}
