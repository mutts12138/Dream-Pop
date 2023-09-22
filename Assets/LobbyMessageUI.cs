using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;
    [SerializeField] Button closeBTN;
    
    private void Awake()
    {
        closeBTN.onClick.AddListener(() =>
        {
            Hide();
        });

        
    }

    private void Start()
    {
        //sub events

        AuthenticationManager.Instance.OnInvalidProfileName += LobbyManager_OnInvalidProfileName;
        AuthenticationManager.Instance.OnInvalidPlayerState += LobbyManager_OnInvalidPlayerState;
        AuthenticationManager.Instance.OnAuthenticationSuccess += LobbyManager_OnAuthenticationSuccess;
        AuthenticationManager.Instance.OnAuthenticationFailed += LobbyManager_OnAuthenticationFailed;

        GameMultiplayer.Instance.OnTryingToJoinGame += GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed += LobbyManager_OnCreateLobbyFailed;

        LobbyManager.Instance.OnJoinStarted += LobbyManager_OnJoinStarted;
        LobbyManager.Instance.OnJoinFailed += LobbyManager_OnJoinFailed;
        LobbyManager.Instance.OnQuickJoinFailed += LobbyManager_OnQuickJoinFailed;


       
        Hide();
    }

    private void LobbyManager_OnInvalidPlayerState(object sender, System.EventArgs e)
    {
        if(AuthenticationService.Instance.Profile != null)
        {
            ShowMessage("A player profile already exist");
        }
        else
        {
            ShowMessage("player profile doesn't exist");
        } 
    }

    private void LobbyManager_OnAuthenticationFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Authentication Failed");
    }

    private void LobbyManager_OnAuthenticationSuccess(object sender, System.EventArgs e)
    {
        ShowMessage("Authentication Success");
    }

    private void LobbyManager_OnInvalidProfileName(object sender, System.EventArgs e)
    {
        ShowMessage("Player name may only contain alphanumeric values, '-', '_', and must be no longer than 30 characters.");
    }

    private void LobbyManager_OnJoinStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void LobbyManager_OnQuickJoinFailed(object sender, System.EventArgs e)
    {
        Debug.Log("quick join failed");
        ShowMessage("Could not find a lobby to quick join");
    }

    private void LobbyManager_OnJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join lobby");
    }

    private void LobbyManager_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to create lobby");
    }

    private void LobbyManager_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void GameMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        ShowMessage("connecting...");
    }


    private void GameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if(NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Failed to connect");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
              
    }



    public void ShowMessage(string message)
    {
        Show();
        messageText.text = message;  
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
        AuthenticationManager.Instance.OnInvalidProfileName -= LobbyManager_OnInvalidProfileName;
        AuthenticationManager.Instance.OnInvalidPlayerState -= LobbyManager_OnInvalidPlayerState;
        AuthenticationManager.Instance.OnAuthenticationSuccess -= LobbyManager_OnAuthenticationSuccess;
        AuthenticationManager.Instance.OnAuthenticationFailed -= LobbyManager_OnAuthenticationFailed;

        GameMultiplayer.Instance.OnTryingToJoinGame -= GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted -= LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed -= LobbyManager_OnCreateLobbyFailed;

        LobbyManager.Instance.OnJoinStarted -= LobbyManager_OnJoinStarted;
        LobbyManager.Instance.OnJoinFailed -= LobbyManager_OnJoinFailed;
        LobbyManager.Instance.OnQuickJoinFailed -= LobbyManager_OnQuickJoinFailed;

    }
}
