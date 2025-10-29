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

    [Header("Control")]
    public bool canMove = true; // 외부에서 움직임을 제어할 스위치

    [Header("References")]
    private CharacterController controller; // Rigidbody 대신 CharacterController 사용
    private Animator an;
    private Transform mainCameraTransform;

    private Vector3 playerVelocity; // 중력 적용을 위한 수직 속도
    private float jumpSpeed; // 계산된 점프 속도
    private bool canDoubleJump; // 2단 점프 가능 여부

    void Start()
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
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // 바닥에 붙어있도록 살짝 아래로 힘을 줌
            canDoubleJump = true; // 바닥에 있으므로 2단 점프 가능
            an.SetBool("isJumping", false); // 바닥이므로 점프 애니메이션 종료
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
                // canDoubleJump는 이미 true 상태입니다.
            }
            else if (canDoubleJump)
            {
                // 2단 점프 (공중)
                playerVelocity.y = jumpSpeed; // 1단 점프와 동일한 높이로 설정
                canDoubleJump = false; // 2단 점프 기회 소진
                an.SetBool("isJumping", true); // 다시 점프 애니메이션 실행 (필요시 2단 점프용 트리거 사용)
            }
        }

        // --- 3. 입력 처리 및 수평 이동 계산 ---
        Vector3 horizontalMove = Vector3.zero;
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
                horizontalMove = moveDirection * moveSpeed;
            }
        }

        // --- 4. 중력 적용 ---
        playerVelocity.y += gravity * Time.deltaTime;

        // --- 5. 최종 이동 실행 (Move를 한 번만 호출!) ---
        // 수평 이동(horizontalMove)과 수직 이동(playerVelocity)을 합쳐서 한 번에 적용합니다.
        controller.Move((horizontalMove + new Vector3(0, playerVelocity.y, 0)) * Time.deltaTime);

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
        canDoubleJump = false;
        Debug.Log($"플레이어를 {destination} 위치로 텔레포트했습니다.");
    }
}