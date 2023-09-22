using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ex_PlayerDataManager : NetworkBehaviour
{
   /*
    //migrate to the gamemultiplayer
    
    private NetworkList<PlayerData> playerDataNetworkList;
    
    public static PlayerDataManager Instance { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(gameObject);


        playerDataNetworkList = new NetworkList<PlayerData>(new PlayerData[8], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {

        if (!IsServer) return;

        //initialize empty values in the list
        ulong emptyClientID = 404;
        PlayerData emptyPlayer = new PlayerData(emptyClientID);
        emptyPlayer.Disconnected();
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            playerDataNetworkList[i] = emptyPlayer;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { OnNetworkManager_OnClientConnectedCallBack(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { OnNetworkManager_OnClientDisconnectedCallBack(clientID); };

    }

    private void OnNetworkManager_OnClientConnectedCallBack(ulong clientID)
    {
        AddNewPlayerToPlayerConnectedList(clientID);
    }

    private void OnNetworkManager_OnClientDisconnectedCallBack(ulong clientID)
    {
        RemovePlayerFromPlayerConnectedList(clientID);
    }


    public void AddNewPlayerToPlayerConnectedList(ulong clientID)
    {
        PlayerData newPlayer = new PlayerData(clientID);
        newPlayer.Connected();


        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].isConnected == false)
            {
                playerDataNetworkList[i] = newPlayer;

                break;
            }
        }
    }

    public void RemovePlayerFromPlayerConnectedList(ulong clientID)
    {
        ulong emptyClientID = 404;
        PlayerData emptyPlayer = new PlayerData(emptyClientID);
        emptyPlayer.Disconnected();

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientID == clientID)
            {
                playerDataNetworkList[i] = emptyPlayer; break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerTeamNumberServerRpc(ulong clientID, int teamNumber)
    {
        PlayerData newPlayerWithNewTeamNumber = new PlayerData(clientID);
        newPlayerWithNewTeamNumber.teamNumber = teamNumber;

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientID == clientID)
            {
                playerDataNetworkList[i] = newPlayerWithNewTeamNumber; break;
            }
        }

    }

    public NetworkList<PlayerData> GetPlayerConnectedList()
    {
        return playerDataNetworkList;
    }


    public void UpdatePlayerScoring()
    {
        int isWin = 1;
        if (GameModeData.Instance == null) return;
        int[] winnerTeams = GameModeData.Instance.GetWinnerTeams();

        //if more than 1 winnerteam, then its a draw: 0
        if (winnerTeams.Length > 1)
        {
            isWin = 0;
        }


        foreach (PlayerCharacter player in GameManager.Instance.playerObjectsArray)
        {
            if (player != null)
            {
                bool playerIsWinner = false;
                foreach (int teamNumber in winnerTeams)
                {
                    if (player.teamNumber.Value == teamNumber)
                    {
                        PlayerDataManager.Instance.AddToPlayerScoring(player.ownerClientID.Value, isWin);
                        playerIsWinner = true;
                    }
                }
                if (!playerIsWinner)
                {
                    PlayerDataManager.Instance.AddToPlayerScoring(player.ownerClientID.Value, -isWin);
                }

            }
        }

    }

    public void AddToPlayerScoring(ulong clientID, int isWin)
    {
        if (clientID == 404) return;
        // lose: -1, draw: 0, win: 1
        Mathf.Clamp(isWin, -1, 1);
        PlayerData updatePlayerData = new PlayerData(404);
        updatePlayerData.Disconnected();

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientID == clientID)
            {
                updatePlayerData = playerDataNetworkList[i];

                switch (isWin)
                {
                    case -1:
                        updatePlayerData.loseCount++;
                        break;
                    case 0:
                        updatePlayerData.drawCount++;
                        break;
                    case 1:
                        updatePlayerData.winCount++;
                        break;
                }

                playerDataNetworkList[i] = updatePlayerData;
                break;
            }
        }
        //invoke update scoring event for UI

    }
   */
}

