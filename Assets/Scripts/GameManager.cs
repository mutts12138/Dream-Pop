using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player playerPreFab;
    [SerializeField] private PlayerUI playerUI;

    //maybe get it from map later
    [SerializeField] private float globalGravityAcc = -100f;
    [SerializeField] private float globalGravityMaxSpeed = -10f;

    

    ulong[] clientIds = new ulong[8];

    delegate void StartGame();
    private event StartGame OnStartGame;

    private ulong localClientID;

    //OnServerConnect
    //OnServerAddPlayer
    //when connected
    //clientIDs array

    //players hold playerNumbers
    //playerNumbers
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(gameObject);


        if (!IsServer) return;

        //gameObject.GetComponent<NetworkObject>().Spawn(true);



        //get playerdata from lobbymanager(doesnt destroy on load)
        //lobbymanager will spawn the player objects
        //LobbyManager.Instance.SpawnAllPlayerObjects();

    }

    public override void OnNetworkSpawn()
    {
        localClientID = NetworkManager.Singleton.LocalClientId;
        //initialize players

        //LobbyManager.Instance.SpawnPlayerObjectClientRpc(localClientID);

        
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToGame(clientsCompleted, clientsTimedOut); };

    }

    private void OnDisable()
    {
        //NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToGame(clientsCompleted, clientsTimedOut); };
    }

    public void SpawnAllPlayerObjectsToGame(List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
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
                    SpawnPlayerObjectToGameServerRpc(playerData.clientID, playerData.teamNumber);
                }
            }
        }
        //client timedout show grey out icon
    }

    [ServerRpc]
    public void SpawnPlayerObjectToGameServerRpc(ulong clientId, int teamNumber)
    {

        
        Player playerObj = Instantiate(playerPreFab);

        //if (NetworkManager.LocalClientId != clientID) return;

        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        playerObj.SetClientId(clientId);

        playerObj.SetTeamNumber(teamNumber);

        Debug.Log(teamNumber);

        Vector3 spawnPosition = AssignSpawnPointPosition(playerObj.GetTeamNumber());
        playerObj.SetPlayerPositionClientRpc(spawnPosition);

        //playerObj.GetComponent<NetworkObject>().Spawn(true);
        //Debug.Log("ClientID: " + clientID + "_playerObject Spawned");
        //Debug.Log(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject);

    }

    private Vector3 AssignSpawnPointPosition(int teamNumber)
    {
        foreach(SpawnPoint spawnPoint in MapDataManager.Instance.GetSpawnPoints())
        {
            if (spawnPoint.GetIsTaken() == false && spawnPoint.GetTeamNumber() == teamNumber)
            {
                return spawnPoint.transform.position;
            }
        }
        
        return Vector3.zero;
        
    }


    public float GetGlobalGravityAcc()
    {
        return globalGravityAcc;
    }

    public float GetGlobalGravityMaxSpeed()
    {
        return globalGravityMaxSpeed;
    }


    
    
}