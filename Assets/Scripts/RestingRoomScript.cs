using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RestingRoomScript : MonoBehaviour
{
    public List<GameObject> playersInRoom = new List<GameObject>();
    public GameObject backGate, frontGate;

    public void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && !playersInRoom.Contains(other.gameObject))
        {
            playersInRoom.Add(other.gameObject);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && playersInRoom.Contains(other.gameObject))
        {
            playersInRoom.Remove(other.gameObject);
            if(playersInRoom.Count == 0)
            {
                var gate = frontGate.GetComponent<StoneGateScript>();
                gate.CloseGate();
            }
        }
    }
}
