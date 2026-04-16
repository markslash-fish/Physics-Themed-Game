using Unity.Netcode;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemManager item;
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
                EquipmentManager manager = FindFirstObjectByType<EquipmentManager>();
                manager.EquipItem(item);


                itemVisual.GetComponent<NetworkObject>().Despawn();
                
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