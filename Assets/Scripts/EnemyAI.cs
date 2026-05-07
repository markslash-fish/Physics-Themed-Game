using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : NetworkBehaviour, IDamageable
{

    public Transform targetPlayer = null;
    public GuardianDataManager guardianData;
    EnemyAttackController attackController;
    EnemyStateHandler stateHandler;
    EnemyAnimationController animationController;
   

    [Header("EnemyStats")]
    [SerializeField] private int enemyMaxHealth;
    [SerializeField] private int enemyMaxStamina;
    [SerializeField] private int enemyDefense;
    [SerializeField] private int enemyMinAP;
    [SerializeField] private int enemyMaxAP;
    [SerializeField] private float enemySpeed;
    [SerializeField] private float attackTimer;
    [SerializeField] private float attackCooldown;

    [SerializeField] private bool enemyAttackStarted;
  
    public int enemyDamage;

    private bool isTrackingPlayer;

    public NetworkVariable<int> enemyCurrentHealth = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public NetworkVariable<int> enemyCurrentStamina = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Animator anim;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;


    public int strafeDirection = 1;
    public float strafeDistance = 6f;
    [Header("Raycast Settings")]
    public float sphereRadius = 0f;
    public Vector3 sphereOffset;

    public LayerMask playerLayer;


    public Vector3 dirToPlayer;


    private void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        attackController = GetComponent<EnemyAttackController>();
        stateHandler = GetComponent<EnemyStateHandler>();
        animationController = GetComponent<EnemyAnimationController>();
    }
    public override void OnNetworkSpawn()
    {
        enemyMaxHealth = guardianData.enemyHealth;
        enemyMaxStamina = guardianData.enemyStamina;
        enemyCurrentHealth.Value = enemyMaxHealth;
        enemyCurrentStamina.Value = enemyMaxStamina;
        enemyDefense = guardianData.enemyDefense;
        enemySpeed = guardianData.enemySpeed;
        enemyMinAP = guardianData.enemyMinAttackPower;
        enemyMaxAP = guardianData.enemyMaxAttackPower;
        navMeshAgent.speed = enemySpeed;

        stateHandler.enemyState.OnValueChanged += OnStateChanged;
        OnStateChanged(stateHandler.enemyState.Value, stateHandler.enemyState.Value);

        InvokeRepeating("SwitchStrafeDirection", 5.2f, 7.7f);


    }
    private void Start()
    {
        navMeshAgent.updateRotation = false;
        
    }
    public override void OnNetworkDespawn()
    {
        stateHandler.enemyState.OnValueChanged -= OnStateChanged;
    }
    void Update()
    {
        if (!IsServer) return;

     
        if (targetPlayer == null) return;

        CalculateDistancefromPlayer();

        dirToPlayer = (targetPlayer.transform.position - transform.position).normalized;
        if (isTrackingPlayer && targetPlayer != null)
        {
            ResetEnemyRotation();
        }

    }
    private void OnStateChanged(EnemyStateHandler.EnemyState oldState, EnemyStateHandler.EnemyState newState)
    {
        if (!IsServer) return;
        if (newState == EnemyStateHandler.EnemyState.OnDeath)
        {
            GetComponent<NetworkObject>().Despawn();
        }
        else if (newState == EnemyStateHandler.EnemyState.IsAttacking)
        {
            SetTarget();
            enemyDamage = Random.Range(enemyMinAP, enemyMaxAP);
            navMeshAgent.isStopped = true;
            navMeshAgent.updatePosition = false;
            attackController.ChooseAction();
     
        }
        else if(newState == EnemyStateHandler.EnemyState.IsStrafing)
        {
            StartCoroutine(AttackRoutine());
            navMeshAgent.isStopped = false;
            navMeshAgent.nextPosition = transform.position;
            navMeshAgent.updatePosition = true;
          
            
        }
        else if(newState == EnemyStateHandler.EnemyState.Idle)
        {
            animationController.PlayIdle();
            navMeshAgent.isStopped = true;
            navMeshAgent.updatePosition = false;
        }
    }
    public void EnemyStrafing()
    {
        if (!IsServer) return;

       
        Vector3 sideVector = Vector3.Cross(Vector3.up, dirToPlayer);

        Vector3 orbitPoint = targetPlayer.transform.position - (dirToPlayer * strafeDistance);
        Vector3 targetpos = orbitPoint + (sideVector * strafeDirection * 5f);
  
        navMeshAgent.SetDestination(targetpos);

        Quaternion lookrotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookrotation, Time.deltaTime * 5f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetTarget();
            stateHandler.CurrentState = EnemyStateHandler.EnemyState.IsStrafing;
            
        }
    }
    void SetTarget()
    {
        float closestPlayer = Mathf.Infinity;

        Collider[] playersHit = Physics.OverlapSphere(transform.position, sphereRadius, playerLayer);

        foreach (Collider  player in playersHit)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position);

            float distance = directionToPlayer.sqrMagnitude;
            if (distance < closestPlayer)
            {
                closestPlayer = distance;
                targetPlayer = player.transform;
            }
        }
    }
  
    void SwitchStrafeDirection()
    {
        strafeDirection *= -1;

    }
    public void TakeDamage(int damage, Vector3 hitDir)
    {
        if (!IsServer) return;
        int healthDamage = (damage * damage) / damage + enemyDefense;
        int staminaDamage = damage/2;
        enemyCurrentHealth.Value-= healthDamage;
        enemyCurrentStamina.Value -= staminaDamage;
        if (enemyCurrentHealth.Value <= 0) stateHandler.CurrentState = EnemyStateHandler.EnemyState.OnDeath;
        if (enemyCurrentStamina.Value <= 0) stateHandler.CurrentState = EnemyStateHandler.EnemyState.IsExhausted;
    }
    void CalculateDistancefromPlayer()
    {
        dirToPlayer = (targetPlayer.transform.position - transform.position).normalized;
    }
    public void ResetEnemyState()
    {
        stateHandler.enemyState.Value = EnemyStateHandler.EnemyState.IsStrafing;
        attackController.isBusy = false;
    }
    private void SetTracking(int state)
    {
        isTrackingPlayer = (state == 1);
    }
    void ResetEnemyRotation() 
    {
        dirToPlayer.y = 0; 
        Quaternion enemyLookrotation = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, enemyLookrotation, Time.deltaTime * 5f);
    }
  
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + sphereOffset, sphereRadius);
      
    }
    private IEnumerator AttackRoutine()
    {
        while(attackController.isBusy)
        {
            yield return null;
        }
        enemyAttackStarted = true;
        float randomInterval = Random.Range(3f, 4f);
        yield return new WaitForSeconds(randomInterval);
        stateHandler.CurrentState = EnemyStateHandler.EnemyState.IsAttacking;
        enemyAttackStarted = false;
    }

   
}