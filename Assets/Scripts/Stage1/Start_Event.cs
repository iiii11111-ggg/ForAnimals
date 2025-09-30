using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Start_Event : MonoBehaviour
{
    public GameObject StartEvent,player;
    public CanvasGroup chText;

    void Update()
    {
        if (Dialog.Instance != null)
        {
            if (Dialog.Instance.EndDialog) 
            { 
                player.GetComponentInChildren<PlayerController>().enabled = true;

            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            StartEvent.GetComponentInChildren<Intro_Animation>().enabled = true;

            player.GetComponentInChildren<PlayerController>().enabled = false;

            StartCoroutine(FadeUI());

            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = false;
        }
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
