using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;
    public Transform [] spawnPoints;
    
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;

            SpawnPlayer(NetworkManager.Singleton.LocalClientId);
        }
    }
    public void SpawnPlayer(ulong clientId)
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)]; 

        GameObject player = Instantiate(playerPrefab, point.position, point.rotation);

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
