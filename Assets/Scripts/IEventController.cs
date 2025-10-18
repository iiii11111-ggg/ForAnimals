using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// EventManager와 소통하는 모든 이벤트 스CRIPT가 반드시 구현해야 하는 인터페이스(계약서)입니다.
/// </summary>
public interface IEventController
{
    /// <summary>
    /// 이벤트를 식별하기 위한 고유 ID
    /// </summary>
    string UniqueID { get; }

    /// <summary>
    /// 이벤트가 시작될 때 EventManager가 호출해 줄 UnityEvent
    /// </summary>
    UnityEvent OnEventStart { get; }

    UnityEvent OnEventEnd { get; }

    /// <summary>
    /// 이벤트가 끝났을 때 비활성화할 게임 오브젝트
    /// </summary>
    GameObject gameObject { get; }
}