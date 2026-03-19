using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    
private Animator anim;
    public float cooldownTime = 2f;
    private float nextFireTime = 0f;
    public static int noOfClicks = 0;
    float lastClickedTime = 0;
    float maxComboDelay = 1;
 
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
 
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            anim.SetInteger("LightAttack", 1);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            anim.SetInteger("LightAttack", 2);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
        {
            anim.SetInteger("LightAttack", 3);
            noOfClicks = 0;
        }
 
 
        if (Time.time - lastClickedTime > maxComboDelay)
        {
            noOfClicks = 0;
        }
 
        //cooldown time
        if (Time.time > nextFireTime)
        {
            // Check for mouse input
            if (Input.GetMouseButtonDown(0))
            {
                OnClick();
 
            }
        }
    }
 
    void OnClick()
    {
        //so it looks at how many clicks have been made and if one animation has finished playing starts another one.
        lastClickedTime = Time.time;
        noOfClicks++;
        if (noOfClicks == 1)
        {
            anim.SetInteger("LightAttack", 1);
        }
        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);
 
        if (noOfClicks >= 2 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            anim.SetInteger("LightAttack", 0);
            anim.SetInteger("LightAttack", 2);
        }
        if (noOfClicks >= 3 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            anim.SetInteger("LightAttack", 2);
            anim.SetInteger("LightAttack", 3);
        }
    }
}

