using System.Xml;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using UnityEngine.TextCore.Text;

public class Dialogue0004 : MonoBehaviour, IEventController
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
    public GameObject Ship;
    public GameObject ShipPlayer;
    public GameObject Player_Rabbit;
    public GameObject Player_Monkey;
    public CanvasGroup FadeScreen;
    public Transform RPoint_S1, MPoint_S1;
    public Transform RPoint_E1, MPoint_E1, MPoint_E2;
    public Transform ShipPoint_E;

    public CanvasGroup EndingUi;
    public GameObject EndingButton;

    private Animator RA, MA;
    public GameObject InGameUI;

    public CinemachineCamera Cam1,Cam2,Cam3,Cam4,Cam5,Cam6,Cam7,Cam8,Cam9,Cam10,Cam11,Cam12,Cam13,Cam14,Cam15;

    public GameObject pastImg1,pastImg2,pastVideo;

    public AudioClip ShipDeparture;

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
        Text_0004 Script = GetComponent<Text_0004>();
        Script.StartDialog_0004();
        Dialog.Instance.OnIndexChanged += IndexChanged;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMEnding();
        }

        // 첫 대화 씬 

        Rabbit.SetActive(true);
        Monkey.SetActive(true);

        Player_Rabbit.SetActive(false);
        Player_Monkey.SetActive(false);

        InGameUI.SetActive(false);


        brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
        Cam1.Priority = 11;

        MoveCharacterToTarget(Rabbit, RPoint_S1.position, RPoint_E1.position, 6f);
        MoveCharacterToTarget(Monkey, MPoint_S1.position, MPoint_E1.position, 6f);

        StartCoroutine(DialogOpen(5f));


    }
    public void ScriptEnd()
    {
        dp = Dialog.Instance.dialogPanel;
        dp.SetActive(false);
        Dialog.Instance.OnIndexChanged -= IndexChanged;

        SoundManager.Instance.PlaySFX(ShipDeparture);

        Cam14.Priority = 9;
        Cam15.Priority = 11;
        ShipPlayer.SetActive(true);
        MoveShipToTarget(Ship, Ship.transform.position, ShipPoint_E.position, 200f);
        StartCoroutine(FadeInEnding(2f, 2f));
    }

    void IndexChanged(int index)
    {
        if (index == 1)
        {
            StopAllCoroutines();
            TeleportCharacterAndCleanup(Rabbit, RPoint_E1.position);
            TeleportCharacterAndCleanup(Monkey, MPoint_E1.position);
            Cam1.Priority = 9;
            Cam2.Priority = 11;
        }
        else if (index == 2)
        {
            Cam2.Priority = 9;
            Cam3.Priority = 11;
        }
        else if (index == 3)
        {
            Rabbit.transform.LookAt(Monkey.transform);
            Monkey.transform.LookAt(Rabbit.transform);
            Cam3.Priority = 9;
            Cam4.Priority = 11;
        }
        else if (index == 4) 
        {

            Cam4.Priority = 9;
            Cam5.Priority = 11;
        }
        else if (index == 5)
        {
            Cam4.Priority = 11;
            Cam5.Priority = 9;
            StartCoroutine(ZoomRoutine(Cam4.Lens.FieldOfView, Cam4.Lens.FieldOfView - 10, 1f, Cam4));
        }
        else if (index == 6)
        {
            pastVideo.SetActive(true);
        }
        else if (index == 7)
        {
            pastVideo.SetActive(false);
            pastImg1.SetActive(true);
        }
        else if (index == 8)
        {
            pastImg1.SetActive(false);
        }
        else if (index == 9) // 왜 우리집을 부순거야?
        {
            Cam5.Priority = 9;
            Cam6.Priority = 11;
        }
        else if (index == 10) 
        {
            Cam6.Priority = 9;
            Cam7.Priority = 11;
        }
        else if (index == 11)
        {
            Cam7.Priority = 9;
            Cam6.Priority = 11;
        }
        else if (index == 12) // 생각이 짧았어.. 근데 넌 왜 내가 인간인걸 알면서 같이 가자고 한거야?
        {
            Cam6.Priority = 9;
            Cam4.Priority = 11;
        }
        else if (index == 13) 
        {
            Cam4.Priority = 9;
            Cam5.Priority = 11;
        }
        else if (index == 14)
        {
            Cam5.Priority = 9;
            Cam7.Priority = 11;
        }
        else if (index == 15)
        {
            pastImg2.SetActive(true);
        }
        else if (index == 16) // 난... 원숭이로 변하고나서 알았어
        {
            pastImg2.SetActive(false);
            Cam7.Priority = 9;
            Cam8.Priority = 11;
        }
        else if (index == 17) 
        {
            Cam8.Priority = 9;
            Cam9.Priority = 11;
            MoveCharacterToTarget(Monkey, MPoint_E1.position, MPoint_E2.position, 2.5f);
        }
        else if (index == 18)
        {
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            Cam9.Priority = 9;
            Cam10.Priority = 11;
        }
        else if (index == 19)
        {
            StopAllCoroutines();
            TeleportCharacterAndCleanup(Monkey, MPoint_E2.position);
            brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.Cut;
            Cam10.Priority = 9;
            Cam11.Priority = 11;
        }
        else if (index == 20)
        {
            Monkey.transform.LookAt(Rabbit.transform);
            Cam11.Priority = 9;
            Cam12.Priority = 11;
        }
        else if (index == 21)
        {
            MoveCharacterToTarget(Monkey, MPoint_E2.position, MPoint_E1.position, 2f);
            Cam12.Priority = 9;
            Cam13.Priority = 11;
        }
        else if (index == 22)
        {
            Cam13.Priority = 9;
            Cam6.Priority = 11;
        }
        else if (index == 23)
        {
            Cam6.Priority = 9;
            Cam14.Priority = 11;
        }




    }
    IEnumerator DialogOpen(float t)
    {
        yield return new WaitForSeconds(t);
        dp.SetActive(true);

        Transform parentTransform = dp.transform;
        GameObject PrevBtn = parentTransform.Find("Dialog_Previous").gameObject;

        PrevBtn.SetActive(false);
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

    IEnumerator FadeInEnding(float fadeDuration, float holdDuration)
    {

        yield return new WaitForSeconds(holdDuration);

        yield return StartCoroutine(FadeEnding(0f, 1f, fadeDuration));

        yield return new WaitForSeconds(fadeDuration);

        EndingButton.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator FadeEnding(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;

        EndingUi.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            EndingUi.alpha = newAlpha;

            yield return null;
        }

        EndingUi.alpha = endAlpha;
    }


    private IEnumerator ZoomRoutine(float startFOV, float endFOV, float duration,CinemachineCamera targetCam)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            float smoothedProgress = Mathf.SmoothStep(0f, 1f, progress);

            // 렌즈 구조체의 FieldOfView 값을 직접 변경합니다.
            targetCam.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, smoothedProgress);

            yield return null; // 다음 프레임까지 대기
        }

        // 최종적으로 목표 FOV로 설정하여 오차를 제거합니다.
        targetCam.Lens.FieldOfView = endFOV;
    }

    public void MoveCharacterToTarget(GameObject character, Vector3 startPos, Vector3 targetPos, float duration)
    {
        StartCoroutine(MoveRoutine(character, startPos, targetPos, duration));
    }

    private IEnumerator MoveRoutine(GameObject character, Vector3 startPos, Vector3 targetPos, float duration)
    {
        // 컴포넌트 가져오기
        Animator animator = character.GetComponent<Animator>();
        Rigidbody rb = character.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody가 없습니다.");
            yield break;
        }

        // **1. 초기 설정 및 Rigidbody 설정**

        // Rigidbody가 움직이려면 Kinematic이 꺼져 있어야 함
        // 원본 isKinematic 상태를 저장해두고, 끝날 때 복구합니다.
        bool originalKinematic = rb.isKinematic;
        rb.isKinematic = false;

        // 애니메이션 설정 (토끼와 몽키는 서로 다른 파라미터를 씀)
        if (character == Rabbit)
        {
            // Null 예외 방지: Animator가 없으면 에러가 날 수 있음.
            if (animator != null) animator.SetBool("isRunning", true);
        }
        else // Monkey
        {
            if (animator != null) animator.SetFloat("Speed", 5f);
        }

        // 시작 위치로 순간 이동 (이동 시작 지점 보장)
        character.transform.position = startPos;

        // 2. 속도 및 방향 계산
        Vector3 startXZ = new Vector3(startPos.x, 0, startPos.z);
        Vector3 targetXZ = new Vector3(targetPos.x, 0, targetPos.z);

        float distance = Vector3.Distance(startXZ, targetXZ);
        float speed = distance / duration; // 필요한 속력

        Vector3 direction = (targetXZ - startXZ).normalized; // 방향


        // 3. 물리 업데이트 주기에 맞춰 이동 (떨림 방지 핵심)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (direction != Vector3.zero)
            {
                character.transform.rotation = Quaternion.LookRotation(direction);
            }

            // 현재 물리 엔진의 Y축 속도(중력)는 가져오고, X/Z 속도만 덮어씌움
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 moveVelocity = direction * speed;

            // Y축은 건드리지 않아서 중력 유지
            rb.linearVelocity = new Vector3(moveVelocity.x, currentVelocity.y, moveVelocity.z);

            // ★ 물리 업데이트 시간만큼 경과 시간 증가
            elapsedTime += Time.fixedDeltaTime;

            // ★ 다음 물리 업데이트 시점까지 대기 (떨림 방지)
            yield return new WaitForFixedUpdate();
        }

        // 4. 도착 후 정지 및 복구

        // X, Z 속도는 0으로 만들고, Y축 속도만 유지 (낙하 중일 수 있으므로)
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        // Rigidbody를 원래 상태로 복구
        rb.isKinematic = originalKinematic;

        // 애니메이션 정지
        if (character == Rabbit)
        {
            if (animator != null) animator.SetBool("isRunning", false);
        }
        else // Monkey
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
        }

        // 마지막 위치 보정 (혹시 모를 오차 방지)
        // Y축은 건드리지 않음
        character.transform.position = new Vector3(targetPos.x, character.transform.position.y, targetPos.z);
    }

    public void MoveShipToTarget(GameObject character, Vector3 startPos, Vector3 targetPos, float duration)
    {
        StartCoroutine(MoveRoutine_Ship(character, startPos, targetPos, duration));
    }

    private IEnumerator MoveRoutine_Ship(GameObject character, Vector3 startPos, Vector3 targetPos, float duration)
    {
        // 컴포넌트 가져오기
        Animator animator = character.GetComponent<Animator>();
        Rigidbody rb = character.GetComponent<Rigidbody>();

        // 1. 초기 설정 및 Rigidbody 설정 (Kinematic)
        if (rb == null)
        {
            Debug.LogError("Rigidbody가 없습니다.");
            yield break;
        }

        bool originalKinematic = rb.isKinematic;
        rb.isKinematic = true; // 

        // 시작 위치로 순간 이동
        character.transform.position = startPos;

        float startTime = Time.time;
        float endTime = startTime + duration;

        // 방향 미리 계산 (회전용)
        Vector3 direction = (targetPos - startPos).normalized;

        if (direction != Vector3.zero)
        {
            character.transform.rotation = Quaternion.LookRotation(direction);
        }

        while (Time.time < endTime)
        {
            // 경과 시간 및 비율 계산
            float elapsedTime = Time.time - startTime;
            float t = elapsedTime / duration; // 0.0에서 1.0 사이의 값

            // Lerp를 사용하여 이동: startPos에서 targetPos까지 t 비율만큼 보간
            character.transform.position = Vector3.Lerp(startPos, targetPos, t);

            // ★ 매 프레임 업데이트 시점까지 대기 (일반 업데이트)
            yield return null;
        }

        // 3. 도착 후 정지 및 복구

        // 최종 위치 보정 (오차 방지)
        character.transform.position = targetPos;

        // Rigidbody를 원래 상태로 복구
        rb.isKinematic = originalKinematic;
    }


    public void TeleportCharacterAndCleanup(GameObject character, Vector3 targetPos)
    {

        Rigidbody rb = character.GetComponent<Rigidbody>();
        Animator animator = character.GetComponent<Animator>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        character.transform.position = targetPos;

        if (animator != null)
        {
            if (character.name.Contains("Rabbit"))
            {
                animator.SetBool("isRunning", false);
            }
            else // Monkey로 가정
            {
                animator.SetFloat("Speed", 0f);
            }
        }
    }

}
