using System.Collections;
using TMPro;
using Unity.AppUI.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button hostButton;
    [SerializeField] private UnityEngine.UI.Button startButton;
    [SerializeField] private UnityEngine.UI.Button joinButton;
    [SerializeField] private GameObject blackScreen;

    [Header("Network Input Fields")]
    [SerializeField] private TMP_InputField ipInputField;   // <-- Added for IP Address
    [SerializeField] private TMP_InputField portInputField; // Existing Port Input
    [SerializeField] private TMP_Text portText;

    private void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        startButton.onClick.AddListener(StartFadeIn);
        joinButton.onClick.AddListener(JoinGame);
    }

    void Update()
    {

    }

    public void HostButtonOnClick()
    {
        ushort lobbyPort = (ushort)Random.Range(7777, 30000);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            // Hosts should listen on "0.0.0.0" (all available network interfaces) 
            // and use the randomized port.
            transport.SetConnectionData("0.0.0.0", lobbyPort);
            Debug.Log($"Hosting on Port: {lobbyPort}");
        }

        NetworkManager.Singleton.StartHost();
        portText.SetText($"Port: {lobbyPort}");
    }

    public void StartFadeIn()
    {
        StartCoroutine(StartDelay());
        blackScreen.GetComponent<Animator>().SetBool("isFade", false);
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Main Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            // 1. Resolve target IP address (fallback to local loopback if empty)
            string targetIP = string.IsNullOrEmpty(ipInputField.text) ? "127.0.0.1" : ipInputField.text.Trim();

            // 2. Resolve target Port (fallback to a default if empty/invalid)
            ushort targetPort = 7777; // Default fallback port
            if (!string.IsNullOrEmpty(portInputField.text))
            {
                if (!ushort.TryParse(portInputField.text, out targetPort))
                {
                    Debug.LogError("Invalid Port format entered! Aborting connection.");
                    return;
                }
            }

            // 3. Apply BOTH the IP and Port to the transport configuration layer
            transport.SetConnectionData(targetIP, targetPort);
            Debug.Log($"Attempting to join {targetIP} on Port: {targetPort}");
        }

        NetworkManager.Singleton.StartClient();
    }

    public void CloseHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Network Shutdown successful.");
        }
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(6f);
        StartGame();
    }
}