using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;

    public static int maxPlayerAmount { get; private set; }

    public event EventHandler OnCreateServerStarted;
    public event EventHandler OnCreateServerSuccess;
    public event EventHandler<OnFailReasonEventArgs> OnCreateServerFailed;

    public event EventHandler OnJoinServerStarted;
    public event EventHandler OnJoinServerSuccess;
    public event EventHandler<OnFailReasonEventArgs> OnJoinServerFailed;
    public class OnFailReasonEventArgs : EventArgs
    {
        public string failReason;
    }

    public bool IsVoluntaryDisconnect;

    public event EventHandler OnServerStoppedInvoluntary;
    public event EventHandler OnClientStoppedInvoluntary;

    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        maxPlayerAmount = 8;
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;

        
    }
    
    

    //calls when lobby created, lobby host migrated
    public async void CreateRelay()
    {    
        //load the loading scene
        SceneLoader.Load(SceneLoader.Scene.Loading);
        try
        {
            //allocate relay and start host and goes to the waitingroom\

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            //its gonna update anyway by LobbyPollUpdate, so OnCreateServerSuccess does not need
            
            NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            //ServerManager.Instance.StartHost();
            //Pass in the connection data to lobby data
            LobbyManager.Instance.UpdateRelayServerCode(relayJoinCode);
            StartHost();

        }
        catch (RelayServiceException e)
        {
            string failReason = e.Reason.ToString();
            OnCreateServerFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = failReason });
        }

    }

    //calls when joinging the lobby
    public async void JoinRelay()
    {
        //load the loading scene
        SceneLoader.Load(SceneLoader.Scene.Loading);
        try
        {
            

            string relayJoinCode = LobbyManager.Instance.joinedLobby.Data[LobbyManager.KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            //ServerManager.Instance.StartClient();
            StartClient();
        }
        catch (RelayServiceException e)
        {
            string failReason = e.Reason.ToString();
            OnJoinServerFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = failReason });
        }



    }

    
    /////////////////////////////////////////
    
    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayerAmount - 1);
            

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
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }
    
    //////////////////////////////////
    
    public void StartHost()
    {
        OnCreateServerStarted?.Invoke(this, EventArgs.Empty);
        IsVoluntaryDisconnect = false;
        NetworkManager.Singleton.OnServerStopped += NetworkManager_OnServerStopped;

        
        if (NetworkManager.Singleton.StartHost())
        {

            OnCreateServerSuccess?.Invoke(this, EventArgs.Empty);
            LobbyManager.Instance.UpdateServerStatus(ServerStatus.Running.ToString());


            SceneLoader.LoadNetwork(SceneLoader.Scene.WaitingRoom);
        }
        else
        {
            OnCreateServerFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = "Start Host failed" });
            
        }

    }

    

    public void StartClient()
    {
        //put password here
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(LobbyManager.Instance.password);
        OnJoinServerStarted?.Invoke(this, EventArgs.Empty);
        IsVoluntaryDisconnect = false;
        NetworkManager.Singleton.OnClientStopped += NetworkManager_OnClientStopped;


        //startclient bool happens instantly, but takes time for netcode to connect.
        if (NetworkManager.Singleton.StartClient())
        {
            

            
            OnJoinServerSuccess?.Invoke(this, EventArgs.Empty);
            
        }
        else
        {
            OnJoinServerFailed?.Invoke(this, new OnFailReasonEventArgs { failReason = "Start Client failed" });
        }
        //SceneLoader.LoadNetwork(SceneLoader.Scene.WaitingRoom);
    }

    

    public enum ServerStatus
    {
        NotRunning,
        Running,
        InGame
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
        //response.Position = new Vector3(0, 10, 0);
        //response.CreatePlayerObject = true;
    }

    private void NetworkManager_OnServerStopped(bool obj)
    {
        NetworkManager.Singleton.OnServerStopped -= NetworkManager_OnServerStopped;
        
        //if leave button is pressed
        if (IsVoluntaryDisconnect)
        {
            //manual: caused by host pressing the leave button
            //returns to lobby, update lobby data
            LobbyManager.Instance.ReturnToLobbyRoom();
        }
        else
        {
            //auto: caused by lost of internet connection, application quits, or power outrage
            //nothing really happens?
            OnServerStoppedInvoluntary?.Invoke(this, EventArgs.Empty);
            LobbyManager.Instance.ReturnToLobbyRoom();
        }


    }

    private void NetworkManager_OnClientStopped(bool obj)
    {
        NetworkManager.Singleton.OnClientStopped -= NetworkManager_OnClientStopped;
        
        //if leave button is pressed
        if ( IsVoluntaryDisconnect )
        {
            //Manual: caused by client pressing the leave button
            //returns to lobby
            LobbyManager.Instance.ReturnToLobbyRoom();
        }
        else
        {
            //auto: caused by host stopping the server
            //pop out message to return to lobby
            OnClientStoppedInvoluntary?.Invoke(this, EventArgs.Empty);
            LobbyManager.Instance.ReturnToLobbyRoom();
        }
    }





}
