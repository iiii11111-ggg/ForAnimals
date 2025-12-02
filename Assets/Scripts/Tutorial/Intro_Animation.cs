using Polyperfect.People;
using System.Collections;
using System.Xml;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Intro_Animation : MonoBehaviour
{
    private AudioSource mainAudioSource;
    public AudioClip backgroundMusicJungle;

    public CinemachineCamera  StartCam, BoomCam,MonkeyCam,MonkeyCam_M, RabbitCam, RabbitFocusingCam,introEndCam;
    public StageIntro_T script;
    public GameObject player_Rabbit,BoomPoint,RabbitPoint, startMap, BrokenMap, nils, monkey,EndPoint,InGamePoint;
    private GameObject dialog;
    public Animator RabbitAn,MonkeyAn;
    public CanvasGroup CG;
    public bool EventTrigger, NotLoop;
    private int sequence, priorityIndex;

    public Light spotLight;
    public AudioSource thunderSound;



    void OnEnable()
    {
        dialog = Dialog.Instance.dialogPanel;
        script = GetComponent<StageIntro_T>();
        sequence = 0;
        priorityIndex = 50;
        EventTrigger = false;
        mainAudioSource = SoundManager.Instance.bgmSource;
        mainAudioSource.clip = backgroundMusicJungle;
        mainAudioSource.loop = true; // 반복 재생 설정
        mainAudioSource.Play();
        Dialog.Instance.OnIndexChanged += IndexChanged;
        StartCoroutine(startScene(BoomPoint.transform.position));
    }
    void OnDisable()
    {
        if (Dialog.Instance != null)
        {
            // Dialog.Instance에 등록했던 구독을 해제합니다.
            Dialog.Instance.OnIndexChanged -= IndexChanged;
        }
    }
    void Update()
    {

        if (sequence == 1 && EventTrigger)
        {
            script.intro();
            player_Rabbit.transform.LookAt(monkey.transform);
            dialog.SetActive(true);
            CinemachineBrain brain = FindObjectOfType<CinemachineBrain>();
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            MonkeyCam.Priority = BoomCam.Priority + 1;
            EventTrigger = false;
        }


    }

    IEnumerator startScene(Vector3 targetPosition)
    {

        float moveDuration = 6.0f;
        Vector3 startPosition = player_Rabbit.transform.position;
        float elapsedTime = 0f;


        player_Rabbit.SetActive(false);
        yield return new WaitForSeconds(2.0f);
        player_Rabbit.SetActive(true);
        RabbitAn.SetBool("isRunning", true);

        while (elapsedTime < moveDuration)
        {

            float t = elapsedTime / moveDuration;

            t = Mathf.SmoothStep(0.0f, 1.0f, t);
            player_Rabbit.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            if (targetPosition != player_Rabbit.transform.position)
            {
                player_Rabbit.transform.LookAt(targetPosition);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        RabbitAn.SetBool("isRunning", false);
        
        Rigidbody rb = player_Rabbit.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic) 
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;
        player_Rabbit.transform.position = targetPosition;
        StartCoroutine(BoomScene());
        sequence = 1; 
    }

    IEnumerator introEndScene(Vector3 targetPosition)
    {

        float moveDuration = 10.0f;
        Vector3 startPositionP = player_Rabbit.transform.position;
        Vector3 startPositionM = monkey.transform.position;
        float elapsedTime = 0f;

        monkey.GetComponent<Animator>().enabled = true;
        MonkeyAn.SetBool("isWalking", true);

        while (elapsedTime < moveDuration)
        {
            RabbitAn.SetBool("isRunning", true);
            float t = elapsedTime / moveDuration;

            t = Mathf.SmoothStep(0.0f, 1.0f, t);
            player_Rabbit.transform.position = Vector3.Lerp(startPositionP, targetPosition, t);
            monkey.transform.position = Vector3.Lerp(startPositionM, targetPosition, t);

            if (targetPosition != player_Rabbit.transform.position)
            {
                player_Rabbit.transform.LookAt(targetPosition);
                monkey.transform.LookAt(targetPosition);

            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator BoomScene()
    {
        BoomCam.Priority = StartCam.Priority + 1;
        dialog.SetActive(false);
        StartCoroutine(Fade(1f, 0f, 2f));
        yield return new WaitForSeconds(2f);
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
            CinemachineBrain brain = Object.FindAnyObjectByType<CinemachineBrain>();
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
            if (CG)
            {
                CG.alpha = 1f;
            }
        }
        else if (index == 2)
        {
            CG.alpha = 0;
        }
        else if (index == 3)
        {
            CG.alpha = 1;
            MonkeyCam.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 4)
        {
            monkey.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            player_Rabbit.transform.position = RabbitPoint.transform.position;
            Rigidbody rb = player_Rabbit.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            CG.alpha = 0;
            MonkeyCam_M.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 5)
        {
            monkey.transform.LookAt(player_Rabbit.transform);
            RabbitCam.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 6)
        {
            BoomCam.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 7)
        {
            RabbitCam.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 8)
        {
            RabbitFocusingCam.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 9)
        {
            RabbitCam.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 10)
        {
            RabbitFocusingCam.Priority = priorityIndex;
            priorityIndex++;
        }
        else if (index == 11) 
        {  
            dialog.SetActive(false);
            SaveManager.Instance.RecordAndSaveEventCompletion("0001");
            Vector3 Rposition = new Vector3(player_Rabbit.transform.position.x + 4, player_Rabbit.transform.position.y, player_Rabbit.transform.position.z);
            monkey.transform.position = Rposition;
            monkey.transform.LookAt(player_Rabbit.transform);
            StartCoroutine(introEndScene(EndPoint.transform.position));
            introEndCam.Priority = priorityIndex;
            priorityIndex++;
            StartCoroutine(Fade(0f, 1f, 5f));
            StartCoroutine(MoveJungle());
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
    IEnumerator MoveJungle() 
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Jungle");
    }
}
