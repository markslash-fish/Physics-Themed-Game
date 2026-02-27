using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    public enum PhaseLevel
    {
        One,
        Two,
        Three
    }
    public Transform player;
    public GuardianDataManager guardianData;
  
    [SerializeField] private float enemyHealth;
    [SerializeField] private float enemyMinAttackPower;
    [SerializeField] private float enemyMaxAttackPower;
    [SerializeField] private float enemyDefense;
    [SerializeField] private float enemySpeed;

     private float enemyDamage;

    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    int strafeDirection = 1;

    private void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        enemyHealth = guardianData.enemyHealth;
        enemyDefense = guardianData.enemyDefense;
        enemySpeed = guardianData.enemySpeed;
    }
    private void Start()
    {
        
        
       
        navMeshAgent.speed = enemySpeed;
        navMeshAgent.acceleration = enemySpeed * 2f;
        
       
        
        InvokeRepeating("SwitchStrafeDirection", 2f, 5f);
       
    }
    void Update()
    {
        enemyDamage = Random.Range(guardianData.enemyMinAttackPower, guardianData.enemyMaxAttackPower);
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
    void SwitchStrafeDirection()
    {
        strafeDirection *= -1;
    }

}
