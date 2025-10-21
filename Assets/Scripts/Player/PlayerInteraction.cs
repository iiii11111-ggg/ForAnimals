// PlayerInteraction.cs
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;
    private bool isInteracting = false;

    void Update()
    {
        // 상호작용 가능하고, F키를 누르고, 현재 상호작용 중이 아닐 때
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.F) && !isInteracting)
        {
            isInteracting = true;
            InteractionEvents.RequestHidePrompt();
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isInteracting) return; // 상호작용 중에는 새 대상을 감지하지 않음

        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            InteractionEvents.RequestShowPrompt(); // UI 표시 신호만 보냄
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null && interactable == currentInteractable)
        {
            InteractionEvents.RequestHidePrompt();
            currentInteractable = null;
        }
    }

    // 사용자의 대화 시스템이 종료될 때 이 함수를 호출해줘야 합니다.
    public void OnInteractionEnd()
    {
        isInteracting = false;
        CheckIfStillInTrigger();
    }

    // 상호작용 종료 후, 아직 플레이어가 범위 안에 있는지 체크
    private void CheckIfStillInTrigger()
    {
        if (currentInteractable != null)
        {
            // 아직 범위 안에 있다면 UI 다시 표시
            InteractionEvents.RequestShowPrompt();
        }
    }
}