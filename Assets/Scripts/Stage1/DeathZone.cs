using UnityEngine;
using System.Collections;
using Polyperfect.Common;

public class DeathZone : MonoBehaviour
{
    public GameObject SaveZone;
    public CanvasGroup FadeScreen;
    public SwapManager swap;
    private GameObject Player;
    private Rigidbody playerRb;

    public float waterGravityMultiplier = 0.3f;
    public float waterLinearDamping = 5f;

    private float originalLinearDamping;

    // 물속 진입 상태 및 속성 변경 지연 플래그
    private bool isInWater = false;
    private bool isChangingToWaterPhysics = false;

    // 컨트롤러 컴포넌트 참조
    private PlayerController_Monkey monkeyController;
    private PlayerController_Rabbit rabbitController;

    private Animator An;

    private void FixedUpdate()
    {
        if (isChangingToWaterPhysics && playerRb != null)
        {
            // 다음 물리 프레임에서 속성 변경 실행
            playerRb.linearDamping = waterLinearDamping;
            playerRb.useGravity = false;

            isChangingToWaterPhysics = false; // 플래그 해제
        }

        // 물속 중력 로직은 isInWater 플래그를 사용하여 FixedUpdate에서 실행
        if (isInWater && playerRb != null)
        {
            Vector3 waterGravity = Physics.gravity * waterGravityMultiplier;
            playerRb.AddForce(waterGravity, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            swap.enabled = false;
            Player = other.gameObject;
            playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                originalLinearDamping = playerRb.linearDamping;

                isChangingToWaterPhysics = true;
                isInWater = true;

                monkeyController = Player.GetComponent<PlayerController_Monkey>();
                rabbitController = Player.GetComponent<PlayerController_Rabbit>();

                if (monkeyController != null) monkeyController.enabled = false;
                if (rabbitController != null) rabbitController.enabled = false;
            }

            StartCoroutine(FadeInOut(0.8f, 0.2f));

            if (monkeyController != null)
            {
                An = Player.GetComponent<Animator>();

                An.SetBool("isSwimming",true);

                StartCoroutine(TP_Monkey(An));
            }
            else
            {
                An = Player.GetComponent<Animator>();

                An.SetTrigger("isDead");

                StartCoroutine(TP_Rabbit(An));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerRb != null)
            {
                // 🐵🐰 [추가] 컨트롤러 컴포넌트를 다시 활성화
                if (monkeyController != null) monkeyController.enabled = true;
                if (rabbitController != null) rabbitController.enabled = true;

                // 물리 속성 복원
                playerRb.linearDamping = originalLinearDamping;
                playerRb.useGravity = true;

                // 플래그 초기화
                isInWater = false;
                isChangingToWaterPhysics = false;

                playerRb = null;
                monkeyController = null;
                rabbitController = null;
            }
        }
    }

    // TP 코루틴 내에서 컨트롤러의 TeleportTo 메서드를 호출하는 것은 그대로 유지
    IEnumerator TP_Monkey(Animator An)
    {
        yield return new WaitForSeconds(0.8f);

        An.SetBool("isSwimming", false);
        An.Play("Moving", 0, 0f);

        PlayerController_Monkey PM = Player.GetComponent<PlayerController_Monkey>();
        if (PM != null) PM.TeleportTo(SaveZone.transform.position, Quaternion.Euler(0f, -160f, 0f));
        swap.enabled = true;
    }

    IEnumerator TP_Rabbit(Animator An)
    {
        yield return new WaitForSeconds(0.8f);

        An.Play("Rabbit_Idle", 0, 0f);

        PlayerController_Rabbit RM = Player.GetComponent<PlayerController_Rabbit>();
        if (RM != null) RM.TeleportTo(SaveZone.transform.position, Quaternion.Euler(0f, -160f, 0f));
        swap.enabled = true;
    }

    // (Fade 코루틴은 동일)
    IEnumerator FadeInOut(float fadeDuration, float holdDuration)
    {
        CanvasGroup fadeScreen = FadeScreen;
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        FadeScreen.alpha = startAlpha;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            FadeScreen.alpha = newAlpha;
            yield return null;
        }
        FadeScreen.alpha = endAlpha;
    }
}