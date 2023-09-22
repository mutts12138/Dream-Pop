using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using UnityEngine.UI;
using System;
//using UnityEditor.PackageManager;

public class WaitingRoomManager : NetworkBehaviour
{
    public static WaitingRoomManager Instance { get; private set; }



    //temp
    [SerializeField] private PlayerCharacter playerPreFab;
    [SerializeField] private GameManager gameManagerPreFab;

    

    private void Awake()
    {
        if (Instance !=null)
        {
            
            Destroy(gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {

        

        if (!IsServer) return;

        //probably call/ handle it from playerdatamanager
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { OnNetworkManager_OnClientConnectedCallBack(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { OnNetworkManager_OnClientDisconnectedCallBack(clientID); };

        GameManager.Instance.onGameEnded += (object sender, EventArgs e) => { GameManager_OnGameEnded(sender, e); };

        
        NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) =>
        {
            NetworkManager_OnLoadEventCompleted(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);

        };
        //This is for returning back to lobby after a game is complete: shouldnt be placed here
        //NetworkManager.SceneManager.OnLoadComplete += (ulong clientId, string sceneName, LoadSceneMode loadSceneMode) => { };
        //NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToLobby(clientsCompleted , clientsTimedOut); };


    }

    private void OnDisable()
    {

        if (!IsServer) return;
        /*
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= (ulong clientID) => { AddPlayerToLobby(clientID); };
            NetworkManager.Singleton.OnClientDisconnectCallback -= (ulong clientID) => { RemovePlayerFromLobby(clientID); };
            
        }
        */
        //NetworkManager.SceneManager.OnLoadEventCompleted -= (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToLobby(clientsCompleted, clientsTimedOut); };
    }
    private void OnNetworkManager_OnClientConnectedCallBack(ulong clientID)
    {
        AddPlayerToWaitingRoom(clientID);
    }

    private void OnNetworkManager_OnClientDisconnectedCallBack(ulong clientID)
    {
        RemovePlayerFromWaitingRoom(clientID);
    }



    private void GameManager_OnGameEnded(object sender, EventArgs e)
    {
        ReturnToWaitingRoom();
    }

    private void NetworkManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "WaitingRoom") return;
        InitializeWaitingRoom(clientsCompleted, clientsTimedOut);
    }



    private void InitializeWaitingRoom (List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SpawnAllPlayerObjectsToWaitingRoom(clientsCompleted, clientsTimedOut);
    }

   



    private void AddPlayerToWaitingRoom(ulong clientID)
    {
        //call it when a new client has connected
        if (!IsServer) return;
        //spawn playerObject for clients
        SpawnPlayerObjectToWaitingRoom(clientID);


        //when maxed out set connection approval to false
    }

    private void RemovePlayerFromWaitingRoom(ulong clientID)
    {
        
    }





    [ServerRpc]
    public void SpawnPlayerObjectToWaitingRoomServerRpc(ulong clientID)
    {
        SpawnPlayerObjectToWaitingRoom(clientID);
    }

    public void SpawnPlayerObjectToWaitingRoom(ulong clientID)
    {
        PlayerCharacter playerObj = Instantiate(playerPreFab);

        //if (NetworkManager.LocalClientId != clientID) return;

        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);

        playerObj.SetOwnerClientId(clientID);

        //playerObj.GetComponent<NetworkObject>().Spawn(true);
        //Debug.Log("ClientID: " + clientID + "_playerObject Spawned");
        //Debug.Log(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject);
    }


    //calls when coming back to lobby after game ends
    private void SpawnAllPlayerObjectsToWaitingRoom(List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        Debug.Log("spawning all players.");

        foreach (ulong clientID in clientsCompleted)
        {
            foreach (PlayerData playerData in GameMultiplayer.Instance.GetRoomPlayerDataNetworkList())
            {
                //Debug.Log(playerData.clientID);
                if (clientID == playerData.clientID)
                {
                    SpawnPlayerObjectToWaitingRoom(clientID);
                }
            }
        }
        

        //client timedout show grey out icon
    }


    public void LoadGameScene()
    {


        //SceneManager.LoadScene(1);
        NetworkManager.SceneManager.LoadScene("TestMap", LoadSceneMode.Single);

    }

    public void ReturnToWaitingRoom()
    {
        Debug.Log("Returning back to Lobby");
        //load scene: lobby
        NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

    }

}
