using UnityEngine;

public class ThroneScript : MonoBehaviour
{
    public GameObject buttonVisual = null;
    public bool isInTrigger;
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
           
            
            if (player != null && player.IsLocalPlayer)
            {
                isInTrigger = true;
                buttonVisual.SetActive(true);
               
            }
        }
    }
    public void OnTriggerExit(Collider other)
    {
        isInTrigger = false;
        buttonVisual.SetActive(false);
    }
}
