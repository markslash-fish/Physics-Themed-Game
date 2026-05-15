using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class BattleRoomScript : NetworkBehaviour
{
    public GameObject backGate, frontGate;
    public List<GameObject> enemiesActive = new List<GameObject>();
    public List<GameObject> playersActive = new List<GameObject>();
    void Start()
    {

    }


    void Update()
    {
        enemiesActive.RemoveAll(enemy => enemy == null);
        if (enemiesActive.Count == 0)
        {
            var gateOpen = frontGate.GetComponent<BattleDoorScript>();
            frontGate.GetComponent<BattleDoorScript>().OpenBattleGate();
            gateOpen.OpenBattleGate();
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
                var gateClose = frontGate.GetComponent<BattleDoorScript>();
                gateClose.CloseBattleGate();
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
                
               var gateClose = frontGate.GetComponent<BattleDoorScript>();
                gateClose.CloseBattleGate();
            }

        }
    }
}

