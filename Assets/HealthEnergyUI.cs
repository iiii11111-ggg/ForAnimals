using UnityEngine;
using UnityEngine.UI;

public class HealthEnergyUI : MonoBehaviour
{
    public Slider healthSlider;
    public Slider energySlider;

    public float maxHealth = 100f;
    public float maxEnergy = 100f;

    private float currentHealth;
    private float currentEnergy;

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        healthSlider.maxValue = maxHealth;
        energySlider.maxValue = maxEnergy;

        UpdateUI();
    }

    void Update()
    {
        // 예시: 테스트용 키 입력으로 체력/에너지 변화
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            UseEnergy(15f);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RecoverHealth(5f);
            RecoverEnergy(5f);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
    }

    public void UseEnergy(float amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        UpdateUI();
    }

    public void RecoverHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    public void RecoverEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        UpdateUI();
    }

    void UpdateUI()
    {
        healthSlider.value = currentHealth;
        energySlider.value = currentEnergy;
    }
}
