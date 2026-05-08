using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    void Start()
    {
        
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pausePanel.SetActive(true);
        }
    }
    public void BackToMainMenu()
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
