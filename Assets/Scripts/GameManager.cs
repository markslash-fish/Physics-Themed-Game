using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> PlayersReadyCount = new NetworkVariable<int>(0);

    [SerializeField] private int maxPlayers;
    [SerializeField] private GameObject startingGate;
    [SerializeField] private int deadPlayers;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        maxPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    [Rpc(SendTo.Server)]
    public void SetPlayerReadyRpc(bool isReady)
    {
        // Validation: Logic only runs on the server
        if (isReady)
        {
            PlayersReadyCount.Value++;
        }
        else
        {
            PlayersReadyCount.Value--;
        }

        CheckReadyStatus();
    }

    private void CheckReadyStatus()
    {
        // Ensure we don't start with just 1 person if maxPlayers is 2
        int connectedCount = NetworkManager.Singleton.ConnectedClients.Count;
        var gateReady = startingGate.GetComponent<StoneGateScript>();

        gateReady.isReady = true;

        if (PlayersReadyCount.Value >= maxPlayers && connectedCount >= maxPlayers)
        {
            // Modern Rpc: Sending from Server to everyone (Clients)
            OpenGateRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void OpenGateRpc()
    {
        if (startingGate != null)
        {
           var gateAnim = startingGate.GetComponent<Animator>();
            gateAnim.SetBool("IsOpen", true);
        }
    }
}