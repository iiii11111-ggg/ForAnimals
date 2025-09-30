using UnityEngine;
using System.Collections;
using TMPro;

public class GlowFlicker : MonoBehaviour
{
    public TextMeshProUGUI pressAny;
    private Material flikText;
    void Awake()
    {
        pressAny = GetComponent<TextMeshProUGUI>();
        flikText = pressAny.fontSharedMaterial;
        StartCoroutine(FlickerGlow());
    }
    void OnEnable()
    {
        StartCoroutine(FlickerGlow());
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnDisable()
    {
        StopAllCoroutines();
        flikText.SetFloat(ShaderUtilities.ID_GlowPower, 0);
    }

    IEnumerator FlickerGlow()
    {
        while (true)
        {
            flikText.SetFloat(ShaderUtilities.ID_GlowPower, 0.7f);
            yield return new WaitForSeconds(0.5f); // 설정된 시간만큼 기다림

            // 2. 글로우 비활성화 (끄기)
            flikText.SetFloat(ShaderUtilities.ID_GlowPower, 0);
            yield return new WaitForSeconds(0.5f); // 설정된 시간만큼 기다림
        }
    }
}
