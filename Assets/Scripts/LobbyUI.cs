using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBTN;
    [SerializeField] private Button createRoomBTN;
    [SerializeField] private Button quickJoinBTN;
    [SerializeField] private TMP_InputField roomCodeInputField;
    [SerializeField] private Button joinGameByCodeBTN;

    [SerializeField] private CreateRoomUI createRoomUI;
    [SerializeField] private AuthenticationUI authenticationUI;

    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;
    private void Awake()
    {

        mainMenuBTN.onClick.AddListener(() =>
        {
            
            SceneLoader.Load(SceneLoader.Scene.MainMenu);
        });

        createRoomBTN.onClick.AddListener(() =>
        {
            //LobbyManager.Instance.CreateLobby("LobbyName", false);
            //opens the create game ui
            createRoomUI.Show();
        });


        quickJoinBTN.onClick.AddListener(() =>
        {
            LobbyManager.Instance.QuickJoinLobby();
        });

        roomCodeInputField.onEndEdit.AddListener((string newRoomCode) =>
        {
            LobbyManager.Instance.lobbyCode = newRoomCode;
        });

        joinGameByCodeBTN.onClick.AddListener(() =>
        {
            //check if the target lobby has password
            //if hasPassword, new windowUI, enter password and connect

            //else just connect
            LobbyManager.Instance.JoinLobbyByCode(LobbyManager.Instance.lobbyCode);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
       // LobbyManager.Instance.OnJoinFailedPassword += (object sender, EventArgs e) => { Hide(); };

        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        /*
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Hide();
        }*/
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
    }

    
    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        //clean up
        foreach(Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        //putting in new
        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void OnDestory()
    {
        LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
    }
}
