using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainMenuCleanUP : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        if(LobbyManager.Instance != null)
        {
            Destroy(LobbyManager.Instance.gameObject);
        }
        if (ServerManager.Instance != null)
        {
            Destroy(ServerManager.Instance.gameObject);
        }
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (WaitingRoomManager.Instance != null)
        {
            Destroy(WaitingRoomManager.Instance.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        
    }

}
