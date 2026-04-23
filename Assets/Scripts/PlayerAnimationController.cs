using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimationController : NetworkBehaviour
{  
    [SerializeField] private Animator anim;
   
    [SerializeField] private Player player;
    [SerializeField] private PlayerInputReader inputReader;

    public bool isMoving;
    public GameObject potionObject;

    public GameObject emptyPotionObject;
    public float throwForce = 15f;

    public override void OnNetworkSpawn()
    {
        inputReader.jumpStarted += PlayJump;
        inputReader.onBlockStarted += PlayBlock;
        inputReader.onBlockFinished += StopBlock;
        inputReader.onLightAttackStarted += PlayLightAttack;
        inputReader.onHeavyAttackStarted += PlayHeavyAttack;
        inputReader.onDodgeStarted += PlayDodge;
        inputReader.onHeal += PlayHeal;

    }
    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();
        inputReader = GetComponent<PlayerInputReader>();
      


    }
    private void Update()
    {
        if (!IsOwner) return;

        


   
        PlayMove();
    }
    void PlayMove()
    {

        bool isMoving = player.move.magnitude > 0.1f;
       
        anim.SetBool("isIdle", !isMoving);
        anim.SetBool("isRunning", isMoving);
    }
    void PlayJump()
    {
      
        if(player.playerState == Player.PlayerState.IsJumping)
        {
            if (player.isGrounded)
            {
                anim.ResetTrigger("jumpTrigger");
                anim.SetTrigger("jumpTrigger");
            }
        }
          
        


    }
    void PlayHeal()
    {
        if (player.playerState == Player.PlayerState.IsHealing)
        {
            StartCoroutine(ShowPotionDelay());
            anim.ResetTrigger("Heal");
            anim.SetTrigger("Heal");
        }
        
    }
    void PlayBlock()
    {
        if (player.playerState == Player.PlayerState.IsBlocking)
        {
            anim.SetBool("Block", true);
        }
      

    }
    void StopBlock()
    {
            anim.SetBool("Block", false);
            player.playerState = Player.PlayerState.None;
    }

    void PlayLightAttack()
    {
        if (player.playerState == Player.PlayerState.IsAttacking)
        {
            if (player.comboStep == 1)
            {
               
                anim.SetInteger("LightAttack", 1);
            }
            else if (player.comboStep == 2)
            {

                player.canCombo = false;
                anim.SetInteger("LightAttack", 2);
            }
            else if (player.comboStep == 3)
            {

                player.canCombo = false;
                anim.SetInteger("LightAttack", 3);
            }
        }
      
    }
    void PlayHeavyAttack()
    {
        if (player.playerState == Player.PlayerState.IsAttacking)
        {
            if (player.comboStep != 0) return;
            anim.SetTrigger("HeavyAttack");
        }
       
    }
    void PlayDodge()
    {
        if (player.playerState == Player.PlayerState.IsDodging)
        {
            anim.ResetTrigger("Dodge");
            anim.SetTrigger("Dodge");
        }
       
    }
    IEnumerator ShowPotionDelay()
    {
        yield return new WaitForSeconds(0.5f); // delay bago lumabas
        potionObject.SetActive(true);
        yield return new WaitForSeconds(2.2f);
        potionObject.SetActive(false);
    }
    public void ThrowBottle()
    {
        GameObject bottle = Instantiate(emptyPotionObject, potionObject.transform.position, potionObject.transform.rotation);
        Debug.Log("Bottle spawned: " + bottle.name);
        bottle.SetActive(true);
        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(bottle.transform.forward * throwForce, ForceMode.Impulse);
        }
        StartCoroutine(DestroyEmptyPotion(bottle));
       
    }
    IEnumerator DestroyEmptyPotion(GameObject bottle)
    {
        yield return new WaitForSeconds(12f);
        bottle.SetActive(false);
    }
    
}