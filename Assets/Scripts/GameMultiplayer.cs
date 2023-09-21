using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkManager;

public class GameMultiplayer : NetworkBehaviour
{
    //handles the multiplayer aspect of the game: example spawning objects
    public static GameMultiplayer Instance { get; private set; }


    //[SerializeField] private DreamBubbleObjectListSO dreamBubbleObjectListSO;
    //[SerializeField] private BuffObjectListSO BuffObjectListSO;
    // [SerializeField] private PickUpObjectListSO PickUpObjectListSO;


    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;


    public event EventHandler OnPlayerDataNetworkListChanged;

    //migrate playerdatamanger to here
    public static int maxPlayerAmount {get; private set;}
    private NetworkList<PlayerData> PlayerDataNetworkList;

    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        maxPlayerAmount = 8;
        PlayerDataNetworkList = new NetworkList<PlayerData>(new PlayerData[0], NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;


        

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void StartHost()
    {   
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { OnNetworkManager_OnClientConnectedCallBack(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { OnNetworkManager_OnClientDisconnectedCallBack(clientID); };

        NetworkManager.Singleton.StartHost();
        SceneLoader.LoadNetwork(SceneLoader.Scene.WaitingRoom);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //generate conditions to approve connection, game in progress, etc. max player is set by lobby.
        //get active scene to compare to loader.scene.selected scene
        if (NetworkManager.Singleton.IsHost)
        {
            //if is host then no check on approval connection conditions
        }
        else
        {
            if (SceneManager.GetActiveScene().name != SceneLoader.Scene.WaitingRoom.ToString())
            {
                response.Approved = false;
                response.Reason = "Game in progress";
                return;
            }
            //compare to max player
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= maxPlayerAmount)
            {
                response.Approved = false;
                response.Reason = "max player reached";
                return;
            }

            /*
            if(LobbyManager.Instance.password == Encoding.ASCII.GetString(request.Payload))
            {
                response.Approved = false;
                response.Reason = "password incorrect";
                return;
            }*/
        }
        
        

        response.Approved = true;
        response.Position = new Vector3(0, 10, 0);
        response.CreatePlayerObject = true;
        


    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        //put password here
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(LobbyManager.Instance.password);

        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => { OnNetworkManager_OnClientConnectedCallBack(clientID); };
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) => { OnNetworkManager_OnClientDisconnectedCallBack(clientID); };
        NetworkManager.Singleton.StartClient();
        //SceneLoader.LoadNetwork(SceneLoader.Scene.WaitingRoom);
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    private void OnNetworkManager_OnClientConnectedCallBack(ulong clientID)
    {
        if (!IsServer) return;
        AddNewPlayerToPlayerConnectedList(clientID);
    }

    private void OnNetworkManager_OnClientDisconnectedCallBack(ulong clientID)
    {
        if (!IsServer) return;
        RemovePlayerFromPlayerConnectedList(clientID);

        if (!IsServer)
        {
            Debug.Log($"Approval Declined Reason: {NetworkManager.Singleton.DisconnectReason}");
        }
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
