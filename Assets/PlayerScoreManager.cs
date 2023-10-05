using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScoreManager : NetworkBehaviour
{
    //keep track of player scores:
    //kill, death, saves
    //gamemode related scores

    private void Awake()
    {
        //gameMultiplayer event : OnPlayerCharacterSpawned

        /*
        foreach (PlayerCharacter playerCharacter in GameMultiplayer.Instance.playerCharacterList)
        {
            playerCharacter.OnKill += PlayerCharacter_OnKill;
            playerCharacter.OnDeath += PlayerCharacter_OnDeath;
            playerCharacter.OnSave += PlayerCharacter_OnSave;

        }
        */
    }

    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        GameMultiplayer.Instance.OnLoadingPlayersComplete += GameMultiplayer_OnLoadingPlayersComplete;

    }

    private void GameMultiplayer_OnLoadingPlayersComplete(object sender, EventArgs e)
    {
        
        foreach(PlayerCharacter playerCharacter in GameMultiplayer.Instance.playerCharacterList)
        {
            
            playerCharacter.OnKill += PlayerCharacter_OnKill;
            playerCharacter.OnDeath += PlayerCharacter_OnDeath;
            playerCharacter.OnSave += PlayerCharacter_OnSave;
        }
        
    }


    private void PlayerCharacter_OnKill(object sender, System.EventArgs e)
    {
        PlayerCharacter playerCharacter = (PlayerCharacter)sender;

        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(playerCharacter.ownerClientId.Value);

        playerData.currentKillCount += 1;
        playerData.totalKillCount += 1;

        GameMultiplayer.Instance.UpdatePlayerData(playerData);
    }

    private void PlayerCharacter_OnDeath(object sender, System.EventArgs e)
    {
        PlayerCharacter playerCharacter = (PlayerCharacter)sender;
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(playerCharacter.ownerClientId.Value);

        playerData.currentDeathCount += 1 ;
        playerData.totalDeathCount +=1 ;

        GameMultiplayer.Instance.UpdatePlayerData(playerData);
    }

    private void PlayerCharacter_OnSave(object sender, System.EventArgs e)
    {
        PlayerCharacter playerCharacter = (PlayerCharacter)sender;

        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(playerCharacter.ownerClientId.Value);

        playerData.currentSaveCount+= 1;
        playerData.totalSaveCount+= 1;

        GameMultiplayer.Instance.UpdatePlayerData(playerData);
    }

    


    void Update()
    {
        
    }


    public override void OnDestroy()
    {
        if (IsServer)
        {

            if (gameObject.GetComponent<NetworkObject>().IsSpawned == true)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }

            GameMultiplayer.Instance.OnLoadingPlayersComplete -= GameMultiplayer_OnLoadingPlayersComplete;
        }

        base.OnDestroy();
    }

    
}
