using System.Runtime.Serialization;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    
    private Animator animator;
    [SerializeField] PlayerInputReader playerInputReader;
    public Player player;
    public bool isIdle;
    public bool isWalking;
    public float comboStep;
    public bool isAttacking;

   void OnEnable()
    {
        
       playerInputReader.onLightAttackStarted += SetLightAttack;
        
    }

    void OnDisable()
    {
        
        
        
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();

    }
    
    void Update()
    {
        SetIdle();
        SetWalking();

        
    }

    public void SetIdle()
    {
         isIdle = player.move.magnitude == 0; 
        animator.SetBool("isIdle", isIdle);
        
    }

    
public void SetWalking()
{
    float speed = player.move.magnitude;

    bool isWalking = speed > 0 && !player.isRunning;
    bool isRunning = speed > 0 && player.isRunning;

    animator.SetBool("isWalking", isWalking);
    animator.SetBool("isRunning", isRunning);
}

    public void SetLightAttack()
    {
          
    
         if (isAttacking == false)
          comboStep++;
    
     
            if (comboStep == 1)
        {

            animator.SetInteger("LightAttack", 1);
            isAttacking = true;
        }
        else if (comboStep == 2)
        {
            animator.SetInteger("LightAttack", 2);
            isAttacking = true;
        }
        else if (comboStep == 3)
        {
            animator.SetInteger("LightAttack", 3);
            isAttacking = true;
        }
              
    }

    void ResetCombo()
    {
        comboStep = 0;
        animator.SetInteger("LightAttack", 0);
        
    }
    
    void CanAttack()
    {
        isAttacking = false;
    }

    
}
    

