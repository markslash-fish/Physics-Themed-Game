using UnityEngine;

public class HealingLightScript : MonoBehaviour
{
    public bool isInTrigger;
    public GameObject buttonVisual = null;
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
