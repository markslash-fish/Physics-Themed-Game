using Unity.AppUI.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button hostButton;
    [SerializeField] private UnityEngine.UI.Button clientButton;



    private void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }


    void Update()
    {
        
    }
    public void HostButtonOnClick()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Main Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
           
    }
    public void ClientButtonOnClick()
    {
        NetworkManager.Singleton.StartClient();
    }
}
