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

    public event EventHandler OnLoadingPlayersComplete;


    //playercharacter cant be serialized across network, so only server keep a copy
    public List<PlayerCharacter> playerCharacterList { get; private set; }

    public static GameMultiplayer Instance { get; private set; }


    [SerializeField] private PickUpSOListSO pickUpSOListSO;

    private void Awake()
    {
        PlayerDataNetworkList = new NetworkList<PlayerData>(new PlayerData[0], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        playerCharacterList = new List<PlayerCharacter>();

        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;

        
    }
    void Start()
    {

        
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { NetworkManager_OnClientConnectedCallback(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { NetworkManager_OnClientDisconnectedCallback(clientID); };

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        
    }


    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("fkfkfkfkfkkfkf");
        if (!IsServer) return;
        Debug.Log("GAME MANAGER STARTING SCENE");

        ClearAllPlayerDataCurrentScore();
        SaveAllPlayerDataToLocal();

        playerCharacterList = new List<PlayerCharacter>();
        SpawnAllPlayerObject();

        OnLoadingPlayersComplete?.Invoke(this, EventArgs.Empty);
    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        if (!IsServer) return;
        Debug.Log("network spawned");

        if(Instance == this)
        {
            //add host to the list first
            AddNewPlayerToPlayerConnectedList(NetworkManager.Singleton.LocalClientId, LocalPlayerData.Instance.localPlayerData);
            //playerCharacterList.Add(SpawnPlayerObject(NetworkManager.Singleton.LocalClientId));
            SpawnAllPlayerObject();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        
        if (!IsServer) return;
        AddNewPlayerToPlayerConnectedListClientRpc(clientId);

        
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
            AddNewPlayerToPlayerConnectedListServerRpc(clientId, LocalPlayerData.Instance.GetLocalPlayerDataAssignNewClientId(clientId));
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
        playerCharacterList.Add(SpawnPlayerObject(playerData));
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

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        for (int i = 0; i < PlayerDataNetworkList.Count; i++)
        {
            if (PlayerDataNetworkList[i].clientId == clientId)
            {
                return PlayerDataNetworkList[i];
            }
        }

        return new PlayerData("Null");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerCurrentTeamNumberServerRpc(ulong clientId, int newTeamNumber)
    {
        PlayerData playerData;

        foreach (PlayerData playerData1 in PlayerDataNetworkList)
        {
            if (playerData1.clientId == clientId)
            {
                playerData = playerData1;
                playerData.currentTeamNumber = newTeamNumber;
                UpdatePlayerData(playerData);
                return;
            }
        }
        
        

        
    }

    

    public void UpdatePlayerData(PlayerData newPlayerData)
    {

        for (int i = 0; i < PlayerDataNetworkList.Count; i++)
        {
            if (PlayerDataNetworkList[i].clientId == newPlayerData.clientId)
            {
                PlayerDataNetworkList[i] = newPlayerData;
                break;
            }
        }
    }


    private void SaveAllPlayerDataToLocal()
    {
        foreach (PlayerData playerData in PlayerDataNetworkList)
        {
            SavePlayerDataToLocalClientRPC(playerData);
        }
    }

    [ClientRpc]
    private void SavePlayerDataToLocalClientRPC(PlayerData playerData)
    {
        if(playerData.clientId == LocalPlayerData.Instance.localPlayerData.clientId)
        {
            LocalPlayerData.Instance.localPlayerData = playerData;
        }
    }

    private void ClearAllPlayerDataCurrentScore()
    {
        foreach(PlayerData playerData in PlayerDataNetworkList)
        {
            playerData.ClearCurrentScore();
            UpdatePlayerData(playerData);
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
            playerCharacterList.Add(SpawnPlayerObject(playerData));
            count++;
        }
    }

  


    public PlayerCharacter SpawnPlayerObject(PlayerData playerData)
    {


        PlayerCharacter playerCharacter = Instantiate(playerPreFab);

        //if (NetworkManager.LocalClientId != clientID) return;

        playerCharacter.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerData.clientId, true);

        playerCharacter.SetOwnerClientId(playerData.clientId);

        playerCharacter.SetTeamNumber(playerData.currentTeamNumber);

        playerCharacter.SetSpawnPointFromMapData();

        playerCharacter.SetPlayerPositionClientRpc(playerCharacter.spawnPosition);

        return playerCharacter;
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

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPickUpObjectServerRpc(Vector3 position, int pickUpSOIndex)
    {
        SpawnPickUpObject(position, GetPickUpSOFromIndex(pickUpSOIndex));
    }


    //need to implement an interface for GetNetworkObject(){return NetworkObject}

    public void SpawnPickUpObject(Vector3 position, PickUpSO pickUpSO)
    {
        //check the pick up pool
        //remove from pick up pool

        PickUpObject pickUpTransform = Instantiate(pickUpSO.pickUpPrefab);
        pickUpTransform.transform.position = position;
        pickUpTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    
    private int GetPickUpSOIndex(PickUpSO pickUpSO)
    {
        return pickUpSOListSO.pickUpSOList.IndexOf(pickUpSO);
    }


    private PickUpSO GetPickUpSOFromIndex(int pickUpSOIndex)
    {
        return pickUpSOListSO.pickUpSOList[pickUpSOIndex];
    }

    public override void OnDestroy()
    {
        /*
        NetworkManager.Singleton.OnClientConnectedCallback -= (ulong clientID) => { NetworkManager_OnClientConnectedCallback(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback -= (ulong clientID) => { NetworkManager_OnClientDisconnectedCallback(clientID); };

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        */

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
