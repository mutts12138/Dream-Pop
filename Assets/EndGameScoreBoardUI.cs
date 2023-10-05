using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EndGameScoreBoardUI : MonoBehaviour
{
    [SerializeField] private Transform scoreBoardListContainer;
    [SerializeField] private Transform playerScoreTemplate;


    //Get info from the scoreboard manager and display it
    
    void Start()
    {
        GameManager.Instance.OnGameOver += GameManager_OnGameOver;
        Hide();
    }

    private void GameManager_OnGameOver(object sender, System.EventArgs e)
    {
        Show();
        UpdatePlayerScoreList();
        
    }

    void Update()
    {
        
    }

    private void UpdatePlayerScoreList()
    {
        //clean up
        //Debug.Log(lobbyListContainer);
        foreach (Transform child in scoreBoardListContainer)
        {
            if (child == playerScoreTemplate) continue;
            Destroy(child.gameObject);
        }

        //putting in new
        foreach (PlayerData playerData in GameMultiplayer.Instance.GetPlayerDataNetworkList())
        {
            Transform playerScoreTransform = Instantiate(playerScoreTemplate, scoreBoardListContainer);
            playerScoreTransform.gameObject.SetActive(true);
            playerScoreTransform.GetComponent<EndGamePlayerScoreListSingleUI>().SetPlayerScoreUI(playerData);
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
