using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BattleRoomScript : MonoBehaviour
{
    public GameObject backGate, frontGate;
    public List<GameObject> enemiesActive = new List<GameObject>();
    public List<GameObject> playersActive = new List<GameObject>();
    void Start()
    {

    }


    void Update()
    {
        if (enemiesActive.Count == 0)
        {
            frontGate.GetComponent<BattleDoorScript>().OpenBattleGate();

        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && !enemiesActive.Contains(other.gameObject))
        {
            var enemy = GetComponent<EnemyStateHandler>();
            enemiesActive.Add(other.gameObject);
            if (enemy.CurrentState == EnemyStateHandler.EnemyState.OnDeath)
            {
                  enemiesActive.Remove(other.gameObject);
            }
          

        }
        if (other.CompareTag("Player") && !playersActive.Contains(other.gameObject))
        {
            playersActive.Add(other.gameObject);
            if (playersActive.Count == 2)
            {
                backGate.GetComponent<BattleDoorScript>().CloseBattleGate();
            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playersActive.Contains(other.gameObject))
        {
            playersActive.Remove(other.gameObject);
            if (playersActive.Count == 0)
            {
                frontGate.GetComponent<BattleDoorScript>().CloseBattleGate();
            }

        }
    }
}

