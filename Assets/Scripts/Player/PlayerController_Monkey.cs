using UnityEngine;

// 이 스크립트는 CharacterController와 Animator 컴포넌트를 필요로 합니다.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Monkey : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 10f; // 대시 속도 추가
    public float turnSpeed = 12f;
    public float gravity = -9.81f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f; // 점프 높이 추가

    [Header("Control")]
    public bool canMove = true;

    [Header("References")]
    private CharacterController controller;
    private Animator an;
    private Transform mainCameraTransform;

    // ▼▼▼ [수정됨] 멈춤 상황별 감속 시간 변수 ▼▼▼
    [Header("Animation Smoothing")]
    public float walkStopSmoothTime = 0.1f;  // 걷거나 가속할 때의 감속 시간
    public float dashStopSmoothTime = 0.2f;  // ★달리다 멈출 때의 감속 시간
    private float animationSpeed = 0f;       // 애니메이터에 실제로 전달될 보간된 속도 값
    private float animationVelocity = 0f;    // SmoothDamp에서 내부적으로 사용하는 참조 변수
    // ▲▲▲ 여기까지 수정 ▲▲▲

    private Vector3 playerVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        an = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;

        // --- 1. 바닥 감지 및 중력 초기화 ---
        bool isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // 바닥에 붙어있도록 살짝 아래로 힘을 줌
        }

        // --- 2. 입력 처리 ---
        Vector3 inputDirection = Vector3.zero;
        if (canMove)
        {
            float xInput = Input.GetAxisRaw("Horizontal");
            float zInput = Input.GetAxisRaw("Vertical");
            inputDirection = new Vector3(xInput, 0f, zInput).normalized;
        }

        // --- 3. 수평 이동 방향 및 속도 계산 ---
        Vector3 horizontalMove = Vector3.zero;
        if (inputDirection.magnitude >= 0.1f)
        {
            bool isDashing = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float currentSpeed = isDashing ? dashSpeed : moveSpeed;

            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

            Vector3 moveDirection = targetRotation * Vector3.forward;
            horizontalMove = moveDirection * currentSpeed;
        }

        // --- 4. 점프 처리 ---
        if (canMove && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- 5. 최종 이동 계산 및 실행 (Move 한 번만 호출!) ---
        // 중력 적용
        playerVelocity.y += gravity * Time.deltaTime;

        // 수평 이동과 수직 이동(중력, 점프)을 합쳐서 한 번에 Move를 호출
        controller.Move((horizontalMove + playerVelocity) * Time.deltaTime);

        // ▼▼▼ [수정됨] 애니메이션 처리 섹션 ▼▼▼
        // --- 6. 애니메이션 처리 ---

        // 1. 목표 속도 계산 (현재 캐릭터의 실제 수평 속도)
        float targetSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        // 2. 현재 상태에 맞는 감속 시간 결정
        bool isStopping = targetSpeed < 0.1f;               // 멈추려고 하는가?
        bool wasDashing = animationSpeed > moveSpeed + 0.1f; // 현재 애니 속도가 걷기 속도보다 빠른가? (달리던 중이었나?)

        float currentSmoothTime;

        if (isStopping && wasDashing)
        {
            // (1) 달리다가 멈출 때: 더 긴 감속 시간 적용
            currentSmoothTime = dashStopSmoothTime;
        }
        else
        {
            // (2) 걷다 멈추거나, 가속/감속(10->5)할 때: 기본 감속 시간 적용
            currentSmoothTime = walkStopSmoothTime;
        }

        // 3. 실제 속도(targetSpeed)를 향해 애니메이션 속도(animationSpeed)를 부드럽게 변경
        animationSpeed = Mathf.SmoothDamp(
      animationSpeed,
      targetSpeed,
      ref animationVelocity,
            currentSmoothTime // <--- 계산된 감속 시간 사용
        );

        // 4. 최종적으로 "보간된(부드러워진)" 속도 값을 애니메이터에 전달
        an.SetFloat("Speed", animationSpeed);
        an.SetBool("isJumping", !isGrounded);
        // ▲▲▲ 여기까지 수정 ▲▲▲
    }
    public void TeleportTo(Vector3 destination)
    {
        // CharacterController를 잠시 비활성화해야 transform.position을 안전하게 설정할 수 있습니다.
        controller.enabled = false;
        transform.position = destination;
        controller.enabled = true;

        // 텔레포트 후 수직 속도를 초기화하여, 텔레포트하자마자
        // 이전에 쌓인 낙하 속도로 인해 바닥으로 곤두박질치는 것을 방지합니다.
        playerVelocity = Vector3.zero;
        Debug.Log($"플레이어를 {destination} 위치로 텔레포트했습니다.");
    }
}