using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

/// <summary>
/// 플레이어 스왑 기능을 제공하는 매니저 클래스
/// Q키를 누르면 등록된 2개의 캐릭터를 번갈아 변경합니다.
/// </summary>
public class SwapManager : MonoBehaviour
{
    [Header("Player References")]
    public GameObject monkey;
    public GameObject rabbit;

    [Header("Camera Reference")]
    [Tooltip("Freelook 카메라 (타겟 변경용)")]
    public CinemachineCamera freeLookCamera;

    [Header("Swap Settings")]
    [Tooltip("스왑 쿨다운 시간 (초)")]
    public float swapCooldown = 0.5f;

    [Header("Teleport Height Adjustments")]
    [Tooltip("Monkey에서 Rabbit으로 스왑할 때 높이 조정 (지상)")]
    public float monkeyToRabbitHeightOffset = 0f;

    [Tooltip("Rabbit에서 Monkey로 스왑할 때 높이 조정 (지상)")]
    public float rabbitToMonkeyHeightOffset = 0f;

    [Tooltip("Monkey에서 Rabbit으로 스왑할 때 높이 조정 (공중)")]
    public float monkeyToRabbitHeightOffset_Air = 0f;

    [Tooltip("Rabbit에서 Monkey로 스왑할 때 높이 조정 (공중)")]
    public float rabbitToMonkeyHeightOffset_Air = 0f;

    [Header("Ground-Clip Correction")]
    [Tooltip("스왑 직후 땅 뚫림을 감지할 시간 (초)")]
    public float correctionDuration = 0.5f;

    [Tooltip("땅 뚫림 보정 시, 땅 위로 띄울 높이 (요청: 0.1f)")]
    public float correctionOffset = 0.1f;

    [Tooltip("땅으로 인식할 레이어 마스크")]
    public LayerMask groundLayerMask;


    // 내부 상태 변수
    private GameObject currentPlayer;
    private GameObject inactivePlayer;
    private float lastSwapTime = 0f;

    void Start()
    {
        // 초기 설정: rabbit을 활성화하고 monkey을 비활성화 (기존 코드 유지)
        if (monkey != null && rabbit != null)
        {
            // Start()에서 rabbit을 활성화했으므로 currentPlayer는 rabbit이어야 합니다.
            // 하지만 현재 코드는 rabbit을 활성화하고 currentPlayer = rabbit을 제대로 수행하므로 문제가 없습니다.
            // (만약 초기화 순서를 반대로 하고 싶다면 아래 두 줄을 변경하세요.)
            currentPlayer = rabbit;
            inactivePlayer = monkey;

            monkey.SetActive(false);
            rabbit.SetActive(true);

            // 카메라 타겟을 현재 플레이어로 설정
            UpdateCameraTarget(currentPlayer.transform);
        }
        else
        {
            Debug.LogError("SwapManager: Monkey 또는 Rabbit이 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        // Q키 입력 감지 및 쿨다운 확인
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (Time.time - lastSwapTime >= swapCooldown)
            {
                SwapPlayers();
                lastSwapTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 플레이어를 스왑하는 메인 함수
    /// </summary>
    void SwapPlayers()
    {
        if (currentPlayer == null || inactivePlayer == null)
        {
            Debug.LogWarning("SwapManager: 플레이어 참조가 유효하지 않습니다.");
            return;
        }

        // 1. 현재 플레이어의 상태 저장
        Vector3 currentPosition = currentPlayer.transform.position;
        Quaternion currentRotation = currentPlayer.transform.rotation;

        CharacterController currentController = currentPlayer.GetComponent<CharacterController>();
        bool isGrounded = (currentController != null && currentController.isGrounded);

        // [추가/통합] 속도 저장
        Vector3 storedVelocity = GetPlayerVelocity(currentPlayer);


        // 지상일 때만 땅에 박히는 현상을 방지하는 미세한 높이 보정 (0.05f)
        if (isGrounded)
        {
            currentPosition.y += 0.05f;
        }


        // 2. 스왑 방향에 따른 높이 오프셋 계산
        float heightOffset = 0f;
        bool isMonkeyToRabbit = currentPlayer == monkey && inactivePlayer == rabbit;
        // bool isRabbitToMonkey = currentPlayer == rabbit && inactivePlayer == monkey; // 지역 변수 사용 안 함

        if (isMonkeyToRabbit)
        {
            heightOffset = isGrounded ? monkeyToRabbitHeightOffset : monkeyToRabbitHeightOffset_Air;
        }
        else // RabbitToMonkey
        {
            heightOffset = isGrounded ? rabbitToMonkeyHeightOffset : rabbitToMonkeyHeightOffset_Air;
        }

        currentPosition.y += heightOffset;

        // 3. 플레이어 활성/비활성 상태 변경 및 코루틴 시작
        currentPlayer.SetActive(false);
        inactivePlayer.SetActive(true);
        // freeLookCamera.Lens.FieldOfView = 55; // PlayerController에서 관리하도록 위임

        // [수정] 코루틴 이름 변경 및 속도 인자 추가
        StartCoroutine(TeleportAndApplyVelocity(inactivePlayer, currentPosition, currentRotation, storedVelocity));

        // 4. 현재 플레이어와 비활성 플레이어 참조 교체
        GameObject temp = currentPlayer;
        currentPlayer = inactivePlayer;
        inactivePlayer = temp;

        Debug.Log($"플레이어 스왑: {currentPlayer.name}로 변경되었습니다. (지상: {isGrounded}, 적용 오프셋: {heightOffset}, 적용 속도: {storedVelocity})");
    }

    /// <summary>
    /// 플레이어 컨트롤러에서 현재 속도를 가져오는 헬퍼 메서드
    /// </summary>
    private Vector3 GetPlayerVelocity(GameObject player)
    {
        if (player.TryGetComponent<PlayerController_Monkey>(out var monkeyController))
        {
            return monkeyController.CurrentVelocity;
        }
        else if (player.TryGetComponent<PlayerController_Rabbit>(out var rabbitController))
        {
            return rabbitController.CurrentVelocity;
        }
        return Vector3.zero;
    }


    /// <summary>
    /// GameObject 활성화 후 한 프레임 대기하여 텔레포트하고 속도를 적용합니다.
    /// (이전 TeleportAfterActivation 코루틴 대체)
    /// </summary>
    IEnumerator TeleportAndApplyVelocity(GameObject player, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        // 한 프레임 대기 (Awake와 Start가 실행되도록)
        yield return null;

        if (player != null)
        {
            PlayerController_Monkey monkeyController = player.GetComponent<PlayerController_Monkey>();
            PlayerController_Rabbit rabbitController = player.GetComponent<PlayerController_Rabbit>();

            // [통합] TeleportTo와 SetVelocity를 함께 호출
            if (monkeyController != null)
            {
                monkeyController.TeleportTo(position, rotation);
                monkeyController.SetVelocity(velocity);
            }
            else if (rabbitController != null)
            {
                rabbitController.TeleportTo(position, rotation);
                rabbitController.SetVelocity(velocity);
            }
            else
            {
                Debug.LogWarning($"SwapManager: {player.name}에 PlayerController 컴포넌트를 찾을 수 없습니다.");
            }

            // 텔레포트가 완료된 직후, 여기서 카메라 타겟을 업데이트합니다.
            UpdateCameraTarget(player.transform);

            // 땅 뚫림 검사 시작
            StartCoroutine(CheckGroundPenetration(player));
        }
    }

    /// <summary>
    /// Freelook 카메라의 타겟을 변경합니다.
    /// </summary>
    void UpdateCameraTarget(Transform target)
    {
        if (freeLookCamera == null)
        {
            Debug.LogWarning("SwapManager: Freelook 카메라가 할당되지 않았습니다.");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("SwapManager: 타겟 Transform이 null입니다.");
            return;
        }

        // CinemachineCamera의 Follow 타겟 설정
        freeLookCamera.Follow = target;

        // CinemachineCamera의 LookAt 타겟 설정
        freeLookCamera.LookAt = target;
    }

    /// <summary>
    /// 외부에서 현재 활성 플레이어를 가져오는 메서드
    /// </summary>
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }

    /// <summary>
    /// 외부에서 비활성 플레이어를 가져오는 메서드
    /// </summary>
    public GameObject GetInactivePlayer()
    {
        return inactivePlayer;
    }

    /// <summary>
    /// 스왑 텔레포트 직후, 정해진 시간(correctionDuration) 동안 땅 뚫림을 감지하고 보정합니다.
    /// </summary>
    IEnumerator CheckGroundPenetration(GameObject player)
    {
        // ... (기존 GroundPenetration 코루틴 로직 유지) ...

        if (player == null) yield break;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogWarning($"[SwapManager] GroundPenetration check: {player.name}에 CharacterController가 없습니다.");
            yield break;
        }

        // 0.5초 타이머 시작
        float endTime = Time.time + correctionDuration;
        Transform playerTransform = player.transform;

        while (Time.time < endTime)
        {
            // 캐릭터의 중심에서 아래로 레이캐스트를 쏴서 땅을 찾습니다.
            // (캐릭터 높이 절반 + 1m) 만큼 넉넉하게 쏩니다.
            Vector3 rayOrigin = playerTransform.position + controller.center;
            float rayDistance = (controller.height * 0.5f) + 1.0f;
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance, groundLayerMask))
            {
                // 땅을 감지함
                float groundHeight = hit.point.y;

                // 캐릭터의 실제 발 위치 (캡슐의 최하단)
                float playerFeetY = (playerTransform.position + controller.center).y - (controller.height * 0.5f);

                // 캐릭터의 발이 땅보다 0.01f (허용 오차) 이상 아래로 내려갔는지 확인
                if (playerFeetY < groundHeight - 0.01f)
                {
                    Debug.LogWarning($"[SwapManager] 땅 뚫림 감지! 보정 실행. PlayerFeet: {playerFeetY}, Ground: {groundHeight}");

                    // 보정 위치 계산 (땅 높이 + correctionOffset)
                    float desiredFeetY = groundHeight + correctionOffset;

                    // 얼마나 위로 올려야 하는지 계산
                    float correctionAmount = desiredFeetY - playerFeetY;

                    // 현재 위치에서 y값만 보정한 새 위치
                    Vector3 correctedPosition = playerTransform.position + new Vector3(0, correctionAmount, 0);

                    // 플레이어의 자체 텔레포트 기능을 사용하여 위치 보정
                    PlayerController_Monkey monkeyController = player.GetComponent<PlayerController_Monkey>();
                    PlayerController_Rabbit rabbitController = player.GetComponent<PlayerController_Rabbit>();

                    if (monkeyController != null)
                    {
                        monkeyController.TeleportTo(correctedPosition, playerTransform.rotation);
                    }
                    else if (rabbitController != null)
                    {
                        rabbitController.TeleportTo(correctedPosition, playerTransform.rotation);
                    }

                    Debug.Log($"[SwapManager] 보정 완료. 새 위치 Y: {correctedPosition.y}");

                    // 보정이 완료되었으므로 코루틴 즉시 종료
                    yield break;
                }
            }

            // 다음 프레임까지 대기
            yield return null;
        }
    }
}