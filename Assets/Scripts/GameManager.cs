using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> PlayersReadyCount = new NetworkVariable<int>(0);
    public NetworkVariable<int> DeadPlayersCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private int maxPlayers;
    [SerializeField] private GameObject startingGate;
    [SerializeField] private GameObject gameOverUI;
    

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
    [Rpc(SendTo.Server)]
    public void NotifyPlayerDeathRpc()
    {
        DeadPlayersCount.Value++;
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        // Example: If everyone is dead, show UI
        if (DeadPlayersCount.Value >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            ShowGameOverUI_Rpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ShowGameOverUI_Rpc()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }
    public void RetryGame()
    {
        SceneManager.LoadScene("Main Game");
    }
    public void BacktoMainMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            // 2. Shut down the network (works for Host, Client, or Server)
            NetworkManager.Singleton.Shutdown();

            // 3. Optional: Clear local references if needed
            Debug.Log("Network Shutdown successful.");
        }
        SceneManager.LoadScene("Map");
    }
}