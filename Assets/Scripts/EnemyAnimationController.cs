using Unity.Netcode;
using UnityEngine;

public class EnemyAnimationController : NetworkBehaviour
{
    Animator animator;
    EnemyStateHandler stateHandler;
    EnemyAI enemyAI;

    void Start()
    {
        animator = GetComponent<Animator>();
        stateHandler = GetComponent<EnemyStateHandler>();
        enemyAI = GetComponent<EnemyAI>();
    }

   public override void OnNetworkSpawn()
    {

    }
    void Update()
    {
    }
    public void PlayStrafe()
    {
       bool isStrafing = stateHandler.enemyState.Value == EnemyStateHandler.EnemyState.IsStrafing;
        animator.SetBool("isStrafing", isStrafing);
    }
    public void PlayIdle()
    {
       bool isIdle = stateHandler.enemyState.Value == EnemyStateHandler.EnemyState.Idle;
        animator.SetBool("isIdle", isIdle);
    }
    public void MirrorStrafe()
    {
      bool  isStrafingRight = enemyAI.strafeDirection == -1;
        animator.SetBool("isStrafingRight", isStrafingRight);
     
    }
    public void PlayExhaust()
    {
    animator.SetBool("isExhausted", true);     
    }
    public void RecoverExhaust()
    {
        animator.SetBool("isExhausted", false);
    }
}
