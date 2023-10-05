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
            GameMultiplayer.Instance.SetPlayerCurrentTeamNumberServerRpc(LocalPlayerData.Instance.localPlayerData.clientId, 1);
            UpdateDisplayPlayers();
        });

        team2BTN.onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.SetPlayerCurrentTeamNumberServerRpc(LocalPlayerData.Instance.localPlayerData.clientId, 2);
            UpdateDisplayPlayers();
        });

        
        leaveBTN.onClick.AddListener(() =>
        {
            ServerManager.Instance.IsVoluntaryDisconnect = true;
            NetworkManager.Singleton.Shutdown();
            //shut down network manager auto trigger return to lobby
        });
        //subscribe lobbylist change delegate

    }

    private void Start()
    {
        GameMultiplayer.Instance.GetPlayerDataNetworkList().OnListChanged += (NetworkListEvent<PlayerData> changeEvent) => { UpdateDisplayPlayers(); };
        UpdateDisplayPlayers();
    }

    private void OnDisable()
    {
        //do i need to unsub startgamebtn.conclick?
        StartGameBTN.onClick.RemoveAllListeners();
        team1BTN.onClick.RemoveAllListeners();
        team2BTN.onClick.RemoveAllListeners();
        leaveBTN.onClick.RemoveAllListeners();
        GameMultiplayer.Instance.GetPlayerDataNetworkList().OnListChanged -= (NetworkListEvent<PlayerData> changeEvent) => { UpdateDisplayPlayers(); };
    }

    public void UpdateDisplayPlayers()
    {
        int index = 0;
        foreach (PlayerData player in GameMultiplayer.Instance.GetPlayerDataNetworkList())
        {
            switch (index)
            {
                case 0:
                    player1Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                case 1:
                    player2Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                case 2:
                    player3Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                case 3:
                    player4Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                case 4:
                    player5Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                case 5:
                    player6Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                case 6:
                    player7Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                case 7:
                    player8Text.text = player.name.ToString() + " Team: " + player.currentTeamNumber + " " + player.totalKillCount + "/" + player.totalDeathCount + "/" + player.totalSaveCount;
                    break;
                
            }
            index++;
        }
        
            
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
