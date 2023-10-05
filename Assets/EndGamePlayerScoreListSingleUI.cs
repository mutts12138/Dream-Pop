using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EndGamePlayerScoreListSingleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text TeamText;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text KDSText;
    

    private PlayerData playerData;

    private void Awake()
    {
        
    }

    //make sure playerData and playerScore has the save clientId
    public void SetPlayerScoreUI(PlayerData playerData)
    {
        
        TeamText.text = playerData.currentTeamNumber.ToString();
        NameText.text = playerData.name.ToString();
        KDSText.text = playerData.currentKillCount.ToString() + "/" + playerData.currentDeathCount.ToString() + "/" + playerData.currentSaveCount.ToString();
        
    }
}
