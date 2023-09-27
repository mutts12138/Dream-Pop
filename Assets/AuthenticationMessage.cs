using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class AuthenticationMessage : MonoBehaviour
{

    private void Awake()
    {

    }

    private void Start()
    {
        //sub events

        AuthenticationManager.Instance.OnInvalidProfileName += AuthenticationManager_OnInvalidProfileName;
        AuthenticationManager.Instance.OnInvalidPlayerState += AuthenticationManager_OnInvalidPlayerState;
        AuthenticationManager.Instance.OnAuthenticationSuccess += AuthenticationManager_OnAuthenticationSuccess;
        AuthenticationManager.Instance.OnAuthenticationFailed += AuthenticationManager_OnAuthenticationFailed;
    }

    private void AuthenticationManager_OnInvalidPlayerState(object sender, System.EventArgs e)
    {
        if (AuthenticationService.Instance.Profile != null)
        {
            SetMessage("A player profile already exist");
        }
        else
        {
            SetMessage("player profile doesn't exist");
        }
    }

    private void AuthenticationManager_OnAuthenticationFailed(object sender, System.EventArgs e)
    {
        SetMessage("Authentication Failed");
    }

    private void AuthenticationManager_OnAuthenticationSuccess(object sender, System.EventArgs e)
    {
        SetMessage("Authentication Success");
    }

    private void AuthenticationManager_OnInvalidProfileName(object sender, System.EventArgs e)
    {
        SetMessage("Player name may only contain alphanumeric values, '-', '_', and must be no longer than 30 characters.");
    }




    public void SetMessage(string message)
    {
        MessageUI.Instance.ShowMessage(message, true);
    }


    private void OnDestroy()
    {
        AuthenticationManager.Instance.OnInvalidProfileName -= AuthenticationManager_OnInvalidProfileName;
        AuthenticationManager.Instance.OnInvalidPlayerState -= AuthenticationManager_OnInvalidPlayerState;
        AuthenticationManager.Instance.OnAuthenticationSuccess -= AuthenticationManager_OnAuthenticationSuccess;
        AuthenticationManager.Instance.OnAuthenticationFailed -= AuthenticationManager_OnAuthenticationFailed;
 

    }
}
