using QFSW.QC;
using System;
using System.Collections;
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


public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    
    //lobby is not real time connection, its all about polling
    //lobby has automatic host migration, host leave, host chose at random
    private Lobby hostLobby;
    private Lobby joinedLobby;

    public string lobbyName;
    public string lobbyCode;
    public string password;
    public bool isPrivate;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float listLobbiesTimer;

    
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;
    public event EventHandler OnJoinFailedPassword;

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public class LobbyErrorMessageEventArgs : EventArgs
    {
        public LobbyServiceException e;
    }

    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        
        password = null;
        lobbyCode = null;

        
    }

    private void Start()
    {
        OnJoinFailedPassword += (object sender, EventArgs e) => { password = null; };
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
        HandlePeriodicListLobbies();
    }

    //use this function to run different profiles on the same pc
    

    //prevents going inactive
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    //lobby is not NGO, does not update automatically, must manually call for updates
    //theres a rate limit
    //such as recognizing players have joined
    private async void HandleLobbyPollForUpdates()
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
            }
        }
    }

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

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameMultiplayer.maxPlayerAmount - 1);

            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
        
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }catch (RelayServiceException e)
        {
            Debug.Log(e) ; 
            return default;
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
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Elimination"/*, DataObject.IndexOptions.S1) */)},
                    {"Map", new DataObject(DataObject.VisibilityOptions.Public, "SchoolRoom"/*, DataObject.IndexOptions.S1) */)}
                }
            };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;

            Debug.Log("Created Lobby!" + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayers(hostLobby);


            //allocate relay and start host and goes to the waitingroom\

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            GameMultiplayer.Instance.StartHost();
            
        }
        catch (LobbyServiceException e)
        {
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
            Debug.Log(e);
        }


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

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);



            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });

            Debug.Log("Lobbies found:" + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        }
        catch ( LobbyServiceException e)
        {

            Debug.Log(e);
        }
        
    }


    public async void JoinLobbyByCode(string newLobbyCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            lobbyCode = newLobbyCode;
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = AuthenticationManager.Instance.GetPlayer(),
                //input Password
                Password = "password" + password
            };

            Lobby lobby= await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("Joined Lobby with code: " + lobbyCode);

            PrintPlayers(joinedLobby);




            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
            //try to join, if needs password, then pop out password ui, enter password, and try again
            if (e.Reason == LobbyExceptionReason.IncorrectPassword)
            {
                OnJoinFailedPassword?.Invoke(this, EventArgs.Empty);
            }
            Debug.Log(e);
        }

    }

    public async void QuickJoinLobby()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            //Does quickJoin auto filter private and password rooms?
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();
        }
        catch(LobbyServiceException e)
        {
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
            //try to join, if needs password, then pop out password ui, enter password, and try again
            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                
            }
            Debug.Log(e);
        }
    }

    //this not really in use
    public async void JoinLobbyById(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = AuthenticationManager.Instance.GetPlayer(),
                //input Password
                Password = "password" + password
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);
            joinedLobby = lobby;
            

            Debug.Log("Joined Lobby with id: " + lobbyId);

            PrintPlayers(joinedLobby);




            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
            //try to join, if needs password, then pop out password ui, enter password, and try again
            if (e.Reason == LobbyExceptionReason.IncorrectPassword)
            {
                OnJoinFailedPassword?.Invoke(this, EventArgs.Empty);
            }
            Debug.Log(e);
        }

    }


    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value);
        foreach ( Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        //for when changing lobby data
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
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

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    private async void KickPlayer()
    {


        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void MigrateLobbyHost(string newHostId)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = newHostId
            });


            PrintPlayers(hostLobby);

            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void DeleteLobby()
    {
        try
        {
            LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    private void RefreshLobbyList()
    {

    }
}
