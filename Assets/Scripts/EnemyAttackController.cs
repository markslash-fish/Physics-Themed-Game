using System;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class EnemyAttackController : MonoBehaviour
{

    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyStateHandler stateHandler;

  
    public List<CombatMove> moveset;
    public List<CombatMove> movesInCooldown = new List<CombatMove>();
    private Animator animator;
    public bool isBusy;
    public bool isTrackingPlayer;
    public float distance1;
    public bool inCooldown;
    private float distanceToPlayer;

    

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        stateHandler = GetComponent<EnemyStateHandler>();
        animator = GetComponent<Animator>();
       
    }
    private void Start()
    { 
            InvokeRepeating("ChooseAction", 3f, 4f); 
    }
    private void OnEnable()
    {
       
    }
    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, enemyAI.player.position);
       
    }
    public  void ChooseAction()
    {

        if (isBusy || stateHandler.enemyState == EnemyStateHandler.EnemyState.Idle || stateHandler.enemyState == EnemyStateHandler.EnemyState.IsExhausted || stateHandler.enemyState == EnemyStateHandler.EnemyState.OnDeath)
        {
            return;
        }
          
            DecideAction(distanceToPlayer);
        

    }
    void DecideAction(float distance)
    {
        List<CombatMove> availableMoves = new List<CombatMove>();
        if(!isBusy)
        {
            foreach (var move in moveset)
            {

                if (distance >= move.minRange && distance <= move.maxRange && !movesInCooldown.Contains(move))
                {

                    availableMoves.Add(move);
                }
            }
            if (availableMoves.Count > 0)
            {
                CalculateTotalWeight();
            }
            Debug.Log(availableMoves.Count);
        }
      
    }
    void CalculateTotalWeight()
    {
        float totalWeight = 0;
        foreach (var move in moveset) totalWeight += move.baseWeight;

        float randomWeight = UnityEngine.Random.Range(0, totalWeight);
        float iterations = 0;

        foreach (var move in moveset)
        {
            iterations += move.baseWeight;
            if (randomWeight <= iterations && !inCooldown)
            {
               
                ExecuteMove(move);
                return;
            }
        }
    }
   public void ExecuteMove(CombatMove move)
    {

        isBusy = true;
        stateHandler.enemyState = EnemyStateHandler.EnemyState.IsAttacking;
        animator.SetTrigger(move.animTrigger);
        StartCoroutine(MoveCooldown(move));
        Debug.Log(move.moveName);
        
    }
   IEnumerator MoveCooldown(CombatMove move)
    {
        movesInCooldown.Add(move);
        yield return new WaitForSeconds(move.cooldown);
        movesInCooldown.Remove(move);

    }
   
    

    




}
