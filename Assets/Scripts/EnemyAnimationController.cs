using Unity.Netcode;
using UnityEngine;

public class EnemyAnimationController : NetworkBehaviour
{
    Animator animator;
    EnemyStateHandler stateHandler;
    EnemyAI enemyAI;


    private bool isStrafingRight;
    private bool isStrafing;
    private bool isIdle;
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
        isStrafing = stateHandler.enemyState.Value == EnemyStateHandler.EnemyState.IsStrafing;
        animator.SetBool("isStrafing", isStrafing);
    }
    public void PlayIdle()
    {
        isIdle = stateHandler.enemyState.Value == EnemyStateHandler.EnemyState.Idle;
        animator.SetBool("isIdle", isIdle);
    }
    public void MirrorStrafe()
    {
        isStrafingRight = enemyAI.strafeDirection == -1;
        animator.SetBool("isStrafingRight", isStrafingRight);
     
    }
}
