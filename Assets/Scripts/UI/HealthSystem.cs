using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    public Animator animator;

    [Header("Potion Throw")]
    public GameObject thrownPotionPrefab;
    public Transform throwPoint;
    public float throwForce = 15f;

    [Header("Potion")]
    public GameObject potionObject;

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
        
        potionObject.SetActive(false);

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
        // TEST BASIC HIT
if (Keyboard.current.uKey.wasPressedThisFrame)
{
    animator.SetTrigger("Hit");
}

// TEST HEAVY HIT
if (Keyboard.current.iKey.wasPressedThisFrame)
{
    animator.SetTrigger("HeavyHit");
}

        // TEST HEAL
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            StartCoroutine(ShowPotionDelay());
            animator.SetTrigger("Heal");
        }

        // TEST STAMINA USE
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            UseStamina(20);
        }

        RegenerateStamina();
        
        if (Keyboard.current.tKey.wasPressedThisFrame)
        
        {
            Debug.Log("T key pressed");
            ThrowBottle();
        }
    }
    public void HealPlayer()
{
     Heal(15);
     StartCoroutine(HidePotionDelay());
     
}
IEnumerator HidePotionDelay()
{
    yield return new WaitForSeconds(1.2f); // delay seconds
    potionObject.SetActive(false);
}
IEnumerator ShowPotionDelay()
{
    yield return new WaitForSeconds(0.5f); // delay bago lumabas
    potionObject.SetActive(true);
}
public void ThrowBottle()
{
    Debug.Log("ThrowBottle called");

    if (thrownPotionPrefab == null)
    {
        Debug.LogError("ThrownPotionPrefab is NOT assigned!");
        return;
    }

    if (throwPoint == null)
    {
        Debug.LogError("ThrowPoint is NOT assigned!");
        return;
    }

    GameObject bottle = Instantiate(thrownPotionPrefab, throwPoint.position, throwPoint.rotation);
    Debug.Log("Bottle spawned: " + bottle.name);

    Rigidbody rb = bottle.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
    }
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