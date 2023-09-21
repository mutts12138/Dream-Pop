using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private TMP_InputField roomCodeInputField;
    [SerializeField] private Button JoinGameByCodeButton;
    

    [SerializeField] private CreateRoomUI createRoomUI;
    private void Awake()
    {

        mainMenuButton.onClick.AddListener(() =>
        {
            SceneLoader.Load(SceneLoader.Scene.MainMenu);
        });

        createRoomButton.onClick.AddListener(() =>
        {
            //LobbyManager.Instance.CreateLobby("LobbyName", false);
            //opens the create game ui
            createRoomUI.Show();
            Hide();
        });


        quickJoinButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.QuickJoinLobby();
        });

        roomCodeInputField.onEndEdit.AddListener((string newRoomCode) =>
        {
            LobbyManager.Instance.lobbyCode = newRoomCode;
        });

        JoinGameByCodeButton.onClick.AddListener(() =>
        {
            //check if the target lobby has password
            //if hasPassword, new windowUI, enter password and connect

            //else just connect
            LobbyManager.Instance.JoinLobbyByCode(LobbyManager.Instance.lobbyCode);
        });

        LobbyManager.Instance.OnJoinPassword += (object sender, EventArgs e) => { Hide(); };




        if (LobbyManager.Instance.playerName == null)
        {
            Hide();
        }
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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
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
    */
}
