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
using QFSW.QC;

public class GameMultiplayer : NetworkBehaviour
{
    //handles the multiplayer aspect of the game spawning objects, separate to another script
    [SerializeField] private PlayerCharacter playerPreFab;
    [SerializeField] private PlayerInGameUI playerUIPreFab;



    private NetworkList<PlayerData> PlayerDataNetworkList;
    public event EventHandler OnPlayerDataNetworkListChanged;

    //playercharacter cant be serialized across network, so only server keep a copy
    private List<PlayerCharacter> playerCharacterList;

    public static GameMultiplayer Instance { get; private set; }

    
    private void Awake()
    {
        
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        

        PlayerDataNetworkList = new NetworkList<PlayerData>(new PlayerData[0], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        playerCharacterList = new List<PlayerCharacter>();
        
       
        
    }
    void Start()
    {
        
        

        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { NetworkManager_OnClientConnectedCallback(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { NetworkManager_OnClientDisconnectedCallback(clientID); };
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        playerCharacterList= new List<PlayerCharacter>();
        SpawnAllPlayerObject();
    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        if (!IsServer) return;
        Debug.Log("network spawned");


        //add host to the list first
        AddNewPlayerToPlayerConnectedList(NetworkManager.Singleton.LocalClientId, LocalPlayerData.Instance.localPlayerData);
        //playerCharacterList.Add(SpawnPlayerObject(NetworkManager.Singleton.LocalClientId));
        SpawnAllPlayerObject();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        
        if (!IsServer) return;
        AddNewPlayerToPlayerConnectedListClientRpc(clientId);
        playerCharacterList.Add(SpawnPlayerObject(clientId));
        testClientMessageClientRpc();

    }

    [Command][ClientRpc]
    private void testClientMessageClientRpc()
    {
        Debug.Log("gameMultiplayer exists, and message is sent here");
    }

    private void NetworkManager_OnClientDisconnectedCallback(ulong clientID)
    {
        if (!IsServer) return;
        RemovePlayerFromPlayerConnectedList(clientID);
        RemoveNullInPlayerCharacterList();
        Debug.Log($"Approval Declined Reason: {NetworkManager.Singleton.DisconnectReason}");
        
    }

    //for host to add himself
    private void AddNewPlayerToPlayerConnectedList(ulong clientId, PlayerData playerData)
    {
        PlayerData newPlayerData = playerData;
        newPlayerData.clientId = clientId;
        playerData = newPlayerData;

        PlayerDataNetworkList.Add(playerData);

        //spawn the playercharacter object
        
        
    }

    //calls the client to pass the localplayerdata
    [ClientRpc]
    private void AddNewPlayerToPlayerConnectedListClientRpc(ulong clientId)
    {
        if(NetworkManager.Singleton.LocalClientId == clientId)
        {
            AddNewPlayerToPlayerConnectedListServerRpc(clientId, LocalPlayerData.Instance.GetLocalPlayerDataAssignNewClientIdClientRPC(clientId));
        }

    }

    //add the client passed localplayerdata to the playerdatalist and create a playercharacter for the client
    [ServerRpc(RequireOwnership = false)]
    private void AddNewPlayerToPlayerConnectedListServerRpc(ulong clientId, PlayerData playerData)
    {
        PlayerData newPlayerData = playerData;
        newPlayerData.clientId = clientId;
        playerData = newPlayerData;

        PlayerDataNetworkList.Add(playerData);

        //spawn the playercharacter object
        
    }


    private void RemovePlayerFromPlayerConnectedList(ulong clientId)
    {
        for (int i = 0; i < PlayerDataNetworkList.Count; i++)
        {
            if (PlayerDataNetworkList[i].clientId == clientId)
            {
                PlayerDataNetworkList.RemoveAt(i) ; break;
            }
        }
    }

    private void RemoveNullInPlayerCharacterList()
    {
        for (int i = 0; i < playerCharacterList.Count; i++)
        {
            if (playerCharacterList[i] == null)
            {
                PlayerDataNetworkList.RemoveAt(i); break;
            }
        }
    }





    public NetworkList<PlayerData> GetPlayerDataNetworkList()
    {
        return PlayerDataNetworkList;
    }




    public void UpdatePlayerScoring(ulong clientId, int isWin)
    {

        for (int i = 0; i < PlayerDataNetworkList.Count; i++)
        {
            if (PlayerDataNetworkList[i].clientId == clientId)
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


    //called when after loading a scene is complete
    private void SpawnAllPlayerObject()
    {
        int count = 0;
        Debug.Log(PlayerDataNetworkList[0].playerId);
        foreach (PlayerData playerData in PlayerDataNetworkList)
        {
            //Debug.Log(playerData.clientID);
            playerCharacterList.Add(SpawnPlayerObject(playerData.clientId));
            count++;
        }
    }




    public PlayerCharacter SpawnPlayerObject(ulong clientId)
    {


        PlayerCharacter playerObj = Instantiate(playerPreFab);

        //if (NetworkManager.LocalClientId != clientID) return;

        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        playerObj.SetOwnerClientId(clientId);

        playerObj.SetTeamNumber(1);

        return playerObj;
        //Debug.Log(teamNumber);

        //playerObj.GetComponent<NetworkObject>().Spawn(true);
        //Debug.Log("ClientID: " + clientID + "_playerObject Spawned");
        //Debug.Log(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject);

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
