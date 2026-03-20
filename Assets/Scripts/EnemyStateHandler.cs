using UnityEngine;
using UnityEngine.AI;
using static EnemyAI;

public class EnemyStateHandler : MonoBehaviour
{
    public EnemyState enemyState;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyAttackController attackController;
    [SerializeField] private NavMeshAgent navMeshAgent;
    public enum EnemyState
    {
        Idle,
        IsStrafing,
        IsAttacking,
        OnDeath
    }
    void SetEnemyState()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                navMeshAgent.isStopped = true;
                break;
            case EnemyState.IsStrafing:
                enemyAI.EnemyStrafing();
                navMeshAgent.isStopped = false;
                break;
            case EnemyState.IsAttacking:
                enemyAI.EnemyAttack();
                break;
            case EnemyState.OnDeath:
                enemyAI.EnemyDeath();
                break;
        }
    }
    private void Update()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();
        attackController = GetComponent<EnemyAttackController>();
        SetEnemyState();
    }
}
