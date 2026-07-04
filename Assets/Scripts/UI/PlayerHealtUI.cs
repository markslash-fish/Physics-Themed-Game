using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Player player;
    [Header("Health UI")]
    public Image healthImage;
    public Image damageImage;
    public Image healImage;
    [Header("Stamina UI")]
    public Image staminaImage;

    public float delay = 0.8f;
    public float damageSpeed = 2f;
    public float healSpeed = 2f;
    [Header("Skill Cooldown UI")]
    public TMP_Text cooldownText;
   

    [Header("Potion Count")]
    public TMP_Text potionCounter;


    Coroutine damageRoutine;
    Coroutine healRoutine;

    private void Update()
    {
        potionCounter.SetText(player.currentPotionCount.ToString());

        // 1. Check the actual running timer directly instead of comparing them
        if (player.cooldownTimer > 0f)
        {
            player.cooldownTimer -= Time.deltaTime;

            if (player.cooldownTimer < 0f)
            {
                player.cooldownTimer = 0f;
            }

            // Show the actual numbers ticking down
            cooldownText.SetText(player.cooldownTimer.ToString("0.0"));
        }
        // 2. If the timer is 0, it's ready!
        else
        {
            cooldownText.SetText("Skill Ready");
        }
    }
    public void SetHealth(float current, float max)
    {
        float target = current / max;
        float previous = healthImage.fillAmount;

        // DAMAGE
        if (target < previous)
        {
            healthImage.fillAmount = target;

            // sync heal bar
            healImage.fillAmount = target;

            if (damageRoutine != null)
                StopCoroutine(damageRoutine);

            damageRoutine = StartCoroutine(DamageEffect(target));
        }

        // HEAL
        else if (target > previous)
        {
            if (healRoutine != null)
                StopCoroutine(healRoutine);

            healRoutine = StartCoroutine(HealEffect(target));
        }
    }

    IEnumerator DamageEffect(float target)
    {
        yield return new WaitForSeconds(delay);

        while (damageImage.fillAmount > target)
        {
            damageImage.fillAmount -= Time.deltaTime * damageSpeed;
            yield return null;
        }

        damageImage.fillAmount = target;
    }

    IEnumerator HealEffect(float target)
    {
        healImage.fillAmount = healthImage.fillAmount;

        while (healImage.fillAmount < target)
        {
            healImage.fillAmount += Time.deltaTime * healSpeed;
            yield return null;
        }

        yield return new WaitForSeconds(delay);

        healthImage.fillAmount = target;

        // sync damage
        damageImage.fillAmount = healthImage.fillAmount;
    }

    // STAMINA SYSTEM
    public void SetStamina(float current, float max)
    {
        float percent = current / max;
        staminaImage.fillAmount = percent;
    }
}