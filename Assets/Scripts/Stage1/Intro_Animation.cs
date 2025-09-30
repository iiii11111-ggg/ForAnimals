using Polyperfect.People;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Intro_Animation : MonoBehaviour
{
    private AudioSource mainAudioSource;
    public AudioClip backgroundMusicJungle;

    public CinemachineCamera FollowCam, StartCam, BoomCam,MonkeyCam, RabbitCam, RabbitFocusingCam,introEndCam;
    public StageIntro_T script;
    public GameObject player, ChatPoint, dialog, startMap, BrokenMap, nils, monkey,EndPoint;
    public Animator RabbitAn,MonkeyAn;
    public CanvasGroup CG;
    public bool EventTrigger, NotLoop;
    private int sequence;

    public Light spotLight;
    public AudioSource thunderSound;



    void OnEnable()
    {
        script = GetComponent<StageIntro_T>();
        sequence = 0;
        EventTrigger = false;
        mainAudioSource = Camera.main.GetComponent<AudioSource>();
        mainAudioSource.clip = backgroundMusicJungle;
        mainAudioSource.loop = true; // 반복 재생 설정
        mainAudioSource.Play();
        StartCoroutine(startScene(ChatPoint.transform.position));
    }

    void Update()
    {
        float distance = Vector3.Distance(player.transform.position, ChatPoint.transform.position);
        if (sequence == 0 && distance <= 1f)
        {
            StartCoroutine(BoomChat());
            sequence++;
        }
        if (sequence == 1 && Dialog.Instance.EndDialog)
        {
            StartCoroutine(BoomScene());
            sequence++;
        }
        if (sequence == 2 && EventTrigger)
        {
            player.transform.LookAt(monkey.transform);
            dialog.SetActive(true);
            script.intro2();
            CinemachineBrain brain = FindObjectOfType<CinemachineBrain>();
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            MonkeyCam.Priority = BoomCam.Priority + 1;
            Dialog.Instance.OnIndexChanged += IndexChanged;
            EventTrigger = false;
        }


    }

    IEnumerator startScene(Vector3 targetPosition)
    {


        float moveDuration = 6.0f;
        Vector3 startPosition = player.transform.position;
        float elapsedTime = 0f;


        player.SetActive(false);
        yield return new WaitForSeconds(2.0f);
        player.SetActive(true);
        RabbitAn.SetBool("isRunning", true);

        while (elapsedTime < moveDuration)
        {

            float t = elapsedTime / moveDuration;

            t = Mathf.SmoothStep(0.0f, 1.0f, t);
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            if (targetPosition != player.transform.position)
            {
                player.transform.LookAt(targetPosition);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        RabbitAn.SetBool("isRunning", false);
    }

    IEnumerator introEndScene(Vector3 targetPosition)
    {

        float moveDuration = 10.0f;
        Vector3 startPositionP = player.transform.position;
        Vector3 startPositionM = monkey.transform.position;
        float elapsedTime = 0f;

        monkey.GetComponent<Animator>().enabled = true;
        MonkeyAn.SetBool("isWalking", true);

        while (elapsedTime < moveDuration)
        {
            RabbitAn.SetBool("isRunning", true);
            float t = elapsedTime / moveDuration;

            t = Mathf.SmoothStep(0.0f, 1.0f, t);
            player.transform.position = Vector3.Lerp(startPositionP, targetPosition, t);
            monkey.transform.position = Vector3.Lerp(startPositionM, targetPosition, t);

            if (targetPosition != player.transform.position)
            {
                player.transform.LookAt(targetPosition);
                monkey.transform.LookAt(targetPosition);

            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator BoomChat()
    {
        CG.alpha = 1;
        BoomCam.Priority = StartCam.Priority + 1;
        dialog.SetActive(true);
        script.intro1();
        yield return null;
    }
    IEnumerator BoomScene()
    {
        dialog.SetActive(false);
        StartCoroutine(Fade(1f, 0f, 2f));
        yield return new WaitForSeconds(0.5f);
        //번개 치기
        spotLight.enabled = true;
        yield return new WaitForSeconds(0.2f);
        thunderSound.Play();
        spotLight.enabled = false;
        yield return new WaitForSeconds(0.1f);
        spotLight.enabled = true;
        yield return new WaitForSeconds(0.2f);
        spotLight.enabled = false;
        yield return new WaitForSeconds(0.1f);
        spotLight.enabled = true;
        yield return new WaitForSeconds(0.1f);
        spotLight.enabled = false;
        yield return new WaitForSeconds(0.1f);


        StartCoroutine(Fade(0f, 1f, 2f));
        yield return new WaitForSeconds(1.5f);
        startMap.SetActive(false);
        BrokenMap.SetActive(true);

        StartCoroutine(Fade(1f, 0f, 2f));

        yield return new WaitForSeconds(4.0f);
        EventTrigger = true;
    }
    void IndexChanged(int index) 
    {

        if (index == 1)
        {
            CinemachineBrain brain = FindObjectOfType<CinemachineBrain>();
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
            RabbitCam.Priority = MonkeyCam.Priority + 1;
        }
        else if (index == 2) 
        {
            monkey.transform.LookAt(player.transform);
            MonkeyCam.Priority = RabbitCam.Priority + 1;
        }
        else if (index == 3)
        {
            RabbitCam.Priority = MonkeyCam.Priority + 1;
        }
        else if (index == 4)
        {
            MonkeyCam.Priority = RabbitCam.Priority + 1;
        }
        else if (index == 5)
        {
            RabbitCam.Priority = MonkeyCam.Priority + 1;
        }
        else if (index == 6)
        {
            RabbitFocusingCam.Priority = RabbitCam.Priority + 1;
        }
        else if (index == 7)
        {
            MonkeyCam.Priority = RabbitCam.Priority + 1;
        }
        else if (index == 8)
        {
            Vector3 Rposition = new Vector3(player.transform.position.x + 4, player.transform.position.y, player.transform.position.z);
            monkey.transform.position = Rposition;
            monkey.transform.LookAt(player.transform);
            StartCoroutine(introEndScene(EndPoint.transform.position));
            introEndCam.Priority = MonkeyCam.Priority + 1;
            StartCoroutine(Fade(0f, 1f, 8f));
        }

    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            CG.alpha = newAlpha;
            yield return null;
            CG.alpha = endAlpha;


        }
    }
}
