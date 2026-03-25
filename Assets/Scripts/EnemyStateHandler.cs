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


    public event Action onIdle;
    public event Action onStrafe;
    public event Action onAttack;
    public event Action onExhaust;
    public event Action onDeath;

    public enum EnemyState
    {
        Idle,
        IsStrafing,
        IsAttacking,
        IsExhausted,
        OnDeath
    }
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();
        attackController = GetComponent<EnemyAttackController>();
    }
    private void OnEnable()
    {
        onIdle += OnEnemyIdle;
        onStrafe += OnEnemyStrafe;
        onAttack += OnEnemyAttack;
        onExhaust += OnEnemyExhaust;
        onDeath += OnEnemyDeath;
    }
    private void OnDisable()
    {
        onIdle -= OnEnemyIdle;
        onStrafe -= OnEnemyStrafe;
        onAttack -= OnEnemyAttack;
        onExhaust -= OnEnemyExhaust;
        onDeath -= OnEnemyDeath;
    }

    private void Update()
    {
       
        SetEnemyState();

       
    }
    void SetEnemyState()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:

                break;
            case EnemyState.IsStrafing:       
                if(!navMeshAgent.updatePosition)
                {
                    navMeshAgent.nextPosition = transform.position;
                    navMeshAgent.isStopped = false;
                    navMeshAgent.updatePosition = true;
                }

                    enemyAI.EnemyStrafing();
               
                break;
            case EnemyState.IsAttacking:
                navMeshAgent.isStopped = true;
                navMeshAgent.updatePosition = false;

                break;
            case EnemyState.IsExhausted:

                break;
            case EnemyState.OnDeath:
                enemyAI.EnemyDeath();
                break;
        }
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
