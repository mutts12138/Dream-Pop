using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkManager;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class GameMultiplayer : NetworkBehaviour
{
    //handles the multiplayer aspect of the game spawning objects, separate to another script
    public event EventHandler OnPlayerDataNetworkListChanged;
    private NetworkList<PlayerData> PlayerDataNetworkList;

    public static GameMultiplayer Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        
        PlayerDataNetworkList = new NetworkList<PlayerData>(new PlayerData[0], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { NetworkManager_OnClientConnectedCallback(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { NetworkManager_OnClientDisconnectedCallback(clientID); };
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void NetworkManager_OnClientConnectedCallback(ulong clientID)
    {
        
        if (!IsServer) return;
        AddNewPlayerToPlayerConnectedList(clientID);
    }

    private void NetworkManager_OnClientDisconnectedCallback(ulong clientID)
    {
        if (!IsServer) return;
        RemovePlayerFromPlayerConnectedList(clientID); 
        Debug.Log($"Approval Declined Reason: {NetworkManager.Singleton.DisconnectReason}");
        
    }


    public void AddNewPlayerToPlayerConnectedList(ulong clientID)
    {
        PlayerData newPlayer = new PlayerData(clientID);

        PlayerDataNetworkList.Add(newPlayer);

    }

    public void RemovePlayerFromPlayerConnectedList(ulong clientID)
    {
        for (int i = 0; i < PlayerDataNetworkList.Count; i++)
        {
            if (PlayerDataNetworkList[i].clientID == clientID)
            {
                PlayerDataNetworkList.RemoveAt(i) ; break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerTeamNumberServerRpc(ulong clientID, int teamNumber)
    {
        for (int i = 0; i < PlayerDataNetworkList.Count; i++)
        {
            if (PlayerDataNetworkList[i].clientID == clientID)
            {
                PlayerData playerData = PlayerDataNetworkList[i];
                playerData.teamNumber = teamNumber;
                PlayerDataNetworkList[i] = playerData;
                break;
            }
        }

    }

    public NetworkList<PlayerData> GetRoomPlayerDataNetworkList()
    {
        return PlayerDataNetworkList;
    }


    public void UpdatePlayerScoring(ulong clientID, int isWin)
    {

        for (int i = 0; i < PlayerDataNetworkList.Count; i++)
        {
            if (PlayerDataNetworkList[i].clientID == clientID)
            {
                PlayerData playerData = PlayerDataNetworkList[i];
                switch (isWin)
                {
                    case -1:
                        playerData.loseCount++;
                        break;
                    case 0:
                        playerData.drawCount++;
                        break;
                    case 1:
                        playerData.winCount++;
                        break;

                }
                
                PlayerDataNetworkList[i] = playerData;
                break;
            }
        }
    }

    


    //get dreambubbleSO (define in dreambubbleSOList as a scriptable object), player object
    /*
    [ServerRpc(RequireOwnership = false)]
    public void SpawnDreamBubbleObjectServerRpc(PlayerCharacter player)
    {
        //SpawnDreamBubbleObject(player.GetNetworkObject());
    }


    //need to implement an interface for GetNetworkObject(){return NetworkObject}
    
    */
    
    /*
    public void SpawnDreamBubbleObject(NetworkObjectReference playerNetworkObjectReference)
    {

        DreamBubble dreamBubbleTransform = Instantiate(dreamBubble);
        dreamBubbleTransform.transform.position = dreamBubbleLocation;
        dreamBubbleTransform.GetComponent<NetworkObject>().Spawn(true);

        //Debug.Log(this);
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCharacter player = playerNetworkObject.GetComponent<PlayerCharacter>();

        dreamBubbleTransform.SetPlayer(player.GetComponent<PlayerCharacter>());
        dreamBubbleTransform.SetPopPowerDistance(player.bubblePowerLevel);
    }

    
    private int GetDreamBubbleSOIndex(DreamBubbleSO dreamBubbleSO)
    {
        return dreamBubbleListSO.dreamBubbleList.IndexOf(dreamBubbleSO);
    }


    private DreamBubbleSO GetDreamBubbleSOFromIndex(int dreamBubbleSOIndex)
    {
        return dreamBubbleListSO.dreamBubbleSOList[dreamBubbleSOIndex];
    }
    */
    //do the same for pickup, buff ^^^^^^^^^^^
}
