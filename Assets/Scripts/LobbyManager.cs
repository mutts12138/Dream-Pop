using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }



    //temp
    [SerializeField] private Player playerPreFab;
    [SerializeField] private GameManager gameManagerPreFab;

    

    private void Awake()
    {
        if (Instance !=null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {

        

        if (!IsServer) return;


        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { AddPlayerToLobby(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { RemovePlayerFromLobby(clientID); };


        //This is for returning back to lobby after a game is complete: shouldnt be placed here
        //NetworkManager.SceneManager.OnLoadComplete += (ulong clientId, string sceneName, LoadSceneMode loadSceneMode) => { };
        //NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToLobby(clientsCompleted , clientsTimedOut); };

        
    }

    private void OnDisable()
    {
        
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= (ulong clientID) => { AddPlayerToLobby(clientID); };
            NetworkManager.Singleton.OnClientDisconnectCallback -= (ulong clientID) => { RemovePlayerFromLobby(clientID); };
            //gameObject.GetComponent<NetworkObject>().Despawn();
        }
        

        
        //NetworkManager.SceneManager.OnLoadEventCompleted -= (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToLobby(clientsCompleted, clientsTimedOut); };
    }

    private void AddPlayerToLobby(ulong clientID)
    {
        //call it when a new client has connected
        if (!IsServer) return;

        PlayerDataManager.Instance.AddNewPlayerToPlayerConnectedList(clientID);

        //spawn playerObject for clients
        SpawnPlayerObjectToLobbyServerRpc(clientID);


        //when maxed out set connection approval to false
    }

    private void RemovePlayerFromLobby(ulong clientID)
    {
        if (!IsServer) return;

        PlayerDataManager.Instance.RemovePlayerFromPlayerConnectedList(clientID);
    }

    

    
    public void LoadGameScene()
    {
        //create game manager based on map
        GameManager gameManager = Instantiate(gameManagerPreFab);
        gameManager.GetComponent<NetworkObject>().Spawn(true);

        //do the scene event sub here


        //SceneManager.LoadScene(1);
        NetworkManager.SceneManager.LoadScene("TestMap", LoadSceneMode.Single);
        
    }


    [ServerRpc]
    public void SpawnPlayerObjectToLobbyServerRpc(ulong clientId)
    {
        
        Player playerObj = Instantiate(playerPreFab);

        //if (NetworkManager.LocalClientId != clientID) return;

        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        playerObj.SetClientId(clientId);

        //playerObj.GetComponent<NetworkObject>().Spawn(true);
        //Debug.Log("ClientID: " + clientID + "_playerObject Spawned");
        //Debug.Log(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject);

    }


    //calls when coming back to lobby after game ends
    private void SpawnAllPlayerObjectsToLobby(List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        Debug.Log("spawning all players.");

        

        foreach (ulong clientID in clientsCompleted)
        {
            foreach (PlayerData playerData in PlayerDataManager.Instance.GetPlayerConnectedList())
            {
                //Debug.Log(playerData.clientID);
                if (clientID == playerData.clientID)
                {
                    SpawnPlayerObjectToLobbyServerRpc(playerData.clientID);
                }
            }
        }
        

        //client timedout show grey out icon
    }

    


}
