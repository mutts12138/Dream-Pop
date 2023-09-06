using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDataManager : NetworkBehaviour
{
    private static NetworkList<PlayerData> playersList;

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


        playersList = new NetworkList<PlayerData>(new PlayerData[8], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
        for (int i = 0; i < playersList.Count; i++)
        {
            playersList[i] = emptyPlayer;
        }


        
    }

    public void AddNewPlayerToPlayerConnectedList(ulong clientID)
    {
        PlayerData newPlayer = new PlayerData(clientID);
        newPlayer.Connected();


        for (int i = 0; i < playersList.Count; i++)
        {
            if (playersList[i].isConnected == false)
            {
                playersList[i] = newPlayer;

                break;
            }
        }
    }

    public void RemovePlayerFromPlayerConnectedList(ulong clientID)
    {
        ulong emptyClientID = 404;
        PlayerData emptyPlayer = new PlayerData(emptyClientID);
        emptyPlayer.Disconnected();

        for (int i = 0; i < playersList.Count; i++)
        {
            if (playersList[i].clientID == clientID)
            {
                playersList[i] = emptyPlayer; break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerTeamNumberServerRpc(ulong clientID, int teamNumber)
    {
        PlayerData newPlayerWithNewTeamNumber = new PlayerData(clientID);
        newPlayerWithNewTeamNumber.teamNumber = teamNumber;

        for (int i = 0; i < playersList.Count; i++)
        {
            if (playersList[i].clientID == clientID)
            {
                playersList[i] = newPlayerWithNewTeamNumber; break;
            }
        }

    }

    public NetworkList<PlayerData> GetPlayerConnectedList()
    {
        return playersList;
    }
}

