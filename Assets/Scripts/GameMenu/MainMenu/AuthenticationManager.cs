using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance;
    // Start is called before the first frame update
    public event EventHandler OnInvalidProfileName;
    public event EventHandler OnInvalidPlayerState;
    public event EventHandler OnAuthenticationSuccess;
    public event EventHandler OnAuthenticationFailed;

    public string playerName { get;  set; }
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

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void Authenticate(string playerName)
    {
        try
        {
            //Debug.Log("this is called");
            InitializationOptions InitializationOptions = new InitializationOptions();


            //this is creating a profile, its profile name, not player name
            InitializationOptions.SetProfile(playerName);

            await UnityServices.InitializeAsync(InitializationOptions);

            AuthenticationService.Instance.SignedIn += () =>
            {
                //do nothing
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            OnAuthenticationSuccess?.Invoke(this, EventArgs.Empty);

            //set the localplayerdata
            LocalPlayerData.Instance.UpdateLocalPlayerData(new PlayerData(AuthenticationService.Instance.PlayerId));
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e.ErrorCode);

            //when player name doesnt match the requirement
            if (e.ErrorCode == AuthenticationErrorCodes.ClientInvalidProfile)
            {
                OnInvalidProfileName?.Invoke(this, EventArgs.Empty);
                
                return;
            }

            //when player try to sign in when theres already a profile signed in
            //or when player try to sign out when no profile exists
            if (e.ErrorCode == AuthenticationErrorCodes.ClientInvalidUserState)
            {
                OnInvalidPlayerState?.Invoke(this, EventArgs.Empty);
                return;
            }

            OnAuthenticationFailed?.Invoke(this, EventArgs.Empty);


        }
    }


    [Command]
    private void PrintCurrentAuthenticateProfile()
    {
        Debug.Log(AuthenticationService.Instance.Profile);
    }

    public Player GetPlayer()
    {
        return new Player
        {
            //Id = AuthenticationService.Instance.PlayerIdId,
            //use the dictionary to store whatever data u want, player characters, loadouts, etc.
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
        };
    }

}
