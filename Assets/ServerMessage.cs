using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;
using static ServerManager;

public class ServerMessage : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        ServerManager.Instance.OnCreateServerStarted += ServerManager_OnCreateServerStarted;
        ServerManager.Instance.OnCreateServerSuccess += ServerManager_OnCreateServerSuccess;
        ServerManager.Instance.OnCreateServerFailed += (object sender, OnFailReasonEventArgs e) => { ServerManager_OnCreateServerFailed(sender, e); };
        

        ServerManager.Instance.OnJoinServerStarted += ServerManager_OnJoinServerStarted;
        ServerManager.Instance.OnJoinServerSuccess += ServerManager_OnJoinServerSuccess;
        ServerManager.Instance.OnJoinServerFailed += (object sender, OnFailReasonEventArgs e) => { ServerManager_OnJoinServerFailed(sender, e); };

        ServerManager.Instance.OnServerStoppedInvoluntary += ServerManager_OnServerStoppedNotVoluntary;
        ServerManager.Instance.OnClientStoppedInvoluntary += ServerManager_OnClientStoppedNotVoluntary;
    }

    

    private void ServerManager_OnCreateServerStarted(object sender, EventArgs e)
    {
        MessageUI.Instance.ShowMessage("Creating Server...", false);
    }
    private void ServerManager_OnCreateServerSuccess(object sender, EventArgs e)
    {
        MessageUI.Instance.Hide();

        
    }

    private void ServerManager_OnCreateServerFailed(object sender, OnFailReasonEventArgs e)
    {
        MessageUI.Instance.ShowMessage("Failed to create server" + ": " + e.failReason, true);
        MessageUI.Instance.ShowCloseBTN();
    }



    private void ServerManager_OnJoinServerStarted(object sender, EventArgs e)
    {
        MessageUI.Instance.ShowMessage("Joining Server...", false);
    }
    private void ServerManager_OnJoinServerSuccess(object sender, EventArgs e)
    {
        //close the messageui window
        MessageUI.Instance.Hide();

        
    }
    private void ServerManager_OnJoinServerFailed(object sender, OnFailReasonEventArgs e)
    {
        MessageUI.Instance.ShowMessage("Failed to Join Server" + ": " + e.failReason, true);
        MessageUI.Instance.ShowCloseBTN();
    }

    


    private void ServerManager_OnServerStoppedNotVoluntary(object sender, EventArgs e)
    {
        
        
        MessageUI.Instance.ShowMessage("disconnected", true);
    }

    private void ServerManager_OnClientStoppedNotVoluntary(object sender, EventArgs e)
    {
        
        
        MessageUI.Instance.ShowMessage("disconnected", true);
    }

    






    private void OnDestroy()
    {
        

    }
}
