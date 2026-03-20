using System;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class EnemyAttackController : MonoBehaviour
{

    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyStateHandler stateHandler;

    public Transform player;
    public List<CombatMove> moveset;
    public List<CombatMove> movesInCooldown = new List<CombatMove>();
    private Animator animator;
    public bool isBusy;
    public float distance1;
    public bool inCooldown;


    private void Awake()
    {
       if(!isBusy) InvokeRepeating("ChooseAction", 1f, 1.2f);

    }
    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        stateHandler = GetComponent<EnemyStateHandler>();
    }
    private void OnEnable()
    {
       
    }
    private void Update()
    {

    }
   public  void ChooseAction()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        distance1 = enemyAI.dirToPlayer.magnitude;
        if (!isBusy)
        {
            DecideAction(distanceToPlayer);
        }

    }
    void DecideAction(float distance)
    {
        List<CombatMove> availableMoves = new List<CombatMove>();
       
        foreach (var move in moveset)
        {
           
            if(distance >= move.minRange && distance <= move.maxRange && !movesInCooldown.Contains(move))
            {
               
                availableMoves.Add(move);
            }
        }
        if(availableMoves.Count > 0)
        {
            CalculateTotalWeight();
        }
        Debug.Log("I found " + availableMoves.Count + " moves in range.");
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
            if(randomWeight <= iterations && !inCooldown)
            {
                ExecuteMove(move);
                return;
            }
        }
    }
   public void ExecuteMove(CombatMove move)
    {
        StartCoroutine(MoveCooldown(move));
        stateHandler.enemyState = EnemyStateHandler.EnemyState.IsAttacking;
        Debug.Log(move.moveName);
        
    }
  
   IEnumerator MoveCooldown(CombatMove move)
    {
        movesInCooldown.Add(move);
        yield return new WaitForSeconds(move.cooldown);
        movesInCooldown.Remove(move);

    }




}
