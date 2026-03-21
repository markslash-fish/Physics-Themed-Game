using System;
using UnityEngine;
using UnityEngine.AI;
using static EnemyAI;

public class EnemyStateHandler : MonoBehaviour
{
    public EnemyState enemyState;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyAttackController attackController;
    [SerializeField] private NavMeshAgent navMeshAgent;


    public static event Action onIdle;
    public static event Action onStrafe;
    public static event Action onAttack;
    public static event Action onExhaust;
    public static event Action onDeath;

    public enum EnemyState
    {
        Idle,
        IsStrafing,
        IsAttacking,
        IsExhausted,
        OnDeath
    }
    void SetEnemyState()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:

                break;
            case EnemyState.IsStrafing:
                enemyAI.EnemyStrafing();
                break;
            case EnemyState.IsAttacking:
             
                break;
            case EnemyState.IsExhausted:

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

        onIdle += OnEnemyIdle;
        onStrafe += OnEnemyStrafe;
        onAttack += OnEnemyAttack;
        onExhaust += OnEnemyExhaust;
        onDeath += OnEnemyDeath;
    }
    void OnEnemyIdle()
    {
        if (enemyState == EnemyState.Idle)
        {
            onIdle?.Invoke();
        }
    }
    void OnEnemyStrafe()
    {
        if (enemyState == EnemyState.IsStrafing)
        {
            onStrafe?.Invoke();
        }
    }
    void OnEnemyAttack()
    {
        if(enemyState == EnemyState.IsAttacking)
        {
            onAttack?.Invoke();
        }
    }
    void OnEnemyExhaust()
    {
        if (enemyState == EnemyState.IsExhausted)
        {
            onExhaust?.Invoke();
        }
    }
    void OnEnemyDeath()
    {
        if (enemyState == EnemyState.OnDeath)
        {
            onDeath?.Invoke();
        }
    }
   
  
}
