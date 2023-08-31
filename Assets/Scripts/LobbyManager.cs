using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class LobbyManager : NetworkBehaviour
{


    // Start is called before the first frame update
    private static NetworkList<PlayerData> playersInLobby;

    public static LobbyManager Instance { get; private set; }

    

    //temp
    [SerializeField] private Player playerPreFab;

    private void Awake()
    {
        if (Instance !=null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(gameObject);


        playersInLobby = new NetworkList<PlayerData>(new PlayerData[8], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        //add self to Lobby
        if (!IsServer) return;

        ulong emptyClientID = 404;
        PlayerData emptyPlayer = new PlayerData(emptyClientID);
        emptyPlayer.Disconnected();
        for (int i = 0; i < playersInLobby.Count; i++)
        {
            playersInLobby[i] = emptyPlayer;
        }


        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { AddPlayerToLobby(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { RemovePlayerFromLobby(clientID); };

        //NetworkManager.SceneManager.OnLoadComplete += (ulong clientId, string sceneName, LoadSceneMode loadSceneMode) => { };
        NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjects(clientsCompleted , clientsTimedOut); };

        
    }

    

    private void AddPlayerToLobby(ulong clientID)
    {
        //call it when a new client has connected
        if (!IsServer) return;

        

        PlayerData newPlayer = new PlayerData(clientID);
        newPlayer.Connected();
        
        
        for (int i = 0; i < playersInLobby.Count; i++)
        {
            if (playersInLobby[i].isConnected == false)
            { 
                playersInLobby[i] = newPlayer;

                break;
            }
        }


        //spawn playerObject for clients
        SpawnPlayerObjectServerRpc(clientID);

        
        //print amount of players in lobby
        int playerCount = 0;
        foreach (PlayerData player in playersInLobby)
        {
            if (player.isConnected == true)
            {
                playerCount++;
            }
        }
        Debug.Log("players in lobby " + playerCount);


        //when maxed out set connection approval to false
    }

    private void RemovePlayerFromLobby(ulong clientID)
    {
        if (!IsServer) return;

        ulong emptyClientID = 404;
        PlayerData emptyPlayer = new PlayerData(emptyClientID);
        emptyPlayer.Disconnected();

        for (int i = 0; i < playersInLobby.Count; i++)
        {
            if (playersInLobby[i].clientID == clientID)
            {
                playersInLobby[i] = emptyPlayer; break;
            }
        }
    }

    
    

    
    public void LoadScene()
    {
        //SceneManager.LoadScene(1);
        NetworkManager.SceneManager.LoadScene("TestMap", LoadSceneMode.Single);
        
    }


    [ServerRpc]
    public void SpawnPlayerObjectServerRpc(ulong clientId)
    {
        
        Player playerObj = Instantiate(playerPreFab);

        //if (NetworkManager.LocalClientId != clientID) return;

        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        playerObj.SetClientId(clientId);

        //playerObj.GetComponent<NetworkObject>().Spawn(true);
        //Debug.Log("ClientID: " + clientID + "_playerObject Spawned");
        //Debug.Log(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject);

    }


    
    public void SpawnAllPlayerObjects(List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        Debug.Log("spawning all players.");

        

        foreach (ulong clientID in clientsCompleted)
        {
            foreach (PlayerData playerData in playersInLobby)
            {
                //Debug.Log(playerData.clientID);
                if (clientID == playerData.clientID)
                {
                    SpawnPlayerObjectServerRpc(playerData.clientID);
                }
            }
        }
        

        //client timedout show grey out icon
    }

    public NetworkList<PlayerData> GetPlayersInLobby()
    {
        return playersInLobby;
    }


}
