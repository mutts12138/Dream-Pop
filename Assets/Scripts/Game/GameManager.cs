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

    

    public PlayerCharacter[] playerCharacterObjectsArray { get; private set; } = new PlayerCharacter[8];

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
        


        roundTimer = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        gameState = new NetworkVariable<GameStates>(GameStates.waitingToStart, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        
        //gameObject.GetComponent<NetworkObject>().Spawn(true);
       


        //get playerdata from lobbymanager(doesnt destroy on load)
        //lobbymanager will spawn the player objects
        //LobbyManager.Instance.SpawnAllPlayerObjects();

    }


    private void Start()
    {

        if (!IsServer) return;

        gameState.OnValueChanged += (GameStates previousValue, GameStates newValue) =>
        {
            GameState_OnValueChanged(previousValue, newValue);
        };
    }
    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        //initialize players

        //LobbyManager.Instance.SpawnPlayerObjectClientRpc(localClientID);


        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        //actually get if waitroom or gamemap from MAPDATA
        if (sceneName == "WaitingRoom") return;
        InitializeRound(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);
    }

    private void OnDisable()
    {
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
    }


    private void Update()
    {

        if (!IsServer) return;


    }

    private void InitializeRound(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        //when everyone is loaded
        //spawn player object

        //bind PlayerUI to player

        ImplementGameMode();

        //Set the round timer for server
        //StartCoroutine(RoundTimerCountDownAsync());

        //NO NEED, JUST MAKE IT NETWORK VARIABLE
        //set the round timer for client
        //CallRoundTimerCountDownClientRpc();


    }


   
        
        

        //client timedout show grey out icon
        /*
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

        }*/

    
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

    

    
    
    //playerUI.CallBindPlayerUIToPlayer();



    


    private void ImplementGameMode()
    {
        Debug.Log(GameModeData.Instance);

        //subscribe checkwincondition to events
        switch (GameModeData.Instance.gameModeName)
        {
            case "Elimination":
                foreach(PlayerCharacter player in playerCharacterObjectsArray)
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
        return playerCharacterObjectsArray;
    }
    
    
}