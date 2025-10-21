using System.Collections;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;

public class Start_Event : MonoBehaviour, IEventController
{
    public GameObject StartEvent,player;
    public CanvasGroup chText;

    private bool hasTriggered = false;

    [Header("Save System ID")]
    [SerializeField] private string uniqueID; 

    [Header("Custom Start Actions")]
    [SerializeField] private UnityEvent onEventStart; 

    [SerializeField] private UnityEvent onEventEnd;

    [Header("System Interface")]
    public string UniqueID => uniqueID;
    public UnityEvent OnEventStart => onEventStart;

    public UnityEvent OnEventEnd => onEventEnd;

    void Awake()
    {
        int currentSlotIndex = PlayerData.currentSlotIndex;
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError("StartEvent의 uniqueID가 설정되지 않았습니다!", gameObject);
            return;
        }

        if (SaveManager.Instance.HasBeenDestroyed(currentSlotIndex, uniqueID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        EventManager.Instance.RequestEventStart(this);
        if (other.CompareTag("Player")) 
        {
            StartEvent.GetComponentInChildren<Intro_Animation>().enabled = true;

            player.GetComponentInChildren<PlayerController_Rabbit>().enabled = false;

            StartCoroutine(FadeUI());
        }
    }
    public void ScriptStart()
    {
        StartEvent.GetComponentInChildren<Intro_Animation>().enabled = true;

        player.GetComponentInChildren<PlayerController_Rabbit>().enabled = false;

        StartCoroutine(FadeUI());

        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = false;
    }
    public void ScriptEnd()
    {
    }



    IEnumerator FadeUI() 
    {
        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(Fade(0f, 1f, 1.0f));

        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(Fade(1f, 0f, 1.0f));

        chText.alpha = 0f;
        chText.interactable = false;
        chText.blocksRaycasts = false;
    }
    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            chText.alpha = newAlpha;
            yield return null;
            chText.alpha = endAlpha;


        }
    }
}
