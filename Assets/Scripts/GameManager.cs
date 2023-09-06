using Mono.CSharp;
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

    private float roundTimer;

    private NetworkVariable<GameStates> gameState;



    public enum GameStates
    {
        gameStart,
        gameInProgress,
        gamePaused,
        gameEnd
    };

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

        gameState = new NetworkVariable<GameStates>(GameStates.gameStart, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        if (!IsServer) return;

        gameState.OnValueChanged += (GameStates previousValue, GameStates newValue) => {  };
        //gameObject.GetComponent<NetworkObject>().Spawn(true);



        //get playerdata from lobbymanager(doesnt destroy on load)
        //lobbymanager will spawn the player objects
        //LobbyManager.Instance.SpawnAllPlayerObjects();

    }

    public override void OnNetworkSpawn()
    {
        //initialize players

        //LobbyManager.Instance.SpawnPlayerObjectClientRpc(localClientID);

        
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => 
        {
            InitializeRound(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);

        };

    }

    private void OnDisable()
    {
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted -= (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { InitializeRound(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut); };
    }


    private void Update()
    {

        if (!IsServer) return;

        
    }

    private void InitializeRound(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        //when everyone is loaded
        //spawn player objects
        SpawnAllPlayerObjectsToGame(clientsCompleted, clientsTimedOut);

        //Set the round timer for server
        StartCoroutine(RoundTimerCountDownAsync());

        //set the round timer for client
        CallRoundTimerCountDownClientRpc();
    }

    [ClientRpc]
    private void CallRoundTimerCountDownClientRpc()
    {
        StartCoroutine(RoundTimerCountDownAsync());
        //Enable player input
    }

    //logic for server, visual for client
    IEnumerator RoundTimerCountDownAsync()
    {
        SetRoundTimer(MapDataManager.Instance.GetRoundTime());

        yield return StartCoroutine(RoundStartCountDown()); ;

        while (roundTimer > 0)
        {
            roundTimer -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("Game has ended");

        if (IsServer)
        {
            gameState.Value = GameStates.gameEnd;
        }
    }

    IEnumerator RoundStartCountDown()
    {
        yield return new WaitForSeconds(3);

        Debug.Log("game has started");

        if (IsServer)
        {
            gameState.Value = GameStates.gameInProgress;
        }
        


    }

    


    //this happens only on server, acts as the true timer
    private void SetRoundTimer(float roundTime)
    {
        roundTimer = roundTime;
        SetClientRoundTimerClientRpc(roundTime);
    }

    //this happens only on all clients, acts as a visual time indicator
    [ClientRpc]
    private void SetClientRoundTimerClientRpc(float roundTime)
    {
        roundTimer = roundTime;
    }



    

    private void SpawnAllPlayerObjectsToGame(List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
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
    private void SpawnPlayerObjectToGameServerRpc(ulong clientId, int teamNumber)
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

    


    
    
    
}