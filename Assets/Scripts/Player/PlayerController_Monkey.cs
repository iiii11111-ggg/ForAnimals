using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Monkey : MonoBehaviour
{
    // ... (기존 변수들) ...
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float slideSpeed = 4f;
    public float turnSpeed = 12f;
    public float gravity = -9.81f;
    [Tooltip("플레이어가 설 수 있는 최대 경사 각도.")]
    public float maxSlopeAngle = 45f;

    [Header("Air Control Settings")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.2f;
    public float airControlSpeed = 1.5f;

    [Header("Ground Check Settings")]
    [Tooltip("바닥으로 인식할 레이어")]
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;
    [Tooltip("땅에서 떨어진 후 점프가 가능한 시간 (코요테 타임)")]
    public float coyoteTime = 0.1f;

    [Header("Hanging Settings")]
    public float hangDetectionRadius = 1.0f;
    public float hangDetectionOffset = 0.5f;
    public string hangPointLedgeTag = "Ledge";
    public string hangPointChildName = "HangPoint";
    public Vector3 handToRootOffset = Vector3.zero;
    public float hangMoveDuration = 0.3f;

    [Header("Swinging Physics")]
    public float swingGravity = 9.8f;
    public float swingPumpForce = 5f;
    public float swingDampening = 0.1f;
    public float maxSwingAngle = 1.8f;
    public float forwardSwingMovementScale = 0.5f;
    public float backwardSwingMovementScale = 0.5f;

    [Header("Swinging Leap Settings")]
    public float leapVelocityFactor = 5f;
    public float baseLeapUpwardForce = 4f;
    public float addedLeapUpwardForce = 6f;

    [Header("Control")]
    public bool canMove = true;

    [Header("Camera Settings")]
    public CinemachineCamera freeLookCamera;
    public float dashFOV = 50f;
    public float fovSmoothSpeed = 5f;

    // ▼▼▼ [추가됨] UI 및 타이머 설정 ▼▼▼
    [Header("UI Settings")]
    public GameObject HoldUi;
    private float uiBlockTimer = 0f; // 매달리기 해제 후 UI 표시를 막는 쿨타임
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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

    private bool isGrounded = false;
    private float timeLastGrounded = 0f;

    private bool isSliding = false;
    private Vector3 slopeNormal;

    private bool isHanging = false;
    private bool isMovingToHangPoint = false;
    private Transform currentHangTransform;
    private bool isLeapingFromHang = false;

    private float currentSwingPosition = 0f;
    private float currentSwingVelocity = 0f;

    private float lastInputMagnitude = 0f;

    private bool isDashing = false;
    private float originalFOV;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        an = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;

        if (controller != null)
        {
            controller.slopeLimit = maxSlopeAngle;
        }

        if (freeLookCamera != null)
        {
            originalFOV = freeLookCamera.Lens.FieldOfView;
        }
        else
        {
            Debug.LogWarning("FreeLook Camera가 할당되지 않았습니다.");
        }

        // UI 초기화
        if (HoldUi != null) HoldUi.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;

        // ▼▼▼ [핵심 로직 변경] 감지 -> UI반영 -> 입력처리를 한 흐름으로 통합 ▼▼▼

        // 1. UI 차단 타이머 감소
        if (uiBlockTimer > 0f)
        {
            uiBlockTimer -= Time.deltaTime;
        }

        // 2. 주변 감지 (매달리지 않았을 때만)
        Transform detectedLedge = null;
        if (!isHanging && !isMovingToHangPoint)
        {
            detectedLedge = DetectNearestHangPoint();
        }

        // 3. UI 표시 처리
        //    (감지된 게 있고 && 차단 타이머가 끝났고 && 현재 매달린 상태가 아님)
        if (HoldUi != null)
        {
            bool showUI = (detectedLedge != null) && (uiBlockTimer <= 0f);
            HoldUi.SetActive(showUI);
        }

        // 4. 매달리기 입력 처리
        //    (별도로 다시 감지하지 않고, 위에서 감지한 detectedLedge를 바로 사용 -> 성능 최적화)
        if (canMove && Input.GetKeyDown(KeyCode.E) && detectedLedge != null)
        {
            StartHanging(detectedLedge);
            if (isMovingToHangPoint) return; // 매달리기 시작했으면 아래 이동 로직 패스
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲


        // --- 0. 특수 상태(매달리기 중) ---
        if (isHanging)
        {
            HandleHanging();
            return;
        }

        if (isMovingToHangPoint) return;


        // --- 1. 중력 적용 ---
        if (!isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        // --- 2. 상태 초기화 ---
        isGrounded = false;

        // --- 3. 입력 및 회전 ---
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

        if (inputDirection.magnitude >= 0.1f && !isLeapingFromHang)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            moveDirection = targetRotation * Vector3.forward;
        }
        else
        {
            isDashing = false;
        }

        // --- 4. 수평 이동 ---
        bool isActuallyGrounded = (Time.time - timeLastGrounded <= coyoteTime);

        if (isActuallyGrounded && !isSliding)
        {
            if (!isLeapingFromHang)
            {
                isDashing = (inputDirection.magnitude >= 0.1f) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
                currentMaxSpeed = isDashing ? dashSpeed : moveSpeed;
                currentHorizontalVelocity = moveDirection * currentMaxSpeed;
            }
        }
        else
        {
            isDashing = false;
            currentMaxSpeed = airControlSpeed;

            if (isSliding)
            {
                Vector3 slideVector = Vector3.ProjectOnPlane(Vector3.down, slopeNormal);
                Vector3 slideDirection = new Vector3(slideVector.x, 0, slideVector.z).normalized;
                Vector3 slideVelocity = slideDirection * slideSpeed;
                Vector3 inputVelocity = moveDirection * currentMaxSpeed;

                if (!isLeapingFromHang)
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
        }

        // --- 5. 점프 ---
        if (canMove && Input.GetKeyDown(KeyCode.Space) && (Time.time - timeLastGrounded <= coyoteTime) && !isLeapingFromHang)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            timeLastGrounded = 0f;
        }

        // --- 6. 최종 이동 ---
        controller.Move((currentHorizontalVelocity + playerVelocity) * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (BackButtonManager.Instance != null && BackButtonManager.Instance.IsPaused) return;

        bool isActuallyGrounded = (Time.time - timeLastGrounded <= coyoteTime);

        if (isHanging || isMovingToHangPoint)
        {
            an.SetFloat("SwingPower", currentSwingPosition);
        }
        else if (isLeapingFromHang)
        {
            // 도약 중
        }
        else
        {
            float targetSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

            if (lastInputMagnitude < 0.1f && isActuallyGrounded)
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
            an.SetBool("isJumping", !isActuallyGrounded && !isSliding);
        }

        HandleCameraFOV();
    }

    void HandleCameraFOV()
    {
        if (freeLookCamera == null) return;
        float t = Mathf.InverseLerp(moveSpeed, dashSpeed, animationSpeed);
        float targetFOV = Mathf.Lerp(originalFOV, dashFOV, t);
        freeLookCamera.Lens.FieldOfView = Mathf.Lerp(
            freeLookCamera.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * fovSmoothSpeed
        );
    }

    #region Hanging Logic

    // ▼▼▼ [분리됨] 감지 로직을 별도 함수로 분리 (유지보수성 UP) ▼▼▼
    Transform DetectNearestHangPoint()
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
        return closestHangPoint;
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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

        // (매달리기 시작 시 UI가 즉시 꺼지도록 타이머나 조건은 Update 루프에서 자동 처리됨)

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

        float swingInput = Input.GetAxisRaw("Vertical");
        float restoringForce = -swingGravity * currentSwingPosition;
        float pumpForce = 0f;

        if (swingInput > 0.1f) pumpForce = swingInput * swingPumpForce;
        else if (swingInput < -0.1f) pumpForce = swingInput * swingPumpForce;

        float totalAcceleration = restoringForce + pumpForce;
        currentSwingVelocity += totalAcceleration * Time.deltaTime;
        currentSwingVelocity *= (1.0f - (swingDampening * Time.deltaTime));
        currentSwingPosition += currentSwingVelocity * Time.deltaTime;

        if (Mathf.Abs(currentSwingPosition) > maxSwingAngle)
        {
            currentSwingPosition = Mathf.Sign(currentSwingPosition) * maxSwingAngle;
            currentSwingVelocity = 0;
        }

        if (currentHangTransform != null)
        {
            Vector3 baseRootPosition = currentHangTransform.position + handToRootOffset;
            float movementScale = currentSwingPosition >= 0 ? forwardSwingMovementScale : backwardSwingMovementScale;
            Vector3 swingOffset = transform.forward * (currentSwingPosition * movementScale);
            Vector3 targetPosition = baseRootPosition + swingOffset;
            targetPosition.y = baseRootPosition.y;
            transform.position = targetPosition;
            transform.rotation = Quaternion.LookRotation(-currentHangTransform.forward, Vector3.up);
        }
    }

    void Dismount()
    {
        // ▼▼▼ [핵심] 매달리기 해제 시 0.2초간 UI 차단 ▼▼▼
        uiBlockTimer = 0.2f;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        isHanging = false;
        isMovingToHangPoint = false;
        StopAllCoroutines();

        isLeapingFromHang = true;
        an.SetBool("isHanging", false);
        an.SetBool("isLeapingFromHang", true);

        if (currentHangTransform != null)
        {
            Vector3 safePosition = transform.position;
            Vector3 baseRootPosition = currentHangTransform.position + handToRootOffset;
            safePosition.y = baseRootPosition.y;
            transform.position = safePosition;
        }

        controller.enabled = true;

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

    // ... (텔레포트 및 기타 유틸리티 함수들은 그대로 유지) ...
    public void TeleportTo(Vector3 destination)
    {
        TeleportTo(destination, transform.rotation);
        playerVelocity = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;
        Debug.Log($"플레이어를 {destination} 위치로 텔레포트했습니다.");
    }

    public void TeleportTo(Vector3 destination, Quaternion newRotation)
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
            if (controller == null) return;
        }
        ResetHangingState();
        controller.enabled = false;
        transform.position = destination;
        transform.rotation = newRotation;
        controller.enabled = true;
    }

    public Vector3 CurrentVelocity
    {
        get { return currentHorizontalVelocity + new Vector3(0, playerVelocity.y, 0); }
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        currentHorizontalVelocity = new Vector3(newVelocity.x, 0, newVelocity.z);
        playerVelocity.y = newVelocity.y;
        timeLastGrounded = 0f;
        isGrounded = false;
        an.SetBool("isJumping", true);
    }

    private void ResetHangingState()
    {
        if (isHanging || isMovingToHangPoint || isLeapingFromHang)
        {
            StopAllCoroutines();
            isHanging = false;
            isMovingToHangPoint = false;
            isLeapingFromHang = false;
            currentSwingPosition = 0f;
            currentSwingVelocity = 0f;

            uiBlockTimer = 0f; // 리셋 시 타이머도 초기화

            an.SetBool("isJumping", false);
            an.SetBool("isHanging", false);
            an.SetBool("isLeapingFromHang", false);
            an.SetFloat("SwingPower", 0f);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isHanging || isMovingToHangPoint) return;

        if ((groundLayer.value & (1 << hit.gameObject.layer)) == 0)
        {
            return;
        }

        float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);

        if (surfaceAngle < controller.slopeLimit)
        {
            if (playerVelocity.y < 0.1f)
            {
                isGrounded = true;
                isSliding = false;

                timeLastGrounded = Time.time;

                playerVelocity.y = -2f;

                an.SetBool("isJumping", false);
                if (isLeapingFromHang)
                {
                    isLeapingFromHang = false;
                    an.SetBool("isLeapingFromHang", false);
                }
            }
        }
        else
        {
            if (surfaceAngle > 89.0f)
            {
                isGrounded = false;
                isSliding = false;
                timeLastGrounded = 0f;
                return;
            }

            isGrounded = false;
            isSliding = true;
            slopeNormal = hit.normal;
            timeLastGrounded = 0f;
        }
    }
}