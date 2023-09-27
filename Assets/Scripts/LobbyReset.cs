using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyReset : MonoBehaviour
{
    public static LobbyReset Instance;

    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void ResetBackToLobby()
    {
        NetworkManager.Singleton.Shutdown();
        LobbyManager.Instance.LeaveLobby();


        //destroy all the managers, new one will be created upon opening lobby scene.
        Destroy(GameManager.Instance.gameObject);
        Destroy(WaitingRoomManager.Instance.gameObject);

        //ServerManager
        Destroy(NetworkManager.Singleton.gameObject);
        Destroy(LobbyManager.Instance.gameObject);
        Destroy(GameMultiplayer.Instance.gameObject);
        SceneLoader.Load(SceneLoader.Scene.Lobby);
        //LobbyMessageUI.Instance.ShowMessage("Disconnected");
    }
}
