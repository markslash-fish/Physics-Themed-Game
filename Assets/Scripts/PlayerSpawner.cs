using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        // Only the Server/Host manages spawning
        if (!IsServer) return;

        // 1. Listen for NEW clients joining
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;

        // 2. Spawn the Host (since they are already "connected")
        SpawnPlayer(NetworkManager.Singleton.LocalClientId);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Check if a player object already exists for this client to prevent double-spawning
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            return;

        if (spawnPoints.Length == 0) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject player = Instantiate(playerPrefab, point.position, point.rotation);

        // This replaces the "Player Prefab" slot in NetworkManager
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }


}