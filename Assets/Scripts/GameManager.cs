using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PlayerCharacter playerPreFab;
    [SerializeField] private PlayerUI playerUIPreFab;

    public PlayerCharacter[] playerObjectsArray { get; private set; } = new PlayerCharacter[8];

    private NetworkVariable<float> roundTimer;

    private NetworkVariable<GameStates> gameState;

    public event EventHandler onGameEnded;

    private Dictionary<ulong, bool> playerReadyDictionary;

    //not in use yet
    public enum GameStates
    {
        waitingToStart,
        countDownToStart,
        gamePlaying,
        gamePaused,
        gameOver
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


        roundTimer = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        gameState = new NetworkVariable<GameStates>(GameStates.waitingToStart, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        if (!IsServer) return;

        gameState.OnValueChanged += (GameStates previousValue, GameStates newValue) =>
        {
            GameState_OnValueChanged(previousValue, newValue);
        };
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
            NetworkManager_OnLoadEventCompleted(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);
        };

    }

    private void OnDisable()
    {
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted -= (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { NetworkManager_OnLoadEventCompleted(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut); };
    }


    private void Update()
    {

        if (!IsServer) return;


    }


    private void NetworkManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "WaitingRoom") return;
        InitializeRound(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);
    }

    private void InitializeRound(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        //when everyone is loaded
        //spawn player objects
        SpawnAllPlayerObjectsToGame(clientsCompleted, clientsTimedOut);

        //bind PlayerUI to player
        InstantiatePlayerUIClientRpc();

        ImplementGameMode();

        //Set the round timer for server
        //StartCoroutine(RoundTimerCountDownAsync());

        //NO NEED, JUST MAKE IT NETWORK VARIABLE
        //set the round timer for client
        //CallRoundTimerCountDownClientRpc();


    }

    /*[ClientRpc]
    private void CallRoundTimerCountDownClientRpc()
    {
        StartCoroutine(RoundTimerCountDownAsync());
        //Enable player input
    }
    */

    //logic for server, visual for client
    /*
    IEnumerator RoundTimerCountDownAsync()
    {
        SetRoundTimer(MapData.Instance.GetRoundTime());

        yield return StartCoroutine(RoundStartCountDown()); ;

        
        while (roundTimer.Value > 0)
        {
            roundTimer.Value -= Time.deltaTime;
            yield return null;
        }



        if (IsServer)
        {
            Debug.Log("RoundTimer Completed");
            if(gameState.Value != GameStates.gameOver)
            {
                gameState.Value = GameStates.gameOver;
                GameEnd();
            }
            
        }
    }

    IEnumerator RoundStartCountDown()
    {
        yield return new WaitForSeconds(3);



        if (IsServer)
        {
            Debug.Log("game has started");
            gameState.Value = GameStates.gamePlaying;
        }



    }
    */
    //this happens only on server, acts as the true timer
    private void SetRoundTimer(float roundTime)
    {
        roundTimer.Value = roundTime;
        //SetClientRoundTimerClientRpc(roundTime);
    }

    //this happens only on all clients, acts as a visual time indicator
    /*
    [ClientRpc]
    private void SetClientRoundTimerClientRpc(float roundTime)
    {
        roundTimer.Value = roundTime;
    }
    */

    private void SpawnAllPlayerObjectsToGame(List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        Debug.Log("spawning all players.");



        foreach (ulong clientID in clientsCompleted)
        {
            int count = 0;
            foreach (PlayerData playerData in GameMultiplayer.Instance.GetRoomPlayerDataNetworkList())
            {
                //Debug.Log(playerData.clientID);
                if (clientID == playerData.clientID)
                {
                    playerObjectsArray[count] = SpawnPlayerObjectToGame(playerData.clientID, playerData.teamNumber);
                }

                count++;
            }
        }
        //client timedout show grey out icon
        for (int i = 0; i < playerObjectsArray.Length; i++)
        {
            if (playerObjectsArray[i] != null)
            {
                Debug.Log("player slot " + i + ": " + playerObjectsArray[i].ownerClientID.Value);
            }
            else
            {
                Debug.Log("player slot " + i + ": empty player slot");
            }

        }

    }


    private PlayerCharacter SpawnPlayerObjectToGame(ulong clientId, int teamNumber)
    {


        PlayerCharacter playerObj = Instantiate(playerPreFab);

        //if (NetworkManager.LocalClientId != clientID) return;

        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        playerObj.SetOwnerClientId(clientId);

        playerObj.SetTeamNumber(teamNumber);

        //Debug.Log(teamNumber);

        Vector3 spawnPosition = AssignSpawnPointPosition(playerObj.teamNumber.Value);
        playerObj.SetPlayerPositionClientRpc(spawnPosition);

        return playerObj;
        //playerObj.GetComponent<NetworkObject>().Spawn(true);
        //Debug.Log("ClientID: " + clientID + "_playerObject Spawned");
        //Debug.Log(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject);

    }

    [ClientRpc]
    private void InstantiatePlayerUIClientRpc()
    {
        PlayerUI playerUI = Instantiate(playerUIPreFab);
        playerUI.CallBindPlayerUIToPlayer();
    }

    private Vector3 AssignSpawnPointPosition(int teamNumber)
    {
        foreach (SpawnPoint spawnPoint in MapData.Instance.GetSpawnPoints())
        {
            if (spawnPoint.GetIsTaken() == false && spawnPoint.GetTeamNumber() == teamNumber)
            {
                return spawnPoint.transform.position;
            }
        }

        return Vector3.zero;

    }


    private void ImplementGameMode()
    {
        Debug.Log(GameModeData.Instance);

        //subscribe checkwincondition to events
        switch (GameModeData.Instance.gameModeName)
        {
            case "Elimination":
                foreach(PlayerCharacter player in playerObjectsArray)
                {
                    if(player != null)
                    {
                        player.onEliminated += (object sender, EventArgs e) => { CheckIsWinConditionAchieved(); };
                    }
                    
                }
                break;
        }
    }

    private void CheckIsWinConditionAchieved()
    {
        if (GameModeData.Instance.IsWinConditionAchieved())
        {
            gameState.Value = GameStates.gameOver;

            GameEnd();
        }
    }

    private void GameState_OnValueChanged(GameStates previousValue, GameStates newValue)
    {

    }

    //Not in use yet
    private void onGameStateChange()
    {
        switch (gameState.Value)
        {
            case GameStates.waitingToStart:
                break;
            case GameStates.gamePlaying:
                break;
            case GameStates.gamePaused:
                break;
            case GameStates.gameOver:
                break;
        }
    }

    private void GameEnd()
    {
        roundTimer.Value = 0;
        
        Debug.Log("The Game has ended");

        onGameEnded?.Invoke(this, EventArgs.Empty);

        //GameMultiplayer.Instance.UpdatePlayerScoring();


        //display scoreboard for a few seconds
        //return to lobby
        
    }

    

    


    public PlayerCharacter[] GetPlayerObjArray()
    {
        return playerObjectsArray;
    }
    
    
}