using UnityEngine;

public class PlayerController_Monkey : MonoBehaviour
{
    public Rigidbody rb;

    public float moveSpeed = 8f;
    public float dashSpeed = 24f;
    public float turnSpeed = 15f;

    public Animator an;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        an = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    public void Movement()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        // 입력 방향 벡터 (Y축은 0으로 고정)
        Vector3 moveDirection = new Vector3(xInput, 0f, zInput).normalized;

        // 1. 대시 상태 확인
        bool isMoving = moveDirection.sqrMagnitude > 0.01f;
        bool isDashing = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // 2. 현재 적용할 이동 속도와 애니메이션 속도 결정
        float currentTargetSpeed; // Rigidbody에 적용할 속도
        float animSpeed;          // Animator에 전달할 Float 값

        if (isMoving)
        {
            // 움직일 때
            currentTargetSpeed = isDashing ? dashSpeed : moveSpeed;

            // Animator에는 Rigidbody에 적용할 속도 값을 그대로 전달 (Idle, Walk, Run 블렌딩 기준)
            animSpeed = currentTargetSpeed;
        }
        else
        {
            // 정지 상태 (Idle)
            currentTargetSpeed = 0f;
            animSpeed = 0f;
        }

        // 3. Animator의 "Speed" 파라미터 업데이트 (딜레이 없이 모션 블렌딩)
        // an.SetFloat("Speed", 0.1f); 대신 an.SetFloat("Speed", animSpeed); 를 사용해야 합니다.
        an.SetFloat("Speed", animSpeed);


        // 4. 캐릭터 회전 로직
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // 부드러운 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        // 5. Rigidbody를 이용한 실제 이동 구현
        float xSpeed = xInput * currentTargetSpeed;
        float zSpeed = zInput * currentTargetSpeed;

        // Rigidbody 선형 속도 설정 (Y축은 중력을 위해 현재 값 유지)
        rb.linearVelocity = new Vector3(xSpeed, rb.linearVelocity.y, zSpeed);
    }

}
