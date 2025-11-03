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
    [Tooltip("Monkey에서 Rabbit으로 스왑할 때 높이 조정 (양수: 위로, 음수: 아래로)")]
    public float monkeyToRabbitHeightOffset = 0f;
    
    [Tooltip("Rabbit에서 Monkey로 스왑할 때 높이 조정 (양수: 위로, 음수: 아래로)")]
    public float rabbitToMonkeyHeightOffset = 0f;

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

        CharacterController currentController = currentPlayer.GetComponent<CharacterController>();

        if (currentController != null && currentController.isGrounded)
        {
            currentPosition.y += 0.05f; 
        }


        // 스왑 방향에 따른 높이 오프셋 계산
        float heightOffset = 0f;
        bool isMonkeyToRabbit = currentPlayer == monkey && inactivePlayer == rabbit;
        bool isRabbitToMonkey = currentPlayer == rabbit && inactivePlayer == monkey;
        
        if (isMonkeyToRabbit)
        {
            heightOffset = monkeyToRabbitHeightOffset;
        }
        else if (isRabbitToMonkey)
        {
            heightOffset = rabbitToMonkeyHeightOffset;
        }

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

        // [수정 3] 카메라 타겟 업데이트 호출을 여기서 삭제합니다.
        // UpdateCameraTarget(currentPlayer.transform); // <-- 이 줄 삭제

        Debug.Log($"플레이어 스왑: {currentPlayer.name}로 변경되었습니다. (높이 오프셋: {heightOffset})");
    }

    /// <summary>
    /// GameObject 활성화 후 한 프레임 대기하여 컴포넌트 초기화를 보장한 뒤 텔레포트
    /// </summary>
    // [수정 4] 매개변수를 Vector3 rotation -> Quaternion rotation으로 변경
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
                // [수정 5] Quaternion.Euler(rotation) 대신 rotation을 바로 전달
                monkeyController.TeleportTo(position, rotation);
            }
            else if (rabbitController != null)
            {
                // [수정 5] Quaternion.Euler(rotation) 대신 rotation을 바로 전달
                rabbitController.TeleportTo(position, rotation);
            }
            else
            {
                Debug.LogWarning($"SwapManager: {player.name}에 PlayerController 컴포넌트를 찾을 수 없습니다.");
            }

            // [수정 6] 텔레포트가 완료된 직후, 여기서 카메라 타겟을 업데이트합니다.
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