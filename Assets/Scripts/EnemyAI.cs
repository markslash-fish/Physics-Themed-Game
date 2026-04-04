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

    public Transform player;
    public GuardianDataManager guardianData;
    EnemyAttackController attackController;
    EnemyStateHandler stateHandler;
   

    [Header("EnemyStats")]
    [SerializeField] private int enemyMaxHealth;
    [SerializeField] private int enemyMaxStamina;
    [SerializeField] private int enemyDefense;
    [SerializeField] private int enemyMinAP;
    [SerializeField] private int enemyMaxAP;
    [SerializeField] private float enemySpeed;
  
    public int enemyDamage;

    private bool isTrackingPlayer;

    public NetworkVariable<int> enemyCurrentHealth = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public NetworkVariable<int> enemyCurrentStamina = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Animator anim;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;


    int strafeDirection = 1;
    public float strafeDistance = 6f;
    [Header("Raycast Settings")]
    public float rayDistance = 0f;
    public Vector3 rayOffset;




    public Vector3 dirToPlayer;


    private void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        attackController = GetComponent<EnemyAttackController>();
        stateHandler = GetComponent<EnemyStateHandler>();

    }
    public override void OnNetworkSpawn()
    {
        enemyMaxHealth = guardianData.enemyHealth;
        enemyMaxStamina = guardianData.enemyStamina;
      
        enemyDefense = guardianData.enemyDefense;
        enemySpeed = guardianData.enemySpeed;
        enemyMinAP = guardianData.enemyMinAttackPower;
        enemyMaxAP = guardianData.enemyMaxAttackPower;
        if (IsServer)
        {
            enemyCurrentHealth.Value = enemyMaxHealth;
            enemyCurrentStamina.Value = enemyMaxStamina;
        }


        InvokeRepeating("EnemyAttack", 3f, 3.1f);
        if (!attackController.isBusy | player != null && stateHandler.enemyState.Value == EnemyStateHandler.EnemyState.IsStrafing)
            InvokeRepeating("SwitchStrafeDirection", 3f, 5f);
    }
    private void OnEnable()
    {
        stateHandler.onIdle += EnemyIdle;
        stateHandler.onDeath += EnemyDeath;
    }
    private void OnDisable()
    {
        stateHandler.onIdle -= EnemyIdle;
        stateHandler.onDeath -= EnemyDeath;
    }
    private void Start()
    {


        navMeshAgent.updateRotation = false;
        navMeshAgent.speed = enemySpeed;
        


       
    }
    void Update()
    {
        if (!IsServer) return;
        if (!attackController.isBusy)
        {
            SetTarget();
        }
        if (player == null) return;

        CalculateDistancefromPlayer();

        dirToPlayer = (player.transform.position - transform.position).normalized;
        if (isTrackingPlayer && player != null)
        {
            ResetEnemyRotation();
        }

    }
    void EnemyIdle()
    {
        if (player != null)
        {
            stateHandler.CurrentState = EnemyStateHandler.EnemyState.Idle;
        }
    }
    void EnemyAttack()
    {
        if (!IsServer) return;
        enemyDamage = Random.Range(enemyMinAP, enemyMaxAP);
        stateHandler.CurrentState = EnemyStateHandler.EnemyState.IsAttacking;

    }
  

    public void EnemyStrafing()
    {
        if (!IsServer) return;
        if (!navMeshAgent.enabled || attackController.isBusy || player == null) return;


        Vector3 sideVector = Vector3.Cross(Vector3.up, dirToPlayer);



        Vector3 orbitPoint = player.transform.position - (dirToPlayer * strafeDistance);
        Vector3 targetpos = orbitPoint + (sideVector * strafeDirection * 5f);



        
        navMeshAgent.SetDestination(targetpos);

        Quaternion lookrotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookrotation, Time.deltaTime * 5f);


    }
   
    public void EnemyDeath()
    {
      if(IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
    void ResetState()
    {
        if (dirToPlayer.magnitude < strafeDistance)
        {
            EnemyStrafing();
        }

    }
    void SetTarget()
    {

        Ray ray = new Ray(transform.position + rayOffset, transform.forward * rayDistance);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                player = hit.transform;
                stateHandler.CurrentState = EnemyStateHandler.EnemyState.IsStrafing;
               
            }
        }
    }
  
    void SwitchStrafeDirection()
    {
        strafeDirection *= -1;

    }
    public void TakeDamage(int damage)
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
        dirToPlayer = (player.transform.position - transform.position).normalized;
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
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + rayOffset, transform.forward * rayDistance);
        //AttackSphere
    }


}