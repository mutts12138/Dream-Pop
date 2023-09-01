using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class LobbyUI : MonoBehaviour
{
    
    [SerializeField] private Button StartGameBTN;

    [SerializeField] private TextMeshProUGUI player1Text;
    [SerializeField] private TextMeshProUGUI player2Text;
    [SerializeField] private TextMeshProUGUI player3Text;
    [SerializeField] private TextMeshProUGUI player4Text;
    [SerializeField] private TextMeshProUGUI player5Text;
    [SerializeField] private TextMeshProUGUI player6Text;
    [SerializeField] private TextMeshProUGUI player7Text;
    [SerializeField] private TextMeshProUGUI player8Text;

    [SerializeField] private Button team1BTN;
    [SerializeField] private Button team2BTN;

    private LobbyManager lobbyManager;
    // Start is called before the first frame update
    void Awake()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();

        
        StartGameBTN.onClick.AddListener(() =>
        {
            lobbyManager.LoadGameScene();
        });

        team1BTN.onClick.AddListener(() =>
        {
            SetPlayerTeamNumberServerRpc(1);
        });

        team2BTN.onClick.AddListener(() =>
        {
            SetPlayerTeamNumberServerRpc(2);
        });
    }

    private void Start()
    {
        //subscribe lobbylist change delegate
        PlayerDataManager.Instance.GetPlayerConnectedList().OnListChanged += (NetworkListEvent<PlayerData> changeEvent) => { UpdatePlayerLobby(); };
    }

    private void OnDisable()
    {
        //do i need to unsub startgamebtn.conclick?
        StartGameBTN.onClick.RemoveAllListeners();
        team1BTN.onClick.RemoveAllListeners();
        team2BTN.onClick.RemoveAllListeners();
        PlayerDataManager.Instance.GetPlayerConnectedList().OnListChanged -= (NetworkListEvent<PlayerData> changeEvent) => { UpdatePlayerLobby(); };
    }

    private void UpdatePlayerLobby()
    {
        int index = 0;
        foreach (PlayerData player in PlayerDataManager.Instance.GetPlayerConnectedList())
        {
            if (player.isConnected == true)
            {
                switch (index)
                {
                    case 0:
                        player1Text.text = player.clientID.ToString();
                        break;
                    case 1:
                        player2Text.text = player.clientID.ToString();
                        break;
                    case 2:
                        player3Text.text = player.clientID.ToString();
                        break;
                    case 3:
                        player4Text.text = player.clientID.ToString();
                        break;
                    case 4:
                        player5Text.text = player.clientID.ToString();
                        break;
                    case 5:
                        player6Text.text = player.clientID.ToString();
                        break;
                    case 6:
                        player7Text.text = player.clientID.ToString();
                        break;
                    case 7:
                        player8Text.text = player.clientID.ToString();
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        player1Text.text = "empty";
                        break;
                    case 1:
                        player2Text.text = "empty";
                        break;
                    case 2:
                        player3Text.text = "empty";
                        break;
                    case 3:
                        player4Text.text = "empty";
                        break;
                    case 4:
                        player5Text.text = "empty";
                        break;
                    case 5:
                        player6Text.text = "empty";
                        break;
                    case 6:
                        player7Text.text = "empty";
                        break;
                    case 7:
                        player8Text.text = "empty";
                        break;
                }
            }

            index++;
            
        }
        
            
    }

    private void SetPlayerTeamNumberServerRpc(int teamNumber)
    {
        Player player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
        if (player == null)
        {
            Debug.Log("cant set team Number, player is null");
            return;
        }

        player.SetTeamNumberServerRpc(teamNumber);
    }
    
}
