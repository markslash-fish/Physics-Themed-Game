using System.Runtime.Serialization;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    
    private Animator animator;
    [SerializeField] PlayerInputReader playerInputReader;
    public Player player;
    public bool isIdle;
    public bool isWalking;

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
        isWalking = player.move.magnitude > 0;
        animator.SetBool("isWalking", isWalking);
        
    }

    public void SetLightAttack()
    {
        // This function is intentionally left empty as the attack logic is handled in the PlayerAttack script.
        animator.SetInteger("LightAttack", 1);
        
    }

    void ResetCombo()
    {
        animator.SetInteger("LightAttack", 0);
    }
    
}
    

