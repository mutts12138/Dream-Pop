using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.BoolParameter;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private List<PickUpSO> pickUpPool = new List<PickUpSO>();
    public NetworkVariable<float> countDownTimer {  get; private set; }
    public NetworkVariable<float> roundTimer { get; private set; }

    private NetworkVariable<GameStates> gameState;

    [SerializeField] float scoreboardDisplayTime;

    public event EventHandler OnPlayerCharacterSpawned;

    public event EventHandler OnWaitingToStart;
    public event EventHandler OnCountDownToStart;
    public event EventHandler OnGamePlaying;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameOver;

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
        countDownTimer = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        roundTimer = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        gameState = new NetworkVariable<GameStates>(GameStates.waitingToStart, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        if (Instance != null)
        {
            
            Destroy(gameObject);
            return;
        }

        Instance = this;


        

        
        
        //gameObject.GetComponent<NetworkObject>().Spawn(true);
       


        //get playerdata from lobbymanager(doesnt destroy on load)
        //lobbymanager will spawn the player objects
        //LobbyManager.Instance.SpawnAllPlayerObjects();

    }


    private void Start()
    {
        gameState.OnValueChanged += (GameStates previousValue, GameStates newValue) =>
        {
            switch (newValue)
            {
                case GameStates.waitingToStart:
                    OnWaitingToStart?.Invoke(this, EventArgs.Empty);
                    break;
                case GameStates.countDownToStart:
                    OnCountDownToStart?.Invoke(this, EventArgs.Empty);
                    break;
                case GameStates.gamePlaying:
                    OnGamePlaying?.Invoke(this, EventArgs.Empty);
                    break;
                case GameStates.gamePaused:
                    OnGamePaused?.Invoke(this, EventArgs.Empty);
                    break;
                case GameStates.gameOver:
                    OnGameOver?.Invoke(this, EventArgs.Empty);
                    break;
            }
        };

        OnWaitingToStart += GameManager_OnWaitingToStart;
        OnCountDownToStart += GameManager_OnCountDownToStart;
        OnGamePlaying += GameManager_OnGamePlaying;
        OnGamePaused += GameManager_OnGamePaused;
        OnGameOver += GameManager_OnGameOver;

        if (!IsServer) return;
        GameMultiplayer.Instance.OnLoadingPlayersComplete += GameMultiplayer_OnLoadingPlayersComplete;

    }
    public override void OnNetworkSpawn()
    {

        //initialize players

        //LobbyManager.Instance.SpawnPlayerObjectClientRpc(localClientID);


        if (!IsServer) return;
        

    }

    

    private void GameManager_OnWaitingToStart(object sender, EventArgs e)
    {
        Debug.Log("waiting to start");
        //reset game manager
        //ResetGameManager();

    }
    private void GameManager_OnCountDownToStart(object sender, EventArgs e)
    {
        GameInput.instance.EnablePlayerInputAction(false);
        Debug.Log("countdown to start");
    }
    private void GameManager_OnGamePlaying(object sender, EventArgs e)
    {
        GameInput.instance.EnablePlayerInputAction(true);
        Debug.Log("game is playing");
        //activate player controls
    }
    private void GameManager_OnGamePaused(object sender, EventArgs e)
    {
        //not in use
    }
    private void GameManager_OnGameOver(object sender, EventArgs e)
    {
        Debug.Log("The Game has ended");
        //deactivate player controls


        //GameMultiplayer.Instance.UpdatePlayerScoring();


        //display scoreboard for a few seconds
        //return to lobby
        if (!IsServer) return;
        StartCoroutine(DisplayScoreBoardThenReturnToLobby(scoreboardDisplayTime));
    }

    IEnumerator DisplayScoreBoardThenReturnToLobby(float displayTime)
    {
        WaitForSeconds waitForSec = new WaitForSeconds(displayTime);


        //display scoreboard
        yield return waitForSec;

        Debug.Log("returning back to lobby");

        GameMultiplayer.Instance.OnLoadingPlayersComplete -= GameMultiplayer_OnLoadingPlayersComplete;

        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);

        //somehow still goes through even though the gameobject is destroyed
        SceneLoader.LoadNetwork(SceneLoader.Scene.WaitingRoom);

    }

    private void GameMultiplayer_OnLoadingPlayersComplete(object sender, EventArgs e)
    {
        //initialize need to happen after the playercharacters have been created
        InitializeRound();
    }

   
    


    private void InitializeRound()
    {
        Debug.Log("Initialize gamemanager");
        ImplementGameMode();
        FillPickUpPool();
        AssignPickUpToBlocks();

        countDownTimer.Value = 3f;
        roundTimer.Value = MapData.Instance.GetRoundTime();

        gameState.Value = GameStates.countDownToStart;
    }

    private void ImplementGameMode()
    {
        Debug.Log(GameModeData.Instance);

        //subscribe checkwincondition to events
        switch (GameModeData.Instance.gameModeName)
        {
            case "Elimination":
                foreach(PlayerCharacter player in GameMultiplayer.Instance.playerCharacterList)
                {
                    if(player != null)
                    {
                        player.OnDeath += CheckIsWinConditionAchieved;
                    }
                    
                }
                break;
        }
    }

    private void FillPickUpPool()
    {
        foreach(PickUpSO pickUpSO in MapData.Instance.GetPickUpPoolSO().pickUpSOList)
        {
            pickUpPool.Add(pickUpSO);
        }
    }

    private void AssignPickUpToBlocks()
    {
        //pick random block from block list and assign a pick up until pool is empty
        int blockCount = MapData.Instance.GetBlockObjectList().Count;


        //could result in infinite loop if blockcount is less than pickup count
        while(pickUpPool.Count > 0)
        {
            PickUpSO pickUpSO = pickUpPool[0];

            //get a random block from the blocklist
            BlockObject block = MapData.Instance.GetBlockObjectList()[UnityEngine.Random.Range(0, blockCount - 1)];

            //if the block has pickupHolder, then it must be block_pickup
            if (block.gameObject.TryGetComponent<PickUpHolder>(out PickUpHolder pickUpHolder))
            {
                //only assign it a pickup if it doesnt already hold one
                if (pickUpHolder.GetPickUpsHeld().Count == 0)
                {

                    pickUpHolder.AddPickUpToInventory(pickUpSO.InitializePickUpEffect(pickUpHolder));

                    pickUpPool.Remove(pickUpSO);
                }
            }

        }
    }

    private void CheckIsWinConditionAchieved(object sender, EventArgs e)
    {
        if (GameModeData.Instance.IsWinConditionAchieved())
        {
            Invoke(nameof(InvokeGameOver), 1);
        }
    }

    private void InvokeGameOver()
    {
        gameState.Value = GameStates.gameOver;
    }


    private void Update()
    {

        if (!IsServer) return;
        HandleTimer();

    }

    private void HandleTimer()
    {
        if(gameState.Value == GameStates.countDownToStart && countDownTimer.Value > 0)
        {
            countDownTimer.Value -= Time.deltaTime;
            if(countDownTimer.Value <= 0)
            {
                gameState.Value = GameStates.gamePlaying;
            }
        }
        if (gameState.Value == GameStates.gamePlaying && roundTimer.Value > 0)
        {
            roundTimer.Value -= Time.deltaTime;
            if(roundTimer.Value <= 0)
            {
                gameState.Value = GameStates.gameOver;
            }
        }
        
    }

    


    public override void OnDestroy()
    {
        if (IsServer)
        {
            if (gameObject.GetComponent<NetworkObject>().IsSpawned == true)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }

        }

        base.OnDestroy();
    }

}