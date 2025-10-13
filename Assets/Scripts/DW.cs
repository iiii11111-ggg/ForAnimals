using UnityEngine;
using System;

public class DW : MonoBehaviour
{
    void OnDestroy()
    {
        // 이 로그는 일반 로그보다 훨씬 더 강력해서 거의 사라지지 않습니다.
        Debug.LogAssertion($"!!!!!! '{this.gameObject.name}' 오브젝트가 파괴되었습니다! 아래 호출 스택을 확인하세요!");

        // Environment.StackTrace는 이 함수를 호출한 모든 함수들의 목록(호출 스택)을 보여줍니다.
        // 이것이 바로 범인을 가리키는 결정적인 증거입니다.
        Debug.Log(Environment.StackTrace);
    }
    void Update()
    {
        if (Dialog.Instance != null)
        {
            // 매 프레임 현재 공식 인스턴스의 ID를 출력합니다.
            Debug.Log($"[감시 카메라] 현재 Dialog.Instance ID: {Dialog.Instance.GetInstanceID()}");
        }
        else
        {
            // 인스턴스가 null이 되는 순간을 포착합니다.
            Debug.LogWarning("[감시 카메라] Dialog.Instance가 현재 NULL 입니다!");
        }
    }
}
