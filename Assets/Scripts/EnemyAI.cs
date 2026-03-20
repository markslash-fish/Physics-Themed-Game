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
    PlayerMovementScript playerMovementScript;


    [SerializeField] private float enemyCurrentHealth;
    [SerializeField] private int enemyMaxHealth;
    [SerializeField] private int enemyDefense;
    [SerializeField] private int enemyMinAP;
    [SerializeField] private int enemyMaxAP;
    [SerializeField] private float enemySpeed;
    private float enemyDamage;

   
  

    private Animator anim;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    int strafeDirection = 1;
    [SerializeField] private float strafeDistance = 6f;



    public Vector3 dirToPlayer;


    private void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        attackController = GetComponent<EnemyAttackController>();
        stateHandler = GetComponent<EnemyStateHandler>();

        enemyMaxHealth = guardianData.enemyHealth;
        enemyCurrentHealth = enemyMaxHealth;
        enemyDefense = guardianData.enemyDefense;
        enemySpeed = guardianData.enemySpeed;
        enemyMinAP = guardianData.enemyMinAttackPower;
        enemyMaxAP = guardianData.enemyMaxAttackPower;


    }
    private void OnEnable()
    {
     

    }
    private void Start()
    {



        navMeshAgent.speed = enemySpeed;
        navMeshAgent.acceleration = enemySpeed * 2f;



        InvokeRepeating("SwitchStrafeDirection", 2f, 5f);

    }
    void Update()
    {


        enemyDamage = Random.Range(enemyMinAP, enemyMaxAP);

       


            CalculateDistancefromPlayer();
        EnemyStrafing();

    }


    public void EnemyStrafing()
    {
        if (player == null) return;


        dirToPlayer = (player.position - transform.position).normalized;
        Vector3 sideVector = Vector3.Cross(Vector3.up, dirToPlayer);
       


        Vector3 orbitPoint = player.position - (dirToPlayer * strafeDistance);
        Vector3 targetpos = orbitPoint + (sideVector * strafeDirection * 5f);




        navMeshAgent.SetDestination(targetpos);

        Quaternion lookrotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookrotation, Time.deltaTime * 5f);
      


    }

    void SwitchStrafeDirection()
    {
        strafeDirection *= -1;

    }
 

    public void TakeDamage(int damage)
    {
        int damageTaken = damage ^ 2 / damage + enemyDefense;
        enemyCurrentHealth -= damageTaken;
        if (enemyCurrentHealth <= 0) stateHandler.enemyState = EnemyStateHandler.EnemyState.OnDeath;
    }
    public void EnemyAttack()
    {

        if (attackController.isBusy) navMeshAgent.isStopped = true;


    }

    IEnumerator EnemyDeathAnimation()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }
    void CalculateDistancefromPlayer()
    {
        dirToPlayer = (player.position - transform.position).normalized;
    }

    public void EnemyDeath()
    {
        gameObject.SetActive(false);
    }
    void ResetState()
    {
        if(dirToPlayer.magnitude < strafeDistance)
        {
            EnemyStrafing();
        }
      
    }
   
}
