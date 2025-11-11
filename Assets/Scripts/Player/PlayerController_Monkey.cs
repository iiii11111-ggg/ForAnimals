using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Monkey : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float turnSpeed = 12f;
    public float gravity = -9.81f;

    [Header("Air Control Settings")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.2f;
    public float airControlSpeed = 1.5f;

    [Header("Ground Check Settings")]
    [Tooltip("바닥으로 인식할 레이어")]
    public LayerMask groundLayer;
    [Tooltip("바닥 감지 스피어의 반지름 (캐릭터 컨트롤러 반지름보다 약간 작게)")]
    public float groundCheckRadius = 0.4f;
    [Tooltip("캐릭터 발밑에서 얼마나 아래까지를 바닥으로 감지할지")]
    public float groundCheckDistance = 0.2f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;
    // 💡 [추가] 코요테 타임 변수
    [Tooltip("땅에서 떨어진 후 점프가 가능한 시간 (코요테 타임)")]
    public float coyoteTime = 0.1f;

    [Header("Hanging Settings")]
    public float hangDetectionRadius = 1.0f;
    public float hangDetectionOffset = 0.5f;
    public string hangPointLedgeTag = "Ledge";
    public string hangPointChildName = "HangPoint";
    // [핵심] 스크립트의 오프셋은 0으로 고정합니다.
    // 높이 조절은 애니메이션 클립에서 직접 합니다.
    public Vector3 handToRootOffset = Vector3.zero;
    public float hangMoveDuration = 0.3f;

    [Header("Swinging Physics")]
    public float swingGravity = 9.8f;
    public float swingPumpForce = 5f;
    public float swingDampening = 0.1f;
    public float maxSwingAngle = 1.8f;
    [Tooltip("앞으로 swing할 때 플레이어 이동 배율")]
    public float forwardSwingMovementScale = 0.5f;
    [Tooltip("뒤로 swing할 때 플레이어 이동 배율")]
    public float backwardSwingMovementScale = 0.5f;

    [Header("Swinging Leap Settings")]
    public float leapVelocityFactor = 5f;
    public float baseLeapUpwardForce = 4f;
    public float addedLeapUpwardForce = 6f;

    [Header("Control")]
    public bool canMove = true;

    // 💡 2. 카메라 FOV 조절을 위한 변수 추가
    [Header("Camera Settings")]
    [Tooltip("FOV를 조절할 시네머신 프리룩 카메라")]
    public CinemachineCamera freeLookCamera;
    [Tooltip("대시할 때 변경될 FOV 값")]
    public float dashFOV = 50f;
    [Tooltip("FOV가 변경되는 속도")]
    public float fovSmoothSpeed = 5f;


    [Header("References")]
    private CharacterController controller;
    private Animator an;
    private Transform mainCameraTransform;

    [Header("Animation Smoothing")]
    public float walkStopSmoothTime = 0.1f;
    public float dashStopSmoothTime = 0.2f;
    private float animationSpeed = 0f;
    private float animationVelocity = 0f;

    // --- 상태 변수 ---
    private Vector3 playerVelocity;
    private Vector3 currentHorizontalVelocity = Vector3.zero;

    // 💡 [수정] isGrounded를 멤버 변수로 변경
    private bool isGrounded = false;
    // 💡 [추가] 코요테 타임을 위한 마지막 지면 감지 시간
    private float timeLastGrounded = 0f;

    private bool isHanging = false;
    private bool isMovingToHangPoint = false;
    private Transform currentHangTransform;
    private bool isLeapingFromHang = false;

    private float currentSwingPosition = 0f;
    private float currentSwingVelocity = 0f;

    private float lastInputMagnitude = 0f;

    // 💡 2. 대시 상태 및 원본 FOV 저장을 위한 변수 추가
    private bool isDashing = false;
    private float originalFOV;


    void Start()
    {
        
        controller = GetComponent<CharacterController>();
        an = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;

        // 💡 3. 시작 시 원본 FOV 값 저장
        if (freeLookCamera != null)
        {
            originalFOV = freeLookCamera.Lens.FieldOfView;
        }
        else
        {
            Debug.LogWarning("PlayerController_Monkey: FreeLook Camera가 할당되지 않았습니다. FOV 조절이 작동하지 않습니다.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;

        if (isHanging)
        {
            HandleHanging();
            return;
        }

        if (isMovingToHangPoint)
        {
            return;
        }

        if (canMove && Input.GetKeyDown(KeyCode.E))
        {
            AttemptGrab();

            if (isMovingToHangPoint)
            {
                return;
            }
        }

        bool isGrounded_Controller = controller.isGrounded;

        Vector3 sphereOrigin = transform.position + (Vector3.up * groundCheckRadius);
        bool isGrounded_SphereCast = Physics.SphereCast(
            sphereOrigin,          // 스피어 시작 위치 (캐릭터 발 약간 위)
            groundCheckRadius,     // 스피어 반지름
            Vector3.down,          // 쏘는 방향 (아래)
            out RaycastHit hit,
            groundCheckDistance,   // 체크할 거리 (시작위치로부터 0.2m 아래까지)
            groundLayer            // 감지할 레이어 (인스펙터에서 설정)
        );

        // 💡 [수정] 로컬 변수 -> 멤버 변수 'isGrounded' 사용
        isGrounded = isGrounded_Controller || isGrounded_SphereCast;

        if (isGrounded)
        {
            // 💡 [추가] 코요테 타임을 위해 마지막 지면 감지 시간 갱신
            timeLastGrounded = Time.time;

            if (playerVelocity.y <= 0f)
            {
                playerVelocity.y = -2f;

                if (isLeapingFromHang)
                {
                    isLeapingFromHang = false;
                    an.SetBool("isLeapingFromHang", false);
                }
            }
        }

        Vector3 inputDirection = Vector3.zero;
        if (canMove && !isLeapingFromHang)
        {
            float xInput = Input.GetAxisRaw("Horizontal");
            float zInput = Input.GetAxisRaw("Vertical");
            inputDirection = new Vector3(xInput, 0f, zInput).normalized;
            lastInputMagnitude = inputDirection.magnitude;
        }

        Vector3 moveDirection = Vector3.zero;
        float currentMaxSpeed = 0f;

        // 💡 4. isDashing 로컬 변수를 멤버 변수로 사용하도록 수정
        if (inputDirection.magnitude >= 0.1f && !isLeapingFromHang)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            moveDirection = targetRotation * Vector3.forward;

            if (isGrounded)
            {
                isDashing = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                currentMaxSpeed = isDashing ? dashSpeed : moveSpeed;
            }
            else
            {
                isDashing = false; // 공중에서는 대시 상태 해제
                currentMaxSpeed = airControlSpeed;
            }
        }
        else
        {
            isDashing = false; // 입력이 없으면 대시 상태 해제
        }

        if (isGrounded)
        {
            if (!isLeapingFromHang)
            {
                currentHorizontalVelocity = moveDirection * currentMaxSpeed;
            }
        }
        else
        {
            if (!isLeapingFromHang)
            {
                Vector3 desiredVelocity = moveDirection * currentMaxSpeed;
                currentHorizontalVelocity = Vector3.Lerp(
                    currentHorizontalVelocity,
                    desiredVelocity,
                    Time.deltaTime * turnSpeed * airControlFactor
                );
            }
        }

        // 💡 [수정] 점프 조건 및 로직 변경 (코요테 타임 적용, 제자리 점프 공중 이동)
        if (canMove && Input.GetKeyDown(KeyCode.Space) && (Time.time - timeLastGrounded <= coyoteTime) && !isLeapingFromHang)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            // [제거] 👈 제자리 점프 시 공중 이동을 위해 이 줄을 제거
            // currentHorizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z); 

            // [추가] 👈 코요테 타임을 사용했으므로 즉시 만료시킴 (공중 이단 점프 방지)
            timeLastGrounded = 0f; 
        }

        if (!isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
        
        controller.Move((currentHorizontalVelocity + playerVelocity) * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;


        if (isHanging || isMovingToHangPoint)
        {
            an.SetFloat("SwingPower", currentSwingPosition);
        }
        else if (isLeapingFromHang)
        {
            // (도약 애니메이션 재생 중)
        }
        else
        {
            // (일반 이동)
            float targetSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
    
            // 💡 [수정] controller.isGrounded -> isGrounded (멤버 변수 사용)
            if (lastInputMagnitude < 0.1f && isGrounded)
            {
                targetSpeed = 0f;
            }
            bool isStopping = targetSpeed < 0.1f;
            bool wasDashing = animationSpeed > moveSpeed + 0.1f;
            float currentSmoothTime = (isStopping && wasDashing) ? dashStopSmoothTime : walkStopSmoothTime;

            animationSpeed = Mathf.SmoothDamp(
                animationSpeed,
                targetSpeed,
                ref animationVelocity,
                currentSmoothTime
            );

            an.SetFloat("Speed", animationSpeed);
            
            // 💡 [수정] controller.isGrounded -> isGrounded (일관성 유지)
            an.SetBool("isJumping", !isGrounded);
        }

        HandleCameraFOV();
    }
    
    void HandleCameraFOV()
    {
        // 카메라가 할당되지 않았으면 아무것도 하지 않음
        if (freeLookCamera == null) return;

        float t = Mathf.InverseLerp(moveSpeed, dashSpeed, animationSpeed);
        float targetFOV = Mathf.Lerp(originalFOV, dashFOV, t);

        // Mathf.Lerp를 사용하여 현재 FOV에서 목표 FOV로 부드럽게 변경
         freeLookCamera.Lens.FieldOfView = Mathf.Lerp(
             freeLookCamera.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * fovSmoothSpeed
        );
    }

    #region Hanging Logic

    // ... (AttemptGrab, StartHanging, MoveToHangPosition, HandleHanging, Dismount 메서드는 기존과 동일) ...
    // (이하 생략)

    void AttemptGrab()
    {
        Vector3 grabCheckCenter = transform.position + transform.forward * hangDetectionOffset;
        Collider[] colliders = Physics.OverlapSphere(grabCheckCenter, hangDetectionRadius);
        Transform closestHangPoint = null;
        float closestDistSqr = float.MaxValue;
        foreach (var col in colliders)
        {
            if (col.CompareTag(hangPointLedgeTag))
            {
                Transform point = col.transform.Find(hangPointChildName);
                if (point != null)
                {
                    float distSqr = (point.position - transform.position).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                        closestHangPoint = point;
                    }
                }
            }
        }
        if (closestHangPoint != null)
        {
            StartHanging(closestHangPoint);
        }
    }

    void StartHanging(Transform hangPoint)
    {
        isMovingToHangPoint = true;
        currentHangTransform = hangPoint;
        controller.enabled = false;
        playerVelocity = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;
        an.SetBool("isJumping", false);
        an.SetBool("isHanging", true);
        an.SetBool("isLeapingFromHang", false);
        currentSwingPosition = 0f;
        currentSwingVelocity = 0f;
        an.SetFloat("SwingPower", 0f);

        // [핵심] 몸통 위치 계산을 가장 단순한 코드로 되돌립니다
        // (어깨 제어 코드 모두 삭제)
        Vector3 targetRootPosition = hangPoint.position + handToRootOffset;

        Quaternion targetRootRotation = Quaternion.LookRotation(-hangPoint.forward, Vector3.up);
        StartCoroutine(MoveToHangPosition(targetRootPosition, targetRootRotation));
    }

    IEnumerator MoveToHangPosition(Vector3 targetPos, Quaternion targetRot)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        while (elapsed < hangMoveDuration)
        {
            float t = elapsed / hangMoveDuration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        transform.rotation = targetRot;
        isMovingToHangPoint = false;
        isHanging = true;
    }

    void HandleHanging()
    {
        if (!Input.GetKey(KeyCode.E))
        {
            Dismount();
            return;
        }

        // --- 그네 물리 계산 ---
        float swingInput = Input.GetAxisRaw("Vertical");
        float restoringForce = -swingGravity * currentSwingPosition;
        float pumpForce = 0f;

        if (swingInput > 0.1f)
        {
            pumpForce = swingInput * swingPumpForce;
        }
        else if (swingInput < -0.1f)
        {
            pumpForce = swingInput * swingPumpForce;
        }

        float totalAcceleration = restoringForce + pumpForce;
        currentSwingVelocity += totalAcceleration * Time.deltaTime;
        currentSwingVelocity *= (1.0f - (swingDampening * Time.deltaTime));
        currentSwingPosition += currentSwingVelocity * Time.deltaTime;

        if (Mathf.Abs(currentSwingPosition) > maxSwingAngle)
        {
            currentSwingPosition = Mathf.Sign(currentSwingPosition) * maxSwingAngle;
            currentSwingVelocity = 0;
        }


        // swing에 따라 플레이어 위치 조정 (앞/뒤 분리)
        if (currentHangTransform != null)
        {
            Vector3 baseRootPosition = currentHangTransform.position + handToRootOffset;
            
            // swing 방향에 따라 다른 스케일 적용
            float movementScale = currentSwingPosition >= 0
                ? forwardSwingMovementScale
                : backwardSwingMovementScale;
            
            Vector3 swingOffset = transform.forward * (currentSwingPosition * movementScale);

            // ▼▼▼ [수정된 부분] ▼▼▼
            // Y축(높이) 값을 baseRootPosition.y로 강제 고정하여
            // 애니메이션 루트모션으로 인해 캐릭터가 아래로 꺼지는 현상을 방지합니다.
            Vector3 targetPosition = baseRootPosition + swingOffset;
            targetPosition.y = baseRootPosition.y;
            transform.position = targetPosition;
            // ▲▲▲ [수정 완료] ▲▲▲
            
            transform.rotation = Quaternion.LookRotation(-currentHangTransform.forward, Vector3.up);
        }
    }

    void Dismount()
    {
        isHanging = false;
        isMovingToHangPoint = false;
        StopAllCoroutines();

        isLeapingFromHang = true;
        an.SetBool("isHanging", false);
        an.SetBool("isLeapingFromHang", true);

        if (currentHangTransform != null)
        {
            // 현재 위치(X, Z)는 유지하되, Y(높이)만 강제로 리셋합니다.
            Vector3 safePosition = transform.position;
            Vector3 baseRootPosition = currentHangTransform.position + handToRootOffset;
            safePosition.y = baseRootPosition.y;
            transform.position = safePosition;
        }

        controller.enabled = true;

        // --- 도약 속도 계산 ---
        float forwardLeapForce = currentSwingVelocity * leapVelocityFactor;
        float heightFactor = Mathf.InverseLerp(0, maxSwingAngle, Mathf.Abs(currentSwingPosition));
        float upwardLeapForce = baseLeapUpwardForce + (heightFactor * addedLeapUpwardForce);

        if (Mathf.Abs(currentSwingVelocity) < 0.2f && Mathf.Abs(currentSwingPosition) < 0.2f)
        {
            upwardLeapForce = 0f;
            forwardLeapForce = 0f;
        }

        playerVelocity.y = Mathf.Sqrt(upwardLeapForce * -2f * gravity);
        currentHorizontalVelocity = transform.forward * forwardLeapForce;

        currentSwingPosition = 0f;
        currentSwingVelocity = 0f;
    }


    #endregion


    public void TeleportTo(Vector3 destination)
    {

        TeleportTo(destination, transform.rotation);

        playerVelocity = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;
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
                Debug.LogError("PlayerController_Monkey: CharacterController를 찾을 수 없습니다!");
                return;
            }
        }

        ResetHangingState();

        controller.enabled = false;
        transform.position = destination;
        transform.rotation = newRotation;
        controller.enabled = true;


        Debug.Log($"플레이어를 {destination} 위치, {newRotation.eulerAngles} 회전으로 텔레포트했습니다.");
    }

    public Vector3 CurrentVelocity
    {
        get { return currentHorizontalVelocity + new Vector3(0, playerVelocity.y, 0); }
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        // 스왑 시 속도 적용
        currentHorizontalVelocity = new Vector3(newVelocity.x, 0, newVelocity.z);
        playerVelocity.y = newVelocity.y;

        // 땅 뚫림 보정 코루틴이 즉시 동작하도록 지면 상태를 강제 해제
        timeLastGrounded = 0f;
        isGrounded = false;
        an.SetBool("isJumping", true); 
    }


    private void  ResetHangingState()
    {
        if (isHanging || isMovingToHangPoint || isLeapingFromHang)
        {
            StopAllCoroutines();
            isHanging = false;
            isMovingToHangPoint = false;
            isLeapingFromHang = false;
            currentSwingPosition = 0f;
            currentSwingVelocity = 0f;

            an.SetBool("isJumping", false);
            an.SetBool("isHanging", false);
            an.SetBool("isLeapingFromHang", false);
            an.SetFloat("SwingPower", 0f);
        }
    }
    
}