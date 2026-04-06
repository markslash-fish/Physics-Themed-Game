using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public EquipmentItem item;
    public GameObject itemVisual;
    public GameObject equipPrompt;

    bool playerNear;

    void Update()
    {
        if(playerNear)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E PRESSED");
                EquipmentManager manager = FindObjectOfType<EquipmentManager>();
                manager.EquipItem(item);
                
                Player player = FindObjectOfType<Player>();
                player.AddBootStats(item.speedBonus, item.jumpBonus);

                Destroy(itemVisual);
                
                equipPrompt.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered trigger");
        if(other.CompareTag("Player"))
        Debug.Log("PLAYER DETECTED");
         playerNear = true;
        
        equipPrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
            playerNear = false;
        equipPrompt.SetActive(false);
    }
}