using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;


    //lobby is not real time connection, its all about polling
    //lobby has automatic host migration, host leave, host chose at random
    private bool isLobbyHost = false;
    public Lobby hostLobby { get; private set; }
    public Lobby joinedLobby { get; private set; }


    public string lobbyName;
    public string lobbyCode;
    public string password;
    public bool isPrivate;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float listLobbiesTimer;

    public const string KEY_SERVER_STATUS = "ServerStatus";
    public const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public const string KEY_GAMEMODE = "GameMode";
    public const string KEY_MAP = "Map";


    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler<OnLobbyEventArgs> OnCreateLobbySuccess;
    public event EventHandler<OnFailReasonEventArgs> OnCreateLobbyFailed;

    public event EventHandler OnJoinLobbyStarted;
    public event EventHandler<OnLobbyEventArgs> OnJoinLobbySuccess;
    public event EventHandler<OnFailReasonEventArgs> OnJoinLobbyFailed;



    public class OnFailReasonEventArgs : EventArgs
    {
        public string failReason;
    }


    public event EventHandler<OnLobbyListEventArgs> OnLobbyListChanged;
    public class OnLobbyListEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public event EventHandler<OnLobbyEventArgs> OnLobbyPollUpdate;
    public class OnLobbyEventArgs : EventArgs
    {
        public Lobby lobby;
        
    }

    private void Awake()
    {
        //rejected: waitroomManager renews lobby manager after leaving waitroom scene.
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
    }

    private void Start()
    {
        

        
    }


    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
        HandlePeriodicListLobbies();
        
    }

    
    

    //for host, prevents lobby from going inactive
    private async void HandleLobbyHeartbeat()
    {
        try
        {
            if (hostLobby != null)
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer < 0f)
                {
                    float heartbeatTimerMax = 15;
                    heartbeatTimer = heartbeatTimerMax;

                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                    
                }
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //REPLACE THIS WITH SUBSCRIBETOLOBBYEVENTCHANGES?
    //lobby is not NGO, does not update automatically, must manually call for updates
    //theres a rate limit
    //lobby updates for the lobby that the player joined
    private async void HandleLobbyPollForUpdates()
    {
        try
        {
            if (joinedLobby != null)
            {
                lobbyUpdateTimer -= Time.deltaTime;
                if (lobbyUpdateTimer < 0f)
                {
                    float lobbyUpdateTimerMax = 1.1f;
                    lobbyUpdateTimer = lobbyUpdateTimerMax;

                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                   
                    joinedLobby = lobby;
                    
                    CheckDidHostAutoMigrateToPlayer(lobby);

                    OnLobbyPollUpdate?.Invoke(this, new OnLobbyEventArgs
                    {
                        lobby = lobby,
                    });

                    
                }
            }
        }catch(LobbyServiceException e)
        {
            Debug.Log("poll for update error");
        }
    }

    
    private void CheckDidHostAutoMigrateToPlayer(Lobby lobby)
    {
        if (isLobbyHost == false)
        {
            if (AuthenticationService.Instance.PlayerId == lobby.HostId)
            {
                hostLobby = lobby;
                isLobbyHost = true;
                //reset relay code and server status
                UpdateRelayServerCode(null);
                UpdateServerStatus("NotRunning");
                
            }
            else
            {
                hostLobby = null;
            }
        }
        
    }

    //update lobby list when player has not joined a lobby
    private void HandlePeriodicListLobbies()
    {
        if(joinedLobby == null 
            && AuthenticationService.Instance.IsSignedIn 
            && SceneManager.GetActiveScene().name == SceneLoader.Scene.Lobby.ToString())
        {
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer <= 0f)
            {
                float listLobbiesTimerMax = 3f;
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }

    }

    

    //command is for calling in console by entering function name
    //lobby is listed on the unity lobby service server
    //lobby inactive after 30secs of no update data
    [Command]
    public async void CreateLobby(string lobbyName, bool isPrivate, string password)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            this.lobbyName = lobbyName;
            if (password != null)
            {
                //if its private, it will have a password, added "password" to remove 8 character limitation
                this.password = "password" + password;
            }
            else
            {
                //public lobby has null/no password
                this.password = password;
            }
            this.isPrivate = isPrivate;
            int maxPlayers = 8;

            //create lobby settings
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = AuthenticationManager.Instance.GetPlayer(),
                Password = this.password,
                //assignning custom data to the lobby
                Data = new Dictionary<string, DataObject>
                {
                    //WaitingRoomStatus: "Not Created", "Created", "InGame"
                    {KEY_SERVER_STATUS, new DataObject(DataObject.VisibilityOptions.Public, "NotRunning") },
                    {KEY_GAMEMODE, new DataObject(DataObject.VisibilityOptions.Public, "Elimination"/*, DataObject.IndexOptions.S1) */)},
                    {KEY_MAP, new DataObject(DataObject.VisibilityOptions.Public, "SchoolRoom"/*, DataObject.IndexOptions.S1) */)}
                }
            };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            isLobbyHost = true;
            hostLobby = lobby;
            joinedLobby = lobby;
            lobbyCode = lobby.LobbyCode;
            isPrivate = lobby.IsPrivate;
            Debug.Log("Created Lobby!" + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayers(hostLobby);

            
            OnCreateLobbySuccess?.Invoke(this, new OnLobbyEventArgs
            {
                lobby = joinedLobby,
            });

        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LengthRequired)
            {
                Debug.Log(e.Message);
            }
                
            string failReason = e.Reason.ToString();
            OnCreateLobbyFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = failReason});
           
        }


    }

    public enum ServerStatus
    {
        NotRunning,
        Running,
        Running_InGame
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
            //filters, and order,see implementation, can have custom filters
            //use UI to set filters and orders later
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    //filter via slot avaliable
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),//GT is greater than

                    //usage of custom filter, S1
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "Elimination", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    //sort via time created
                    new QueryOrder (false, QueryOrder.FieldOptions.Created)
                }

            };
            //

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);



            OnLobbyListChanged?.Invoke(this, new OnLobbyListEventArgs
            {
                lobbyList = queryResponse.Results
            });

            Debug.Log("Lobbies found:" + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data[KEY_GAMEMODE].Value);
            }
        }
        catch ( LobbyServiceException e)
        {

            Debug.Log(e);
        }
        
    }


    public async void JoinLobbyByCode(string newLobbyCode)
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            lobbyCode = newLobbyCode;
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = AuthenticationManager.Instance.GetPlayer(),
                //input Password
                Password = "password" + password
            };

            Lobby lobby= await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            isLobbyHost = false;
            lobbyCode = lobby.LobbyCode;
            isPrivate = lobby.IsPrivate;

            Debug.Log("Joined Lobby with code: " + lobbyCode);

            PrintPlayers(joinedLobby);

            OnJoinLobbySuccess?.Invoke(this, new OnLobbyEventArgs
            {
                lobby = joinedLobby,
            });
  
        }
        catch (LobbyServiceException e)
        {
            password = null;
            string failReason = e.Reason.ToString();
            OnJoinLobbyFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = failReason });
            Debug.Log(e);
            
        }

    }

    public async void QuickJoinLobby()
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            //Does quickJoin auto filter private and password rooms?
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            isLobbyHost = false;
            lobbyCode = joinedLobby.LobbyCode;
            isPrivate = joinedLobby.IsPrivate;
            OnJoinLobbySuccess?.Invoke(this, new OnLobbyEventArgs
            {
                lobby = joinedLobby,
            });
 
        }
        catch(LobbyServiceException e)
        {

            password = null;
            string failReason = e.Reason.ToString();
            OnJoinLobbyFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = failReason });
            
        }
    }

    //this not really in use
    public async void JoinLobbyById(string lobbyId)
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = AuthenticationManager.Instance.GetPlayer(),
                //input Password
                Password = "password" + password
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);
            isLobbyHost = false;
            joinedLobby = lobby;
            lobbyCode = lobby.LobbyCode;
            isPrivate = lobby.IsPrivate;

            Debug.Log("Joined Lobby with id: " + lobbyId);

            PrintPlayers(joinedLobby);

            OnJoinLobbySuccess?.Invoke(this, new OnLobbyEventArgs
            {
                lobby = joinedLobby,
            });
            
        }
        catch (LobbyServiceException e)
        {
            password = null;
            string failReason = e.Reason.ToString();
            OnJoinLobbyFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = failReason });
            Debug.Log(e);
            
        }

    }


    /*
    //This is used primarily as notification: poll update doesnt notify you that a player has joined or tell you what had changed
    //but eventcallback will tell you what changes has made and trigger events based on it

    private async void SubscribeToLobbyEventsAsync()
    {
        var callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        callbacks.KickedFromLobby += OnKickedFromLobby;
        callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
        try
        {
            m_LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(m_Lobby.Id, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{m_Lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                default: throw;
            }
        }
    }

    private void OnLobbyChanged(ChangedLobbyValue<int> changes)
    {
        if (changes.LobbyDeleted)
        {
            // Handle lobby being deleted
            // Calling changes.ApplyToLobby will log a warning and do nothing
        }
        else
        {
            changes.ApplyToLobby(m_Lobby);
        }
        // Refresh the UI in some way
    }

    private void OnKickedFromLobby()
    {


        // These events will never trigger again, so let’s remove it.
        this.m_LobbyEvents = null;
        // Refresh the UI in some way
    }
    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed: //Update the UI if necessary, as the subscription has been stopped.  
                break;
            case LobbyEventConnectionState.Subscribing: // Update the UI if necessary, while waiting to be subscribed.  
                break;
            case LobbyEventConnectionState.Subscribed: // Update the UI if necessary, to show subscription is working. 
                break;
            case LobbyEventConnectionState.Unsynced: // Update the UI to show connection problems. Lobby will attempt to reconnect automatically. 
                break;
            case LobbyEventConnectionState.Error: // Update the UI to show the connection has errored. Lobby will not attempt to reconnect as something has gone wrong.
        }
    }
    private void OnLobbyChanged(ILobbyChanges changes)
    {
        changes.ApplyToLobby(m_Lobby);
        // Refresh the UI in some way
    }
    */






    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " + lobby.Name + " " + lobby.Data[KEY_GAMEMODE].Value);
        foreach ( Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    public async void UpdateServerStatus(string newServerStatus)
    {
        //for when changing lobby data
        if (hostLobby == null)
        {
            Debug.Log("cant update server status lobby info because hostLobby is empty");
        }
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_SERVER_STATUS, new DataObject(DataObject.VisibilityOptions.Public, newServerStatus) }
                }
            });

            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdateRelayServerCode(string newRelayCode)
    {
        if (hostLobby == null)
        {
            Debug.Log("cant update server status lobby info because hostLobby is empty");
        }
        try
        {
            
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, newRelayCode) }
                }
            });

            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        //for when changing lobby data
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { KEY_GAMEMODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
            }
            });


            PrintPlayers(hostLobby);

            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    //whats the use of this?
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            AuthenticationManager.Instance.playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationManager.Instance.playerName)}
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    //both host and client use this to return to lobby
    public void ReturnToLobbyRoom()
    {
        SceneLoader.Load(SceneLoader.Scene.Lobby);
        //only when server is shutdown can you return to lobby room, so reset server code and server status
        if(isLobbyHost)
        {
            UpdateRelayServerCode(null);
            UpdateServerStatus("NotRunning");
        }
        
    }

    public async void LeaveLobby()
    {
        try
        {
            
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            

            joinedLobby = null;
            hostLobby = null;
            lobbyName = null;
            lobbyCode = null;
            password = null;
}
        catch (LobbyServiceException e)
        {
            Debug.Log("Leave Lobby exception");
            
        }
        
    }

    private async void KickPlayer(string targetPlayerId)
    {
        
        try
        {
            
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, targetPlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    
    //host calls this manually to set another player as host
    private async void MigrateLobbyHost(string newHostId)
    {
        try
        {
            UpdateRelayServerCode(null);
            UpdateServerStatus("NotRunning");

            Lobby newHostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = newHostId
            });


            PrintPlayers(hostLobby);
            isLobbyHost = false;
            hostLobby = null;
            joinedLobby = newHostLobby;
            //reset server code and status
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    private void RefreshLobbyList()
    {

    }

    







    void OnApplicationQuit()
    {
        if(joinedLobby != null)
        {
            LeaveLobby();
        }
        
    }

}
