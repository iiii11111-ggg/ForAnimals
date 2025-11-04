using UnityEngine;

// 이 스크립트는 CharacterController와 Animator 컴포넌트를 필요로 합니다.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Rabbit : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float doubleJumpForce = 4f;

    [Header("Ground Check Settings")]
    [Tooltip("바닥으로 인식할 레이어")]
    public LayerMask groundLayer;
    [Tooltip("바닥 감지 스피어의 반지름 (캐릭터 컨트롤러 반지름보다 약간 작게)")]
    public float groundCheckRadius = 0.4f;
    [Tooltip("캐릭터 발밑에서 얼마나 아래까지를 바닥으로 감지할지")]
    public float groundCheckDistance = 0.2f;

    [Header("Air Control Settings")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.2f;
    public float airControlSpeed = 1.5f;

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

        // --- 1. 바닥 감지 및 점프 리셋 (💡 여기가 수정된 부분입니다!) ---

        Vector3 sphereOrigin = transform.position + (Vector3.up * groundCheckRadius);

        bool isGrounded_SphereCast = false; // 기본값은 false (공중)
        RaycastHit hit; // 충돌 정보 저장 변수

        // 1. SphereCast로 groundLayer만 감지
        if (Physics.SphereCast(
                sphereOrigin,
                groundCheckRadius,
                Vector3.down,
                out hit,
                groundCheckDistance,
                groundLayer // "Ledge"가 제외된 groundLayer 마스크 사용
            ))
        {
            // 2. groundLayer에 닿았다면, 닿은 표면의 각도를 검사
            float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);

            // 3. 닿은 각도가 설 수 있는 경사(Slope Limit)보다 완만할 때만
            if (surfaceAngle < controller.slopeLimit)
            {
                isGrounded_SphereCast = true; // '진짜 바닥'으로 인정
            }
            // (만약 각도가 더 가파르면? isGrounded_SphereCast는 false로 유지됨 -> 미끄러짐)
        }
        // (만약 Ledge에 닿았다면? groundLayer에 없으므로 이 if문 자체가 실행 안 됨 -> 미끄러짐)

        // 4. 최종 바닥 판정은 오직 SphereCast 결과로만 결정!
        bool isGrounded = isGrounded_SphereCast;

        // (기존 코드: bool isGrounded = isGrounded_Controller || isGrounded_SphereCast; 는 삭제됨)

        // --- (수정 끝) ---


        if (isGrounded)
        {
            an.SetBool("isJumping", false);
            an.SetBool("isJumping_Dubble", false);
            canDoubleJump = true;
            if (playerVelocity.y <= 0f)
            {
                playerVelocity.y = -2f; // 바닥에 붙어있도록 살짝 아래로 힘을 줌
            }
        }

        // 💡 [A. 수정] --- 입력 처리 (2단 점프 로직보다 먼저 계산) ---
        float xInput = 0f;
        float zInput = 0f;
        Vector3 inputDirection = Vector3.zero;
        Quaternion targetRotation = transform.rotation; // 현재 회전 값으로 초기화
        Vector3 moveDirection = Vector3.zero;           // 0으로 초기화

        if (canMove)
        {
            xInput = Input.GetAxisRaw("Horizontal");
            zInput = Input.GetAxisRaw("Vertical");
            inputDirection = new Vector3(xInput, 0f, zInput).normalized;

            if (inputDirection.magnitude >= 0.1f)
            {
                // 카메라 기준 이동 방향 계산
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
                targetRotation = Quaternion.Euler(0f, targetAngle, 0f); // targetRotation 계산
                moveDirection = targetRotation * Vector3.forward;     // moveDirection 계산
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
            }
            // 💡 [B. 수정] --- 2단 점프 로직 ---
            else if (canDoubleJump)
            {
                // 2단 점프 (공중)
                playerVelocity.y = jumpSpeed; // 수직 점프력은 동일하게 적용
                canDoubleJump = false; // 2단 점프 기회 소진
                an.SetBool("isJumping_Dubble", true);

                if (inputDirection.magnitude >= 0.1f)
                {
                    // 1. 키 입력이 있으면: 위에서 계산한 카메라 기준 방향 (moveDirection)으로 힘 적용
                    currentHorizontalVelocity = moveDirection * doubleJumpForce;
                }
                else if (currentHorizontalVelocity.magnitude > 0.1f)
                {
                    // 2. 키 입력이 없고, 원래 속도가 있으면: 원래 진행 방향으로 힘 적용
                    Vector3 doubleJumpDirection = currentHorizontalVelocity.normalized;
                    currentHorizontalVelocity = doubleJumpDirection * doubleJumpForce;
                }
                else
                {
                    // 3. 둘 다 없으면 (제자리 2단 점프): 수평 속도를 0으로 설정합니다.
                    currentHorizontalVelocity = Vector3.zero;
                }
            }
        }

        // --- 3. 입력 처리 및 수평 이동 계산 ---
        Vector3 desiredHorizontalVelocity = Vector3.zero;
        if (canMove)
        {
            // 입력값 계산 로직은 (A)로 이동했음
            if (inputDirection.magnitude >= 0.1f)
            {
                // 부드러운 회전 (회전은 매 프레임 적용)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                // 최종 수평 이동 방향 (목표 속도 계산)
                float currentMaxSpeed = isGrounded ? moveSpeed : airControlSpeed;
                desiredHorizontalVelocity = moveDirection * currentMaxSpeed;
            }
        }

        // 지상/공중에 따른 수평 속도 제어 (공중에서는 감쇠 적용)
        if (isGrounded)
        {
            // [지상일 때]
            // 플레이어의 입력값을 그대로 수평 속도로 사용합니다.
            currentHorizontalVelocity = desiredHorizontalVelocity;
        }
        else // [공중 또는 가파른 경사로일 때]
        {
            // CharacterController가 직전 프레임에 *실제로* 움직인
            // 수평 속도(슬라이드 속도 포함)를 가져옵니다.
            Vector3 lastFrameHorizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

            // '실제 속도'를 기반으로 '원하는 입력 방향'으로 부드럽게 변경합니다 (공중 제어).
            currentHorizontalVelocity = Vector3.Lerp(
                lastFrameHorizontalVelocity,    // 👈 (중요) currentHorizontalVelocity 대신 이걸 사용
                desiredHorizontalVelocity,
                Time.deltaTime * turnSpeed * airControlFactor
            );
        }

        // --- 4. 중력 적용 ---
        if (!isGrounded) // 👈 '진짜 바닥'이 아닐 때만 중력 적용
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