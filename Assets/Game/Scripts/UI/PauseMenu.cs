using Unity.Netcode;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private ConnectionPage connectionPage;
    
    public void Activate()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Deactivate()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Leave()
    {
        gameObject.SetActive(false);
        if (NetworkManager.Singleton.IsServer)
        {
            GameManager.Main.StopServer();
            MatchManager.Main.StopGame();
        }
        else
        {
            GameManager.Main.StopClient();
        }
    }

    public void Close()
    {
        Deactivate();
        gameObject.SetActive(false);
    }
}
