using UnityEngine;
using UnityEngine.InputSystem;

public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    private float currentStamina;
    public float staminaRegenSpeed = 25f;

    [Header("UI")]
    public BossHPBarUI bossUI;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        if (bossUI != null)
        {
            bossUI.SetHealth(currentHealth, maxHealth);
            bossUI.SetStamina(currentStamina, maxStamina);
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // TEST DAMAGE
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }

        // TEST HEAL
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            Heal(15);
        }

        // TEST STAMINA USE
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            UseStamina(20);
        }

        RegenerateStamina();
    }

    // HEALTH FUNCTIONS
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (bossUI != null)
            bossUI.SetHealth(currentHealth, maxHealth);
    }

    // STAMINA FUNCTIONS
    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina -= amount;

        UpdateStaminaUI();
        return true;
    }

    void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenSpeed * Time.deltaTime;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            UpdateStaminaUI();
        }
    }

    void UpdateStaminaUI()
    {
        if (bossUI != null)
            bossUI.SetStamina(currentStamina, maxStamina);
    }
}