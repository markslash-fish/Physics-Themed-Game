using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour, IDamageable
{
    private HashSet<IDamageable> alreadyhit = new HashSet<IDamageable>();
    public enum PhaseLevel
    {
        One,
        Two,
        Three
    }
    public Transform player;
    public GuardianDataManager guardianData;
    public AttackRandomizer attackRandomizer;
    PlayerMovementScript playerMovementScript;
    [SerializeField] private float enemyCurrentHealth;
    [SerializeField] private float enemyMaxHealth;
    [SerializeField] private float enemyMinAttackPower;
    [SerializeField] private float enemyMaxAttackPower;
    [SerializeField] private float enemyDefense;
    [SerializeField] private float enemyMinAP;
    [SerializeField] private float enemyMaxAP;
    [SerializeField] private float enemySpeed;
    private float enemyDamage;

    public static event Action onEnemyDeath;
    public static event Action<float> onEnemyAttack;
   

    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    int strafeDirection = 1;
    private bool isAttacking;
    private bool isInCombat;

    private void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        enemyMaxHealth = guardianData.enemyHealth;
        enemyCurrentHealth = enemyMaxHealth;
        enemyDefense = guardianData.enemyDefense;
        enemySpeed = guardianData.enemySpeed;
        enemyMinAP = guardianData.enemyMinAttackPower;
        enemyMaxAP = guardianData.enemyMaxAttackPower;



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
        EnemyStrafing();
        
    }
    public void EnemyStrafing()
    {
        if (player == null) return;

        // 1. Calculate direction vectors
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Vector3 sideVector = Vector3.Cross(Vector3.up, dirToPlayer);
        float strafeDistance = 10f;
       
       
        Vector3 orbitPoint = player.position - (dirToPlayer * strafeDistance);
        Vector3 targetpos = orbitPoint + (sideVector * strafeDirection * 5f);
         
    

    // 3. Update Destination
    navMeshAgent.SetDestination(targetpos);

        Quaternion lookrotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookrotation, Time.deltaTime * 5f);


    }
    void EnemyDeath()
    {
        if(enemyCurrentHealth <= 0)
        {
            StartCoroutine(EnemyDeathAnimation());
        }
    }
    void SwitchStrafeDirection()
    {
        strafeDirection *= -1;

    }

    public void TakeDamage()
    {
        float attAndDefSum = playerMovementScript.playerDamage + enemyDefense;
        float damagetakenValue = playerMovementScript.playerDamage * playerMovementScript.playerDamage / attAndDefSum;
        enemyCurrentHealth -= damagetakenValue;
    }
    public void EnemyAttacK()
    {
        alreadyhit.Clear();
        isAttacking = true;
    }
  
    IEnumerator EnemyDeathAnimation()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }
   
}
