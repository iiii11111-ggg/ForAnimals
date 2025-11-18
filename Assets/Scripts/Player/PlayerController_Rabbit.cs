using UnityEngine;

// 이 스크립트는 CharacterController와 Animator 컴포넌트를 필요로 합니다.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Rabbit : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float slideSpeed = 4f;
    public float turnSpeed = 12f;
    public float gravity = -9.81f;
    [Tooltip("플레이어가 설 수 있는 최대 경사 각도. CharacterController의 Slope Limit에도 자동 적용됩니다.")]
    public float maxSlopeAngle = 45f;

    [Header("Ground Check Settings")]
    [Tooltip("바닥으로 인식할 레이어")]
    public LayerMask groundLayer;

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
        controller = GetComponent<CharacterController>();
        an = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;

        jumpSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (controller != null)
        {
            controller.slopeLimit = maxSlopeAngle;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;

        if (!canMove)
        {
            an.SetBool("isRunning", false);
            an.SetBool("isJumping", false);
            an.SetBool("isJumping_Dubble", false);

            return;
        }

        justDoubleJumped = false;

        // --- 1. 중력 적용 ---
        if (!isGrounded_Strict)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        // --- 2. 상태 초기화 ---
        isGrounded_Strict = false;

        // --- 3. 코요테 타임 ---
        coyoteTimeCounter -= Time.deltaTime;


        // --- 4. 입력 처리 ---
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

        currentMaxSpeed = (coyoteTimeCounter > 0f) ? moveSpeed : airControlSpeed;

        // 회전 로직 (입력이 있을 때 항상 회전)
        if (canMove && inputDirection.magnitude >= 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        // --- 5. 점프 입력 처리 ---
        if (canMove && Input.GetButtonDown("Jump"))
        {
            if (coyoteTimeCounter > 0f) // 1단 점프
            {
                playerVelocity.y = jumpSpeed;
                an.SetBool("isJumping", true);
                currentHorizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
                coyoteTimeCounter = 0f;
            }
            else if (canDoubleJump && coyoteTimeCounter <= 0f && !isSliding) // 2단 점프
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


        // --- 6. 수평 이동 계산 ---
        if (coyoteTimeCounter > 0f) // [지상일 때]
        {
            Vector3 targetHorizontalVelocity = moveDirection * currentMaxSpeed;
            currentHorizontalVelocity = targetHorizontalVelocity;
        }
        else // [공중 또는 가파른 경사로일 때]
        {
            if (isSliding)
            {
                // [미끄러질 때]
                Vector3 slideVector = Vector3.ProjectOnPlane(Vector3.down, slopeNormal);
                Vector3 slideDirection = new Vector3(slideVector.x, 0, slideVector.z).normalized;
                Vector3 slideVelocity = slideDirection * slideSpeed;
                Vector3 inputVelocity = moveDirection * currentMaxSpeed;

                if (!justDoubleJumped)
                {
                    Vector3 targetVelocity = slideVelocity + inputVelocity;
                    currentHorizontalVelocity = Vector3.Lerp(
                        currentHorizontalVelocity,
                        targetVelocity,
                        Time.deltaTime * turnSpeed
                    );
                }
            }
            else
            {
                // [순수 공중일 때]
                if (!justDoubleJumped)
                {
                    Vector3 targetHorizontalVelocity = moveDirection * currentMaxSpeed;
                    currentHorizontalVelocity = Vector3.Lerp(
                        currentHorizontalVelocity,
                        targetHorizontalVelocity,
                        Time.deltaTime * turnSpeed * airControlFactor
                    );
                }
            }
        }

        // --- 7. 최종 이동 실행 ---
        controller.Move((currentHorizontalVelocity + new Vector3(0, playerVelocity.y, 0)) * Time.deltaTime);


        // --- 8. 애니메이션 처리 ---
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        an.SetBool("isRunning", currentHorizontalSpeed > 0.1f && coyoteTimeCounter > 0f);
        an.SetBool("isJumping", coyoteTimeCounter <= 0f && !isSliding);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 1. GroundLayer가 아니면 무시
        if ((groundLayer.value & (1 << hit.gameObject.layer)) == 0)
        {
            return;
        }

        // 2. 각도 계산
        float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);

        // 3. 바닥/경사면 판단
        if (surfaceAngle < controller.slopeLimit)
        {
            // [정상적인 바닥]
            if (playerVelocity.y < 0.1f)
            {
                isGrounded_Strict = true;
                isSliding = false;
                canDoubleJump = true;
                coyoteTimeCounter = coyoteTimeDuration;

                playerVelocity.y = -2f;

                an.SetBool("isJumping", false);
                an.SetBool("isJumping_Dubble", false);
            }
        }
        else
        {
            // [수정됨] 90도 벽은 '미끄러짐'이 아니라 '공중'으로 처리
            if (surfaceAngle > 89.0f)
            {
                isGrounded_Strict = false;
                isSliding = false;
                coyoteTimeCounter = 0f;
                return;
            }

            // [미끄러운 경사면]
            isGrounded_Strict = false;
            isSliding = true;
            slopeNormal = hit.normal;
            coyoteTimeCounter = 0f;
        }
    }


    // --- (이하 TeleportTo, SetVelocity 등 다른 함수들은 수정 없음) ---

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

    // 🔴 [수정됨] "Full path:" 오타 제거
    public Vector3 CurrentVelocity
    {
        get { return currentHorizontalVelocity + new Vector3(0, playerVelocity.y, 0); }
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        currentHorizontalVelocity = new Vector3(newVelocity.x, 0, newVelocity.z);
        playerVelocity.y = newVelocity.y;

        coyoteTimeCounter = 0f;
        isGrounded_Strict = false;
        isSliding = false;
        canDoubleJump = true;

        if (newVelocity.y > 0.01f || newVelocity.y < -0.01f)
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