using TMPro;
using UnityEngine;

public class InterUIManager : MonoBehaviour
{
    [Header("UI 요소")]
    public GameObject interactionPromptPanel;
    public TextMeshProUGUI promptText;

    [Header("표시할 메시지")]
    [SerializeField] private string promptMessage = "[F] 상호작용";

    // 씬의 UIManager가 활성화될 때마다 이벤트 구독
    private void OnEnable()
    {
        InteractionEvents.OnShowPromptRequest += ShowPrompt;
        InteractionEvents.OnHidePromptRequest += HidePrompt;
    }

    // 씬이 전환되거나 비활성화될 때 구독 해제
    private void OnDisable()
    {
        InteractionEvents.OnShowPromptRequest -= ShowPrompt;
        InteractionEvents.OnHidePromptRequest -= HidePrompt;
    }

    void Start()
    {
        HidePrompt();
    }

    private void ShowPrompt()
    {
        if (promptText != null)
        {
            promptText.text = promptMessage;
        }
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(false);
        }
    }
}
