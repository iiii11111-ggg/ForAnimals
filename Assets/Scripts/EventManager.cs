using UnityEngine;

/// <summary>
/// 모든 IEventController 이벤트를 총괄하는 싱글톤 매니저.
/// 이벤트의 시작/종료 흐름만 제어하고, 세부 동작은 각 이벤트에 위임합니다.
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private IEventController currentActiveEvent;

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Dialog 시스템의 이벤트는 EventManager가 유일하게 구독합니다.
        if (Dialog.Instance != null)
        {
            Dialog.Instance.OnDialogEnded += OnDialogEndedHandler;
        }
    }

    void OnDisable()
    {
        if (Dialog.Instance != null)
        {
            Dialog.Instance.OnDialogEnded -= OnDialogEndedHandler;
        }
    }

    /// <summary>
    /// 이벤트 스크립트가 이벤트 시작을 요청하는 함수.
    /// </summary>
    /// <param name="eventController">시작을 요청하는 이벤트 스크립트 (IEventController를 구현한)</param>
    public void RequestEventStart(IEventController eventController)
    {
        // 1. 이미 진행중인 이벤트가 있는지 확인 (인터페이스 파괴 체크 포함)
        if (currentActiveEvent != null && (currentActiveEvent as MonoBehaviour) != null)
        {
            Debug.LogWarning($"[EventManager] '{currentActiveEvent.UniqueID}' 이벤트가 진행 중이라 '{eventController.UniqueID}' 요청을 무시합니다.");
            return;
        }

        // 요청을 승인하고, 현재 진행 중인 이벤트로 등록
        currentActiveEvent = eventController;
        Debug.Log($"<color=lime><b>[EventManager] '{currentActiveEvent.UniqueID}' 이벤트 시작을 승인합니다.</b></color>");

        // ✅ 매니저의 유일한 임무: 이벤트에게 "시작하라"는 신호만 보낸다.
        currentActiveEvent.OnEventStart.Invoke();
    }

    /// <summary>
    /// 대화가 끝났을 때 호출되는 공통 핸들러.
    /// </summary>
    private void OnDialogEndedHandler()
    {
        // ⚠️ 핵심 수정: 인터페이스가 가리키는 오브젝트가 실제로 파괴되었는지 확인
        // (currentActiveEvent as MonoBehaviour) == null 체크가 필수입니다.
        if (currentActiveEvent == null || (currentActiveEvent as MonoBehaviour) == null)
        {
            // 이미 파괴되었거나 이벤트가 없으면 종료
            currentActiveEvent = null;
            return;
        }

        Debug.Log($"<color=orange><b>[EventManager] '{currentActiveEvent.UniqueID}' 이벤트의 공통 종료 절차를 시작합니다.</b></color>");

        // 1. 이벤트 종료 콜백 실행 (여기서 Destroy가 일어날 수도 있음)
        currentActiveEvent.OnEventEnd.Invoke();

        // 2. 종료 콜백 직후에 객체가 파괴되었을 수도 있으니 한 번 더 체크
        if ((currentActiveEvent as MonoBehaviour) == null)
        {
            currentActiveEvent = null;
            return;
        }

        // 3. 저장 관련 로직
        int currentSlotIndex = PlayerData.currentSlotIndex;
        // (int는 null이 될 수 없으므로 0인지 체크하는 것이 맞습니다)
        if (currentSlotIndex == 0)
        {
            Debug.LogError("[EventManager] 유효한 슬롯 인덱스가 없어 이벤트 완료 기록을 건너뜁니다!");
        }
        else
        {
            // 필요한 저장 로직...
        }

        // 4. 이벤트 오브젝트 콜라이더(트리거) 비활성화
        // 여기서 .gameObject에 접근할 때 터지던 문제 해결됨
        DisableEventCollider(currentActiveEvent.gameObject);

        // 5. 현재 이벤트 상태를 초기화하여 다음 이벤트를 받을 준비
        currentActiveEvent = null;
    }

    private void DisableEventCollider(GameObject eventObject)
    {
        Debug.Log($"[EventManager] EventCollider 종료 함수 호출");

        // 여기도 안전장치
        if (eventObject == null) return;

        Collider collider = eventObject.GetComponent<Collider>();
        if (collider != null)
        {
            // isTrigger 속성을 false로 설정하거나 enabled를 끄기
            collider.enabled = false;
            Debug.Log($"[EventManager] {eventObject.name}의 Collider 비활성화되었습니다.");
        }
        else
        {
            // 에러 대신 경고로 낮춤 (Destroy된 경우 등 대비)
            Debug.LogWarning($"[EventManager] Collider를 찾을 수 없습니다.");
        }
    }

    public void OnDialogQuit()
    {
        currentActiveEvent = null;
    }
}