using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> PlayersReadyCount = new NetworkVariable<int>(0);
    public NetworkVariable<int> DeadPlayersCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] public int maxPlayers;
    [SerializeField] private GameObject startingGate;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private GameObject endGameCanvas;
    [SerializeField] private GameObject endGameResults;
    [SerializeField] private GameObject throneObj;
    [SerializeField] private GameObject healerStatue;
    public GameObject confirmationWindow = null;

    public bool isInConfirmation()
    {
        return confirmationWindow != null && confirmationWindow.activeSelf;
    }
    public bool inEndGameInput()
    {
        return endGameCanvas != null && endGameCanvas.activeSelf;
    }

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
    private void Update()
    {
        TriggerReadyGate();
        TriggerThrone();
        TriggerHealerStatue();

        if (endGameCanvas.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("PanelFadeOut") && (endGameCanvas.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f))
        {
            endGameCanvas.SetActive(false);
            endGameResults.SetActive(true);
            endGameResults.GetComponent<Animator>().Play("EndGameFadeIn");
            playerName.SetText("Special Thanks To Player: " + $"<color=#FFD700>{nameInputField.text}</color>" + " for playing our game!");
            
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

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void TriggerReadyGate()
    {
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        var gate = startingGate.GetComponent<StoneGateScript>();
        // Logic: Only allow E if we are in trigger AND the local player isn't ready
        if (gate.isInTrigger && localPlayer != null && !localPlayer.IsReadySynced.Value)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {


                confirmationWindow.SetActive(true);
                Cursor.lockState = CursorLockMode.None;





            }
        }
    }
    public void TriggerThrone()
    {
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        var throne = throneObj.GetComponent<ThroneScript>();
        var playerAnimator = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Animator>();
        // Logic: Only allow E if we are in trigger AND the local player isn't ready
        if (throne.isInTrigger && localPlayer != null)
        {
            if (Input.GetKeyDown(KeyCode.E) && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Kneel"))
            {


                endGameCanvas.SetActive(true);
                endGameCanvas.GetComponent<Animator>().SetBool("isFade", false);
                playerAnimator.SetTrigger("Kneel");
                Cursor.lockState = CursorLockMode.None;
             




            }
        }
    }
    public void TriggerHealerStatue()
    {
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        var statue = healerStatue.GetComponent<HealingLightScript>();
        var playerAnimator = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Animator>();
        // Logic: Only allow E if we are in trigger AND the local player isn't ready
        if (statue.isInTrigger && localPlayer != null)
        {
            if (Input.GetKeyDown(KeyCode.E) && !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Kneel"))
            {


                
                playerAnimator.SetTrigger("Kneel");
                
              




            }
            if(playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Kneel") && playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                 
              if(localPlayer.currentPotionCount != localPlayer.maxPotionCount)
              {
                localPlayer.currentPotionCount = localPlayer.maxPotionCount;
              }
             if(localPlayer.playerBaseCurrentHealth.Value != localPlayer.playerBaseMaxHealth)
              {
                localPlayer.playerBaseCurrentHealth.Value = localPlayer.playerBaseMaxHealth;
              }
                playerAnimator.ResetControllerState();
            }
        }
    }
    public void ConfirmName()
    {
        var fadeOut = endGameCanvas.GetComponent<Animator>();
        fadeOut.SetBool("isFade", true);
        
    }

}