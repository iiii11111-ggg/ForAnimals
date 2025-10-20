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

        // --- 6. 애니메이션 처리 ---
        // CharacterController의 velocity를 사용하여 실제 이동 속도를 반영
        float horizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        an.SetFloat("Speed", horizontalSpeed);
        an.SetBool("isJumping", !isGrounded);
    }
}