using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static LobbyManager;

public class LobbyMessage : MonoBehaviour
{
    private void Awake()
    {
        
    }

    private void Start()
    {

        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbySuccess += (object sender, OnLobbyEventArgs e) => { LobbyManager_OnCreateLobbySuccess(sender, e); };
        LobbyManager.Instance.OnCreateLobbyFailed += (object sender, OnFailReasonEventArgs e) => { LobbyManager_OnCreateLobbyFailed(sender, e); };
        

        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbySuccess += (object sender, OnLobbyEventArgs e) => { LobbyManager_OnJoinLobbySuccess(sender, e); };
        LobbyManager.Instance.OnJoinLobbyFailed += (object sender, OnFailReasonEventArgs e) => { LobbyManager_OnJoinLobbyFailed(sender, e); };
        
    }


    //create lobby
    private void LobbyManager_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        
        MessageUI.Instance.ShowMessage("Creating Lobby...", false);
    }

    private void LobbyManager_OnCreateLobbySuccess(object sender, OnLobbyEventArgs e)
    {
        //close the messageui window
        MessageUI.Instance.Hide();
        
    }
    private void LobbyManager_OnCreateLobbyFailed(object sender, OnFailReasonEventArgs e)
    {
        MessageUI.Instance.ShowMessage("Failed to create lobby" + ": " + e.failReason, true);
        MessageUI.Instance.ShowCloseBTN();
    }

    //join lobby
    private void LobbyManager_OnJoinLobbyStarted(object sender, System.EventArgs e)
    {
        
        MessageUI.Instance.ShowMessage("Joining Lobby...", false);
    }

    private void LobbyManager_OnJoinLobbySuccess(object sender, OnLobbyEventArgs e)
    {
        //close the messageui window
        MessageUI.Instance.Hide();
    }

    private void LobbyManager_OnJoinLobbyFailed(object sender, OnFailReasonEventArgs e)
    {
        if (e.failReason == LobbyExceptionReason.IncorrectPassword.ToString())
        {
            Debug.Log(e.failReason + " == " + LobbyExceptionReason.IncorrectPassword.ToString());
            MessageUI.Instance.ShowMessage("Enter correct password.",true);
            MessageUI.Instance.ShowCloseBTN();
            PasswordUI.Instance.Show();
        }
        else
        {
            MessageUI.Instance.ShowMessage("Failed to Join lobby" + ": " + e.failReason, true);
            MessageUI.Instance.ShowCloseBTN();
        }


        
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnCreateLobbyStarted -= LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbySuccess -= (object sender, OnLobbyEventArgs e) => { LobbyManager_OnCreateLobbySuccess(sender, e); };
        LobbyManager.Instance.OnCreateLobbyFailed -= (object sender, OnFailReasonEventArgs e) => { LobbyManager_OnCreateLobbyFailed(sender, e); };


        LobbyManager.Instance.OnJoinLobbyStarted -= LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbySuccess -= (object sender, OnLobbyEventArgs e) => { LobbyManager_OnJoinLobbySuccess(sender, e); };
        LobbyManager.Instance.OnJoinLobbyFailed -= (object sender, OnFailReasonEventArgs e) => { LobbyManager_OnJoinLobbyFailed(sender, e); };

    }
}
