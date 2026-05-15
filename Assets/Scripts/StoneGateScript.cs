using Unity.Netcode;
using UnityEngine;

public class StoneGateScript : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject floatingButtonUI = null;
    [SerializeField] GameObject confirmationWindow = null;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSFX;
    [SerializeField] private AudioClip closeSFX;

    bool isInTrigger;
    void Start()
    {
       
    }


    void Update()
    {
        // Find the local player to check their status
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();

        // Logic: Only allow E if we are in trigger AND the local player isn't ready
        if (isInTrigger && localPlayer != null && !localPlayer.IsReadySynced.Value)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                confirmationWindow.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();

            // Check the SYNCED variable
            if (player != null && player.IsLocalPlayer && !player.IsReadySynced.Value)
            {
                isInTrigger = true;
                floatingButtonUI.SetActive(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        isInTrigger = false;
    }
    public void OpenGate()
    {
        animator.SetBool("IsOpen", true);
        PlaySound(openSFX);
    }
    public void CloseGate()
    {
        animator.SetBool("IsOpen", false);
    }
    public void OnConfirmReady() // Link THIS to the Button's OnClick event
    {
        // 1. Tell the player script to register as 'Ready' on the network
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();

            if (localPlayer != null)
            {
                // This triggers the NetworkVariable update and the GameManager count
                localPlayer.SetReadyRpc();
            }
        }

        // 2. Local cleanup (This stops the UI from showing for the person who clicked)
        confirmationWindow.SetActive(false);
        floatingButtonUI.SetActive(false);

        // We set this to false so the Update loop doesn't try to open the window again
        isInTrigger = false;
    }
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // PlayOneShot para hindi maputol ang tunog kung magkasunod na tinawag
            audioSource.PlayOneShot(clip); 
        }
    }
}
