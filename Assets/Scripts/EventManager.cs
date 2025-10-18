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
        if (currentActiveEvent != null)
        {
            Debug.LogWarning($"[EventManager] '{currentActiveEvent.UniqueID}' 이벤트가 진행 중이라 '{eventController.UniqueID}' 요청을 무시합니다.");
            return;
        }

        // 요청을 승인하고, 현재 진행 중인 이벤트로 등록
        currentActiveEvent = eventController;
        Debug.Log($"<color=lime><b>[EventManager] '{currentActiveEvent.UniqueID}' 이벤트 시작을 승인합니다.</b></color>");

        // ✅ 매니저의 유일한 임무: 이벤트에게 "시작하라"는 신호만 보낸다.
        //    이벤트가 구체적으로 어떤 동작을 할지는 전혀 알지 못한다.
        currentActiveEvent.OnEventStart.Invoke();
    }

    /// <summary>
    /// 대화가 끝났을 때 호출되는 공통 핸들러.
    /// </summary>
    private void OnDialogEndedHandler()
    {
        if (currentActiveEvent == null) return;

        Debug.Log($"<color=orange><b>[EventManager] '{currentActiveEvent.UniqueID}' 이벤트의 공통 종료 절차를 시작합니다.</b></color>");

        currentActiveEvent.OnEventEnd.Invoke();

        // --- 공통 종료 로직 ---
        // 1. 이벤트 완료 상태 저장
        int currentSlotIndex = PlayerData.currentSlotIndex;
        SaveManager.Instance.MarkAsDestroyed(currentSlotIndex, currentActiveEvent.UniqueID);
        SaveManager.Instance.SaveGameData();

        // 2. 이벤트 오브젝트 트리거 비활성화
        Collider collider = currentActiveEvent.gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            // 3. isTrigger 속성을 false로 설정하여 물리적 충돌체로 만듭니다.
            collider.isTrigger = false;

            Debug.Log(currentActiveEvent.gameObject.name + "의 isTrigger가 비활성화되었습니다.");
        }
        else
        {
            Debug.LogError(currentActiveEvent.gameObject.name + "에서 Collider를 찾을 수 없습니다!");
        }

        // 3. 현재 이벤트 상태를 초기화하여 다음 이벤트를 받을 준비
        currentActiveEvent = null;
    }
}