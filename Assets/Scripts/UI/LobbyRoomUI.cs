using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomUI : MonoBehaviour
{
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private Transform playerTemplate;

    [SerializeField] private TMP_Text lobbyName;
    [SerializeField] private TMP_Text lobbyCode;
    [SerializeField] private TMP_Text serverStatusText;
    [SerializeField] private Button serverBTN;
    [SerializeField] private TMP_Text serverBTN_Text;
    [SerializeField] private Button leaveRoomBTN;

    
    private Lobby lobby;
    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.Instance.OnCreateLobbySuccess += LobbyManager_OnCreateLobbySuccess;

        LobbyManager.Instance.OnJoinLobbySuccess += LobbyManager_OnJoinLobbySuccess;

        LobbyManager.Instance.OnLobbyPollUpdate += LobbyManager_OnLobbyPollUpdate;

        serverBTN.onClick.AddListener(() =>
        {
            serverBTNPressed();
        });

        leaveRoomBTN.onClick.AddListener(() =>
        {
            //leave lobby, close lobbyroom ui
            LobbyManager.Instance.LeaveLobby();
            Hide();
        });

        playerTemplate.gameObject.SetActive(false);

        if(LobbyManager.Instance.joinedLobby != null)
        {
            DisplayLobbyData(LobbyManager.Instance.joinedLobby);
        }
        else
        {
            Hide();
        }
        
    }

    private void LobbyManager_OnCreateLobbySuccess(object sender, LobbyManager.OnLobbyEventArgs e)
    {
        DisplayLobbyData(e.lobby);
    }

    private void LobbyManager_OnJoinLobbySuccess(object sender, LobbyManager.OnLobbyEventArgs e)
    {
        DisplayLobbyData(e.lobby);

    }

    private void LobbyManager_OnLobbyPollUpdate(object sender, LobbyManager.OnLobbyEventArgs e)
    {
        UpdateLobbyRoom(e.lobby);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateLobbyRoom(Lobby updatedLobby)
    {
        //playerList
        //clean up
        lobby = updatedLobby;

        Debug.Log(playerListContainer);
        foreach (Transform child in playerListContainer)
        {
            if (child == playerTemplate) continue;
            Destroy(child.gameObject);
        }

        //putting in new
        foreach (Player player in lobby.Players)
        {
            Transform playerTransform = Instantiate(playerTemplate, playerListContainer);
            playerTransform.gameObject.SetActive(true);
            playerTransform.GetComponent<PlayerListSingleUI>().SetPlayer(player);
        }

        
        //Server Status:Running, NotRunning, InGame
        string serverStatus = lobby.Data[LobbyManager.KEY_SERVER_STATUS].Value;

        //check ServerManager.ServerStatus.... for cases
        switch(serverStatus)
        {
            
            case "NotRunning":
                serverStatusText.text = "Waiting For Host to Start Server...";

                if (AuthenticationService.Instance.PlayerId == lobby.HostId)
                {
                    serverBTN_Text.text = "Start Server";
                }
                else
                {
                    serverBTN_Text.text = "Wait...";
                }
                break;
            case "Running":
                serverStatusText.text = "Server Is Running";
                serverBTN_Text.text = "Join Server";
                break;
            case "InGame":
                serverStatusText.text = "Server Is Running: Game in Session...";
                serverBTN_Text.text = "Wait...";
                break;
        }
    }


    private void serverBTNPressed()
    {
        string serverStatus = lobby.Data[LobbyManager.KEY_SERVER_STATUS].Value;
        switch (serverStatus)
        {
            case "NotRunning":
                serverStatusText.text = "Waiting For Host to Start Server...";

                if (AuthenticationService.Instance.PlayerId == lobby.HostId)
                {
                    ServerManager.Instance.CreateRelay();
                    
                }
                else
                {
                    //message ui: server has not started
                    MessageUI.Instance.ShowMessage("Host has not started Server...", true);
                }
                break;
            case "Running":
                ServerManager.Instance.JoinRelay();
                break;
            case "InGame":
                //message ui: wait for game sesssion to end
                MessageUI.Instance.ShowMessage("Waiting for Game Session to End..." , true);
                break;
        }
    }

    private void DisplayLobbyData(Lobby lobby)
    {
        Show();
        
        lobbyName.text = lobby.Name;
        lobbyCode.text = lobby.LobbyCode;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnCreateLobbySuccess -= LobbyManager_OnCreateLobbySuccess;

        LobbyManager.Instance.OnJoinLobbySuccess -= LobbyManager_OnJoinLobbySuccess;

        LobbyManager.Instance.OnLobbyPollUpdate -= LobbyManager_OnLobbyPollUpdate;
    }
}
