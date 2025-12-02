using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using TMPro; // 텍스트 제어용
using UnityEngine.UI; // Image 제어용

/// <summary>
/// 플레이어 스왑 기능을 제공하는 매니저 클래스
/// Q키를 누르면 등록된 2개의 캐릭터를 번갈아 변경합니다.
/// </summary>
public class SwapManager : MonoBehaviour
{
    [Header("Player References")]
    public GameObject monkey;
    public GameObject rabbit;
    public GameObject MUI, RUI;

    [Header("UI Settings")]
    [Tooltip("쿨타임 시간을 표시할 텍스트 (TextMeshProUGUI)")]
    public TextMeshProUGUI cooldownText;

    [Tooltip("UI가 준비되었을 때의 투명도 (0~1)")]
    public float readyAlpha = 225f / 255f; // 약 0.88

    [Tooltip("쿨타임 중일 때의 투명도 (0~1)")]
    public float cooldownAlpha = 110f / 255f; // 약 0.43

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
    private GameObject currentPlayer, currentUI;
    private GameObject inactivePlayer, inactiveUI;
    private float lastSwapTime = -10f; // 시작 즉시 스왑 가능하도록 초기화

    void Start()
    {
        if (monkey != null && rabbit != null)
        {
            currentPlayer = rabbit;
            inactivePlayer = monkey;

            currentUI = RUI;
            inactiveUI = MUI;

            monkey.SetActive(false);
            rabbit.SetActive(true);

            MUI.SetActive(false);
            RUI.SetActive(true);

            UpdateCameraTarget(currentPlayer.transform);

            // UI 초기화: 텍스트 숨김, 현재 UI 알파값 설정
            if (cooldownText != null) cooldownText.gameObject.SetActive(false);
            SetUIAlpha(currentUI, readyAlpha);
        }
        else
        {
            Debug.LogError("SwapManager: Monkey 또는 Rabbit이 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        // 1. 쿨타임 UI 상태 업데이트 (매 프레임 체크)
        HandleCooldownUI();

        // 2. 입력 감지
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
    /// 남은 쿨타임을 계산하여 텍스트를 표시하고 UI 투명도를 조절합니다.
    /// </summary>
    void HandleCooldownUI()
    {
        float timePassed = Time.time - lastSwapTime;
        float remainingTime = swapCooldown - timePassed;

        if (remainingTime > 0)
        {
            // [쿨타임 중]
            if (cooldownText != null)
            {
                if (!cooldownText.gameObject.activeSelf)
                    cooldownText.gameObject.SetActive(true);

                // 소수점 첫째자리까지 표시 (0.4s)
                cooldownText.text = $"{remainingTime:F1}";
            }

            // 현재 UI 투명하게 (110)
            SetUIAlpha(currentUI, cooldownAlpha);
        }
        else
        {
            // [쿨타임 종료 - 준비 완료]
            if (cooldownText != null && cooldownText.gameObject.activeSelf)
            {
                cooldownText.gameObject.SetActive(false);
            }

            // 현재 UI 선명하게 (225)
            SetUIAlpha(currentUI, readyAlpha);
        }
    }

    /// <summary>
    /// Image 컴포넌트의 Color Alpha값을 직접 수정합니다.
    /// </summary>
    void SetUIAlpha(GameObject uiObject, float alpha)
    {
        if (uiObject == null) return;

        // 1. 먼저 자기 자신에게 Image가 있는지 확인
        Image img = uiObject.GetComponent<Image>();

        // 2. 없으면 자식 오브젝트들 중에서 Image를 찾음 (안전장치)
        if (img == null)
        {
            img = uiObject.GetComponentInChildren<Image>();
        }

        // 3. 이미지를 찾았으면 알파값 변경
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
        else
        {
            // 만약 여전히 못 찾았다면 콘솔에 경고를 띄움
            Debug.LogWarning($"SwapManager: '{uiObject.name}' 또는 그 자식에서 Image 컴포넌트를 찾을 수 없습니다. 인스펙터를 확인하세요.");
        }
    }

    void SwapPlayers()
    {
        if (currentPlayer == null || inactivePlayer == null) return;

        // 1. 현재 상태 저장
        Vector3 currentPosition = currentPlayer.transform.position;
        Quaternion currentRotation = currentPlayer.transform.rotation;
        CharacterController currentController = currentPlayer.GetComponent<CharacterController>();
        bool isGrounded = (currentController != null && currentController.isGrounded);
        Vector3 storedVelocity = GetPlayerVelocity(currentPlayer);

        if (isGrounded) currentPosition.y += 0.05f;

        // 2. 높이 오프셋 계산
        float heightOffset = 0f;
        bool isMonkeyToRabbit = currentPlayer == monkey && inactivePlayer == rabbit;

        if (isMonkeyToRabbit)
            heightOffset = isGrounded ? monkeyToRabbitHeightOffset : monkeyToRabbitHeightOffset_Air;
        else
            heightOffset = isGrounded ? rabbitToMonkeyHeightOffset : rabbitToMonkeyHeightOffset_Air;

        currentPosition.y += heightOffset;

        // 3. 플레이어 및 UI 교체
        currentPlayer.SetActive(false);
        inactivePlayer.SetActive(true);

        // UI 오브젝트 교체
        currentUI.SetActive(false);
        inactiveUI.SetActive(true);

        StartCoroutine(TeleportAndApplyVelocity(inactivePlayer, currentPosition, currentRotation, storedVelocity));

        // 4. 참조 변수 스왑
        GameObject tempPlayer = currentPlayer;
        currentPlayer = inactivePlayer;
        inactivePlayer = tempPlayer;

        GameObject tempUI = currentUI;
        currentUI = inactiveUI;
        inactiveUI = tempUI;

        // 스왑 직후 로그
        Debug.Log($"플레이어 스왑: {currentPlayer.name}");
    }

    private Vector3 GetPlayerVelocity(GameObject player)
    {
        if (player.TryGetComponent<PlayerController_Monkey>(out var monkeyController)) return monkeyController.CurrentVelocity;
        else if (player.TryGetComponent<PlayerController_Rabbit>(out var rabbitController)) return rabbitController.CurrentVelocity;
        return Vector3.zero;
    }

    IEnumerator TeleportAndApplyVelocity(GameObject player, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        yield return null;
        if (player != null)
        {
            PlayerController_Monkey monkeyController = player.GetComponent<PlayerController_Monkey>();
            PlayerController_Rabbit rabbitController = player.GetComponent<PlayerController_Rabbit>();

            if (monkeyController != null) { monkeyController.TeleportTo(position, rotation); monkeyController.SetVelocity(velocity); }
            else if (rabbitController != null) { rabbitController.TeleportTo(position, rotation); rabbitController.SetVelocity(velocity); }

            UpdateCameraTarget(player.transform);
            StartCoroutine(CheckGroundPenetration(player));
        }
    }

    void UpdateCameraTarget(Transform target)
    {
        if (freeLookCamera == null || target == null) return;
        freeLookCamera.Follow = target;
        freeLookCamera.LookAt = target;
    }

    IEnumerator CheckGroundPenetration(GameObject player)
    {
        if (player == null) yield break;
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller == null) yield break;

        float endTime = Time.time + correctionDuration;
        Transform playerTransform = player.transform;

        while (Time.time < endTime)
        {
            Vector3 rayOrigin = playerTransform.position + controller.center;
            float rayDistance = (controller.height * 0.5f) + 1.0f;
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance, groundLayerMask))
            {
                float groundHeight = hit.point.y;
                float playerFeetY = (playerTransform.position + controller.center).y - (controller.height * 0.5f);

                if (playerFeetY < groundHeight - 0.01f)
                {
                    float desiredFeetY = groundHeight + correctionOffset;
                    float correctionAmount = desiredFeetY - playerFeetY;
                    Vector3 correctedPosition = playerTransform.position + new Vector3(0, correctionAmount, 0);

                    PlayerController_Monkey monkeyController = player.GetComponent<PlayerController_Monkey>();
                    PlayerController_Rabbit rabbitController = player.GetComponent<PlayerController_Rabbit>();

                    if (monkeyController != null) monkeyController.TeleportTo(correctedPosition, playerTransform.rotation);
                    else if (rabbitController != null) rabbitController.TeleportTo(correctedPosition, playerTransform.rotation);

                    yield break;
                }
            }
            yield return null;
        }
    }
}