using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class WaitingRoomUI : MonoBehaviour
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

    [SerializeField] private Button leaveBTN;
    // Start is called before the first frame update
    void Awake()
    {
        

        
        StartGameBTN.onClick.AddListener(() =>
        {
            WaitingRoomManager.Instance.LoadGameScene();
        });

        team1BTN.onClick.AddListener(() =>
        {
            SetPlayerTeamNumberServerRpc(1);
        });

        team2BTN.onClick.AddListener(() =>
        {
            SetPlayerTeamNumberServerRpc(2);
        });

        
        leaveBTN.onClick.AddListener(() =>
        {
            ServerManager.Instance.IsVoluntaryDisconnect = true;
            NetworkManager.Singleton.Shutdown();
            //shut down network manager auto trigger return to lobby
        });
        
    }

    private void Start()
    {
        //subscribe lobbylist change delegate
        GameMultiplayer.Instance.GetRoomPlayerDataNetworkList().OnListChanged += (NetworkListEvent<PlayerData> changeEvent) => { UpdateDisplayPlayers(); };
    }

    private void OnDisable()
    {
        //do i need to unsub startgamebtn.conclick?
        StartGameBTN.onClick.RemoveAllListeners();
        team1BTN.onClick.RemoveAllListeners();
        team2BTN.onClick.RemoveAllListeners();
        leaveBTN.onClick.RemoveAllListeners();
        GameMultiplayer.Instance.GetRoomPlayerDataNetworkList().OnListChanged -= (NetworkListEvent<PlayerData> changeEvent) => { UpdateDisplayPlayers(); };
    }

    private void UpdateDisplayPlayers()
    {
        int index = 0;
        foreach (PlayerData player in GameMultiplayer.Instance.GetRoomPlayerDataNetworkList())
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
        
            
    }

    private void SetPlayerTeamNumberServerRpc(int teamNumber)
    {
        PlayerCharacter player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<PlayerCharacter>();
        if (player == null)
        {
            Debug.Log("cant set team Number, player is null");
            return;
        }

        player.SetTeamNumberServerRpc(teamNumber);
    }
    

    //listen to events from start
    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    
}
