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

    private void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        InitializeWaitingRoom();
        

        /*
        NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) =>
        {
            NetworkManager_OnLoadEventCompleted(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);

        };*/


        //This is for returning back to lobby after a game is complete: shouldnt be placed here
        //NetworkManager.SceneManager.OnLoadComplete += (ulong clientId, string sceneName, LoadSceneMode loadSceneMode) => { };
        //NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToLobby(clientsCompleted , clientsTimedOut); };


    }

    

    private void InitializeWaitingRoom()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { OnNetworkManager_OnClientConnectedCallBack(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { OnNetworkManager_OnClientDisconnectedCallBack(clientID); };
        SpawnAllPlayerObjectsToWaitingRoom();
    }

    //calls when coming back to lobby after game ends
    private void SpawnAllPlayerObjectsToWaitingRoom()
    {
        if (!IsServer) return;

        Debug.Log("spawning all players.");

        foreach (PlayerData playerData in GameMultiplayer.Instance.GetRoomPlayerDataNetworkList())
        { 
            SpawnPlayerObjectToWaitingRoom(playerData.clientID);
        }
        //client timedout show grey out icon
    }

    private void OnNetworkManager_OnClientConnectedCallBack(ulong clientID)
    {
        AddPlayerToWaitingRoom(clientID);
    }

    private void OnNetworkManager_OnClientDisconnectedCallBack(ulong clientID)
    {
        RemovePlayerFromWaitingRoom(clientID);
    }

    

    //this for UI purposes?
    private void AddPlayerToWaitingRoom(ulong clientID)
    {
        //spawn playerObject for clients
        SpawnPlayerObjectToWaitingRoom(clientID);

        //when maxed out set connection approval to false
    }

    private void RemovePlayerFromWaitingRoom(ulong clientID)
    {
        
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


    


    public void LoadGameScene()
    {


        //SceneManager.LoadScene(1);
        NetworkManager.SceneManager.LoadScene("TestMap", LoadSceneMode.Single);

    }



    
    private void OnDisable()
    {

        
    }
    
}
