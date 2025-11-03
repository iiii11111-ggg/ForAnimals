using UnityEngine;

// 이 스크립트는 CharacterController와 Animator 컴포넌트를 필요로 합니다.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Rabbit : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;
    public float gravity = -9.81f; // 중력 값
    public float jumpHeight = 1.5f; // 점프 높이 (Inspector에서 조절 가능)

    [Header("Air Control Settings")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.2f;

    [Header("Control")]
    public bool canMove = true; // 외부에서 움직임을 제어할 스위치

    [Header("References")]
    private CharacterController controller; // Rigidbody 대신 CharacterController 사용
    private Animator an;
    private Transform mainCameraTransform;

    private Vector3 playerVelocity; // 중력 적용을 위한 수직 속도
    private float jumpSpeed; // 계산된 점프 속도
    private bool canDoubleJump; // 2단 점프 가능 여부
    private Vector3 currentHorizontalVelocity = Vector3.zero;

    void Awake()
    {
        controller = GetComponent<CharacterController>(); // CharacterController 컴포넌트 가져오기
        an = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;

        // 점프 높이에 도달하기 위한 점프 속도 계산
        jumpSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // PauseManager의 static 변수를 참조하여 일시정지 상태인지 확인합니다.
        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;

        // --- 1. 바닥 감지 및 점프 리셋 ---
        bool isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            
            an.SetBool("isJumping", false);
            an.SetBool("isJumping_Dubble", false);
            canDoubleJump = true;
            if (playerVelocity.y <= 0f)
            {
                playerVelocity.y = -2f; 
            }
        }

        // --- 2. 점프 입력 처리 ---
        // canMove 상태일 때, "Jump" 버튼(기본: 스페이스바)을 눌렀을 때
        if (canMove && Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // 1단 점프 (지상)
                playerVelocity.y = jumpSpeed;
                an.SetBool("isJumping", true);
                // 점프 직후 현재 수평 속도를 보존하여 자연스러운 이행
                currentHorizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
                // canDoubleJump는 이미 true 상태입니다.
            }
            else if (canDoubleJump)
            {
                // 2단 점프 (공중)
                playerVelocity.y = jumpSpeed; // 1단 점프와 동일한 높이로 설정
                canDoubleJump = false; // 2단 점프 기회 소진
                an.SetBool("isJumping_Dubble", true); 
                // 공중 점프 시에도 현재 수평 속도 유지
                currentHorizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
            }
        }

        // --- 3. 입력 처리 및 수평 이동 계산 ---
        Vector3 desiredHorizontalVelocity = Vector3.zero;
        if (canMove)
        {
            float xInput = Input.GetAxisRaw("Horizontal");
            float zInput = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(xInput, 0f, zInput).normalized;

            if (inputDirection.magnitude >= 0.1f)
            {
                // 카메라 기준 이동 방향 계산
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

                // 부드러운 회전
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                // 최종 수평 이동 방향
                Vector3 moveDirection = targetRotation * Vector3.forward;
                desiredHorizontalVelocity = moveDirection * moveSpeed;
            }
        }

        // 지상/공중에 따른 수평 속도 제어 (공중에서는 감쇠 적용)
        if (isGrounded)
        {
            currentHorizontalVelocity = desiredHorizontalVelocity;
        }
        else
        {
            currentHorizontalVelocity = Vector3.Lerp(
                currentHorizontalVelocity,
                desiredHorizontalVelocity,
                Time.deltaTime * turnSpeed * airControlFactor
            );
        }

        // --- 4. 중력 적용 ---
        if (!isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        // --- 5. 최종 이동 실행 (Move를 한 번만 호출!) ---
        // 수평 이동(currentHorizontalVelocity)과 수직 이동(playerVelocity)을 합쳐서 한 번에 적용합니다.
        controller.Move((currentHorizontalVelocity + new Vector3(0, playerVelocity.y, 0)) * Time.deltaTime);

        // --- 6. 애니메이션 처리 ---
        // CharacterController의 실제 속도를 기반으로 애니메이션을 제어합니다.
        // (수정) 점프 중에는 달리기 애니메이션이 실행되지 않도록 isGrounded 조건 추가
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        an.SetBool("isRunning", currentHorizontalSpeed > 0.1f && isGrounded);
    }

    /// <summary>
    /// 캐릭터를 지정된 위치로 즉시 이동시킵니다 (텔레포트).
    /// </summary>
    /// <param name="destination">이동할 목표 위치</param>
    public void TeleportTo(Vector3 destination)
    {
        // CharacterController를 잠시 비활성화해야 transform.position을 안전하게 설정할 수 있습니다.
        controller.enabled = false;
        transform.position = destination;
        controller.enabled = true;

        // 텔레포트 후 수직 속도를 초기화하여, 텔레포트하자마자
        // 이전에 쌓인 낙하 속도로 인해 바닥으로 곤두박질치는 것을 방지합니다.
        playerVelocity = Vector3.zero;
        // 텔레포트 후에는 공중에 있을 수 있으므로, 2단 점프가 아닌 1단 점프만 가능하도록 설정
        canDoubleJump = true;
        Debug.Log($"플레이어를 {destination} 위치로 텔레포트했습니다.");
    }
    public void TeleportTo(Vector3 destination, Quaternion newRotation)
    {
        // controller가 null인 경우 다시 가져오기 (SetActive 직후 호출 시 초기화가 안 되었을 수 있음)
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("PlayerController_Rabbit: CharacterController를 찾을 수 없습니다!");
                return;
            }
        }

        // CharacterController를 잠시 비활성화해야 transform.position을 안전하게 설정할 수 있습니다.
        controller.enabled = false;
        transform.position = destination;
        transform.rotation = newRotation;
        controller.enabled = true;


        // 텔레포트 후 수직 속도를 초기화하여, 텔레포트하자마자
        // 이전에 쌓인 낙하 속도로 인해 바닥으로 곤두박질치는 것을 방지합니다.
        playerVelocity = Vector3.zero;
        // 텔레포트 후에는 공중에 있을 수 있으므로, 2단 점프가 아닌 1단 점프만 가능하도록 설정
        canDoubleJump = true;
        Debug.Log($"플레이어를 {destination} 위치로 텔레포트했습니다.");
    }
}