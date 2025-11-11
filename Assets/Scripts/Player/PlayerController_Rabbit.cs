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
    [Tooltip("경사면에서 미끄러지는 속도에 영향을 주는 배율")]
    public float slopeGravityFactor = 2.0f;
    [Tooltip("플레이어가 설 수 있는 최대 경사 각도. CharacterController의 Slope Limit에도 자동 적용됩니다.")]
    public float maxSlopeAngle = 45f;

    [Header("Ground Check Settings")]
    [Tooltip("바닥으로 인식할 레이어")]
    public LayerMask groundLayer;
    [Tooltip("바닥 감지 스피어의 반지름 (캐릭터 컨트롤러 반지름보다 약간 작게)")]
    public float groundCheckRadius = 0.4f;
    [Tooltip("캐릭터 발밑에서 얼마나 아래까지를 바닥으로 감지할지")]
    public float groundCheckDistance = 0.2f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;
    public float doubleJumpForce = 4f;
    [Tooltip("1단 점프 직후, 점프 입력을 허용하는 시간 (Coyote Time)")]
    public float coyoteTimeDuration = 0.1f;

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

    // --- 상태 변수 ---
    private float coyoteTimeCounter = 0f;
    private bool isSliding = false;
    private Vector3 slopeNormal;
    private bool isGrounded_Strict = false;
    private bool justDoubleJumped = false; // 2단 점프 직후 Lerp를 막기 위한 플래그


    void Awake()
    {
        controller = GetComponent<CharacterController>(); // CharacterController 컴포넌트 가져오기
        an = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;

        // 점프 높이에 도달하기 위한 점프 속도 계산
        jumpSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // 스크립트의 maxSlopeAngle 값을 CharacterController의 slopeLimit에 동기화
        if (controller != null)
        {
            controller.slopeLimit = maxSlopeAngle;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // PauseManager의 static 변수를 참조하여 일시정지 상태인지 확인합니다.
        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;

        if (!canMove)
        {
            an.SetBool("isRunning", false);
            an.SetBool("isJumping", false);
            an.SetBool("isJumping_Dubble", false);

            return;
        }

        justDoubleJumped = false; // 매 프레임 플래그 초기화

        // --- 1. 바닥 감지 및 점프 리셋 ---
        // (isGroundedByController, SphereCast, isGrounded_Strict, isSliding, slopeNormal, coyoteTimeCounter 계산)
        bool isGroundedByController = controller.isGrounded;
        isGrounded_Strict = false;
        isSliding = false;
        slopeNormal = Vector3.zero;

        if (isGroundedByController)
        {
            Vector3 sphereOrigin = transform.position + (Vector3.up * groundCheckRadius);
            RaycastHit hit;

            if (Physics.SphereCast(
                    sphereOrigin,
                    groundCheckRadius,
                    Vector3.down,
                    out hit,
                    groundCheckDistance,
                    groundLayer
                ))
            {
                float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
                slopeNormal = hit.normal;

                if (surfaceAngle < controller.slopeLimit)
                {
                    isGrounded_Strict = true;
                    isSliding = false;
                }
                else
                {
                    isGrounded_Strict = false;
                    isSliding = true;
                }
            }
            else
            {
                isGrounded_Strict = true;
                isSliding = false;
            }
        }
        else
        {
            isGrounded_Strict = false;
            isSliding = false;
        }

        // 코요테 타임
        if (isGrounded_Strict)
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 지상 착지 처리
        if (isGrounded_Strict)
        {
            an.SetBool("isJumping", false);
            an.SetBool("isJumping_Dubble", false);
            canDoubleJump = true;
            if (playerVelocity.y <= 0f)
            {
                playerVelocity.y = -2f;
            }
        }


        // --- 입력 처리 (점프 로직보다 먼저 계산) ---
        // (xInput, zInput, inputDirection, targetRotation, moveDirection, currentMaxSpeed 계산)
        float xInput = 0f;
        float zInput = 0f;
        Vector3 inputDirection = Vector3.zero;
        Quaternion targetRotation = transform.rotation;
        Vector3 moveDirection = Vector3.zero;
        float currentMaxSpeed = 0f;

        if (canMove)
        {
            xInput = Input.GetAxisRaw("Horizontal");
            zInput = Input.GetAxisRaw("Vertical");
            inputDirection = new Vector3(xInput, 0f, zInput).normalized;

            if (inputDirection.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
                targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                moveDirection = targetRotation * Vector3.forward;
            }
        }

        currentMaxSpeed = isGrounded_Strict ? moveSpeed : airControlSpeed;


        // --- 2. 점프 입력 처리 ---
        // (1단 점프, 2단 점프, justDoubleJumped 플래그 설정)
        if (canMove && Input.GetButtonDown("Jump"))
        {
            if (coyoteTimeCounter > 0f) // 1단 점프 (코요테 타임 포함)
            {
                playerVelocity.y = jumpSpeed;
                an.SetBool("isJumping", true);
                currentHorizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
                coyoteTimeCounter = 0f;
            }
            else if (canDoubleJump && !isGrounded_Strict && !isSliding) // 2단 점프
            {
                playerVelocity.y = jumpSpeed;
                canDoubleJump = false;
                an.SetBool("isJumping_Dubble", true);
                justDoubleJumped = true;

                if (inputDirection.magnitude >= 0.1f)
                {
                    currentHorizontalVelocity = moveDirection * doubleJumpForce;
                }
                else if (currentHorizontalVelocity.magnitude > 0.1f)
                {
                    Vector3 doubleJumpDirection = currentHorizontalVelocity.normalized;
                    currentHorizontalVelocity = doubleJumpDirection * doubleJumpForce;
                }
                else
                {
                    currentHorizontalVelocity = Vector3.zero;
                }
            }
        }


        // --- 3. 수평 이동 계산 ---
        // (MODIFIED) 회전 로직만 밖으로 분리
        if (canMove && inputDirection.magnitude >= 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        if (isGrounded_Strict)
        {
            // [지상일 때]
            Vector3 targetHorizontalVelocity = moveDirection * currentMaxSpeed;
            currentHorizontalVelocity = targetHorizontalVelocity;
        }
        else // [공중 또는 가파른 경사로일 때]
        {
            // 💡 (MODIFIED) 미끄러짐 / 공중 로직 분리
            if (isSliding)
            {
                // [미끄러질 때]
                // 1. (FIXED) 정확한 내리막(downhill) 벡터 계산
                Vector3 slideVector = Vector3.ProjectOnPlane(Vector3.down, slopeNormal);
                Vector3 slideDirection = new Vector3(slideVector.x, 0, slideVector.z).normalized;
                Vector3 slideVelocity = slideDirection * moveSpeed * slopeGravityFactor;

                // 2. 플레이어의 공중 제어 입력 계산
                Vector3 inputVelocity = moveDirection * currentMaxSpeed; // currentMaxSpeed = airControlSpeed

                // 3. 2단 점프 직후가 아니면, (미끄러짐 속도 + 입력)을 목표로 Lerp
                if (!justDoubleJumped)
                {
                    Vector3 targetVelocity = slideVelocity + inputVelocity; // 미끄러짐(주) + 입력(보조)

                    currentHorizontalVelocity = Vector3.Lerp(
                        currentHorizontalVelocity,
                        targetVelocity,
                        Time.deltaTime * turnSpeed * airControlFactor
                    );
                }
            }
            else
            {
                // [순수 공중일 때]
                // 2단 점프 직후가 아닐 때만 공중 제어(Lerp)를 적용
                if (!justDoubleJumped)
                {
                    Vector3 targetHorizontalVelocity = moveDirection * currentMaxSpeed; // currentMaxSpeed = airControlSpeed

                    currentHorizontalVelocity = Vector3.Lerp(
                        currentHorizontalVelocity,
                        targetHorizontalVelocity,
                        Time.deltaTime * turnSpeed * airControlFactor
                    );
                }
            }
            // 2단 점프 직후(justDoubleJumped == true)에는 2번에서 계산된 속도(currentHorizontalVelocity)가 그대로 유지됨
        }

        // --- 4. 중력 적용 ---
        // (isSliding일 때 -2f, 아닐 때 gravity 누적)
        if (!isGrounded_Strict)
        {
            if (isSliding)
            {
                playerVelocity.y = gravity;
            }
            else
            {
                playerVelocity.y += gravity * Time.deltaTime;
            }
        }


        // --- 5. 최종 이동 실행 (Move를 한 번만 호출!) ---
        controller.Move((currentHorizontalVelocity + new Vector3(0, playerVelocity.y, 0)) * Time.deltaTime);


        // --- 6. 애니메이션 처리 ---
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        an.SetBool("isRunning", currentHorizontalSpeed > 0.1f && isGrounded_Strict);
        an.SetBool("isJumping", !isGrounded_Strict && !isSliding);
    }

    /// <summary>
    /// 캐릭터를 지정된 위치로 즉시 이동시킵니다 (텔레포트).
    /// </summary>
    public void TeleportTo(Vector3 destination)
    {
        controller.enabled = false;
        transform.position = destination;
        controller.enabled = true;

        playerVelocity = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;
        coyoteTimeCounter = 0f;
        canDoubleJump = true;
        Debug.Log($"플레이어를 {destination} 위치로 텔레포트했습니다.");
    }

    /// <summary>
    /// 캐릭터를 지정된 위치와 회전값으로 즉시 이동시킵니다 (텔레포트).
    /// </summary>
    public void TeleportTo(Vector3 destination, Quaternion newRotation)
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("PlayerController_Rabbit: CharacterController를 찾을 수 없습니다!");
                return;
            }
        }

        controller.enabled = false;
        transform.position = destination;
        transform.rotation = newRotation;
        controller.enabled = true;

        coyoteTimeCounter = 0f;
        canDoubleJump = true;
        Debug.Log($"플레이어를 {destination} 위치로 텔레포트했습니다.");
    }

    public Vector3 CurrentVelocity
    {
        get { return currentHorizontalVelocity + new Vector3(0, playerVelocity.y, 0); }
    }

    // [추가] 2. 외부에서 속도를 설정하는 메서드
    public void SetVelocity(Vector3 newVelocity)
    {
        // 스왑 시 속도 적용
        currentHorizontalVelocity = new Vector3(newVelocity.x, 0, newVelocity.z);
        playerVelocity.y = newVelocity.y;

        // 속도 적용 후 상태 초기화 (공중 상태 강제 적용)
        coyoteTimeCounter = 0f;
        isGrounded_Strict = false;
        isSliding = false;
        canDoubleJump = true; // 2단 점프는 허용

        // 애니메이션 업데이트를 위해 isJumping 플래그 설정
        if (newVelocity.y > 0.01f || newVelocity.y < -0.01f) // 수직 속도가 있으면 점프 애니메이션
        {
            an.SetBool("isJumping", true);
            an.SetBool("isRunning", false);
        }
    }

    public void StopMovementAndDisableControls()
    {
        canMove = false;

        currentHorizontalVelocity = Vector3.zero;

        playerVelocity = Vector3.zero;

        an.SetBool("isRunning", false);
        an.SetBool("isJumping", false);
        an.SetBool("isJumping_Dubble", false);
    }
}