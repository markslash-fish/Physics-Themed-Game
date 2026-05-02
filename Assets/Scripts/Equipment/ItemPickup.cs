using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    public ItemManager item;
    public GameObject itemVisual;
    public GameObject equipPrompt;

    bool playerNear;


    private void Start()
    {
        equipPrompt.SetActive(false);
    }
    void Update()
    {
        if(playerNear)
        {
            
            if (Input.GetKeyDown(KeyCode.E))
            {



              
                RequestEquipServerRPC();


               
            }
           
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestEquipServerRPC(RpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId;
       

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out var client))
        {
            var playerObject = client.PlayerObject;
            EquipmentManager manager = playerObject.GetComponent<EquipmentManager>();
            

            if (manager != null)
            {
                if (manager.primaryAugment != null && manager.secondaryAugment != null) return;
                int index = manager.allEquipment.IndexOf(item);

                
                manager.FindItemByIndexRpc(index);
            }
        }
        equipPrompt.SetActive(false);
        itemVisual.GetComponent<NetworkObject>().Despawn();
        this.enabled = false;
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