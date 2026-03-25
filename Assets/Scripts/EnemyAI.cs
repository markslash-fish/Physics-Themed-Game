using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour, IDamageable
{

    public Transform player;
    public GuardianDataManager guardianData;
    EnemyAttackController attackController;
    EnemyStateHandler stateHandler;
   

    [Header("EnemyStats")]
    [SerializeField] private float enemyCurrentHealth;
    [SerializeField] private int enemyMaxHealth;
    [SerializeField] private int enemyCurrentStamina;
    [SerializeField] private int enemyMaxStamina;
    [SerializeField] private int enemyDefense;
    [SerializeField] private int enemyMinAP;
    [SerializeField] private int enemyMaxAP;
    [SerializeField] private float enemySpeed;

    private float enemyDamage;

    private bool isTrackingPlayer;



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
      

        enemyMaxHealth = guardianData.enemyHealth;
        enemyMaxStamina = guardianData.enemyStamina;
        enemyCurrentHealth = enemyMaxHealth;
        enemyCurrentStamina = enemyMaxStamina;
        enemyDefense = guardianData.enemyDefense;
        enemySpeed = guardianData.enemySpeed;
        enemyMinAP = guardianData.enemyMinAttackPower;
        enemyMaxAP = guardianData.enemyMaxAttackPower;
      


    }
    private void OnEnable()
    {
       
        stateHandler.onStrafe += EnemyStrafing;
        stateHandler.onDeath += EnemyDeath;
    }
    private void OnDisable()
    {
        stateHandler.onStrafe -= EnemyStrafing;
        stateHandler.onDeath -= EnemyDeath;
    }
    private void Start()
    {
        EnemyIdle();

        navMeshAgent.updateRotation = false;
        navMeshAgent.speed = enemySpeed;
        


        if (!attackController.isBusy | player != null && stateHandler.enemyState == EnemyStateHandler.EnemyState.IsStrafing)
            InvokeRepeating("SwitchStrafeDirection", 3f, 5f);
    }
    void Update()
    {
        

        enemyDamage = Random.Range(enemyMinAP, enemyMaxAP);


        if (!attackController.isBusy)
        {
            SetTarget();
        }

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
            stateHandler.enemyState = EnemyStateHandler.EnemyState.Idle;
        }
    }
  

    public void EnemyStrafing()
    {
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
        gameObject.SetActive(false);
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
                stateHandler.enemyState = EnemyStateHandler.EnemyState.IsStrafing;
               
            }
        }
    }
    private void OnDrawGizmos()
    {
        // Ray1
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + rayOffset, transform.forward * rayDistance);

    }
    void SwitchStrafeDirection()
    {
        strafeDirection *= -1;

    }
    public void TakeDamage(int damage)
    {
        int healthDamage = damage ^ 2 / damage + enemyDefense;
        int staminaDamage = damage/2;
        enemyCurrentHealth -= healthDamage;
        enemyCurrentStamina -= staminaDamage;
        if (enemyCurrentHealth <= 0) stateHandler.enemyState = EnemyStateHandler.EnemyState.OnDeath;
        if (enemyCurrentStamina <= 0) stateHandler.enemyState = EnemyStateHandler.EnemyState.IsExhausted;
    }
    void CalculateDistancefromPlayer()
    {
        dirToPlayer = (player.transform.position - transform.position).normalized;
    }
    public void ResetEnemyState()
    {
        stateHandler.enemyState = EnemyStateHandler.EnemyState.IsStrafing;
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


}