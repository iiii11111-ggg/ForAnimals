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

    // ▼▼▼ [수정 1] 공중 스왑을 위한 오프셋 변수 추가 ▼▼▼
    [Tooltip("Monkey에서 Rabbit으로 스왑할 때 높이 조정 (공중)")]
    public float monkeyToRabbitHeightOffset_Air = 0f;

    [Tooltip("Rabbit에서 Monkey로 스왑할 때 높이 조정 (공중)")]
    public float rabbitToMonkeyHeightOffset_Air = 0f;
    // ▲▲▲ [수정 1] 완료 ▲▲▲


    // 내부 상태 변수
    private GameObject currentPlayer;
    private GameObject inactivePlayer;
    private float lastSwapTime = 0f;

    void Start()
    {
        // 초기 설정: monkey을 활성화하고 rabbit을 비활성화
        if (monkey != null && rabbit != null)
        {
            currentPlayer = monkey;
            inactivePlayer = rabbit;

            monkey.SetActive(true);
            rabbit.SetActive(false);

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
        // 플레이어 참조 확인
        if (currentPlayer == null || inactivePlayer == null)
        {
            Debug.LogWarning("SwapManager: 플레이어 참조가 유효하지 않습니다.");
            return;
        }

        // 현재 플레이어의 위치 저장
        Vector3 currentPosition = currentPlayer.transform.position;
        Quaternion currentRotation = currentPlayer.transform.rotation;

        // ▼▼▼ [수정 2] 현재 플레이어의 지상 상태(isGrounded)를 확인합니다. ▼▼▼
        CharacterController currentController = currentPlayer.GetComponent<CharacterController>();
        bool isGrounded = (currentController != null && currentController.isGrounded);

        // 지상일 때만 땅에 박히는 현상을 방지하는 미세한 높이 보정을 추가합니다.
        if (isGrounded)
        {
            // (참고: 이 0.05f 값은 기존 코드에 있던 값입니다)
            currentPosition.y += 0.05f;
        }
        // ▲▲▲ [수정 2] 완료 ▲▲▲


        // 스왑 방향에 따른 높이 오프셋 계산
        float heightOffset = 0f;
        bool isMonkeyToRabbit = currentPlayer == monkey && inactivePlayer == rabbit;
        bool isRabbitToMonkey = currentPlayer == rabbit && inactivePlayer == monkey;

        // ▼▼▼ [수정 3] isGrounded 상태에 따라 지상/공중 오프셋을 선택합니다. ▼▼▼
        if (isMonkeyToRabbit)
        {
            heightOffset = isGrounded ? monkeyToRabbitHeightOffset : monkeyToRabbitHeightOffset_Air;
        }
        else if (isRabbitToMonkey)
        {
            heightOffset = isGrounded ? rabbitToMonkeyHeightOffset : rabbitToMonkeyHeightOffset_Air;
        }
        // ▲▲▲ [수정 3] 완료 ▲▲▲

        // 높이 오프셋 적용
        currentPosition.y += heightOffset;

        // 플레이어 활성/비활성 상태 변경
        currentPlayer.SetActive(false);
        inactivePlayer.SetActive(true);

        // 코루틴에 Quaternion 값을 전달합니다.
        StartCoroutine(TeleportAfterActivation(inactivePlayer, currentPosition, currentRotation));

        // 현재 플레이어와 비활성 플레이어 참조 교체
        GameObject temp = currentPlayer;
        currentPlayer = inactivePlayer;
        inactivePlayer = temp;

        Debug.Log($"플레이어 스왑: {currentPlayer.name}로 변경되었습니다. (지상: {isGrounded}, 적용 오프셋: {heightOffset})");
    }

    /// <summary>
    /// GameObject 활성화 후 한 프레임 대기하여 컴포넌트 초기화를 보장한 뒤 텔레포트
    /// </summary>
    IEnumerator TeleportAfterActivation(GameObject player, Vector3 position, Quaternion rotation)
    {
        // 한 프레임 대기 (Awake와 Start가 실행되도록)
        yield return null;

        // 텔레포트 실행
        if (player != null)
        {
            PlayerController_Monkey monkeyController = player.GetComponent<PlayerController_Monkey>();
            PlayerController_Rabbit rabbitController = player.GetComponent<PlayerController_Rabbit>();

            if (monkeyController != null)
            {
                monkeyController.TeleportTo(position, rotation);
            }
            else if (rabbitController != null)
            {
                // (참고: 토끼 컨트롤러에도 TeleportTo(position, rotation) 오버로드가 필요합니다)
                rabbitController.TeleportTo(position, rotation);
            }
            else
            {
                Debug.LogWarning($"SwapManager: {player.name}에 PlayerController 컴포넌트를 찾을 수 없습니다.");
            }

            // 텔레포트가 완료된 직후, 여기서 카메라 타겟을 업데이트합니다.
            UpdateCameraTarget(player.transform);
        }
    }

    /// <summary>
    /// Freelook 카메라의 타겟을 변경합니다.
    /// </summary>
    /// <param name="target">새로운 타겟 Transform</param>
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
    /// <returns>현재 활성화된 플레이어 GameObject</returns>
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }

    /// <summary>
    /// 외부에서 비활성 플레이어를 가져오는 메서드
    /// </summary>
    /// <returns>현재 비활성화된 플레이어 GameObject</returns>
    public GameObject GetInactivePlayer()
    {
        return inactivePlayer;
    }
}