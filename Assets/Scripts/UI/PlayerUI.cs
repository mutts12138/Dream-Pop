using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

using static PlayerCharacter;
using System;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Button BindPlayerUIToLocalClientPlayerObjectBTN;

    [SerializeField] private TextMeshProUGUI TMP_bubble;
    [SerializeField] private TextMeshProUGUI TMP_power;
    [SerializeField] private TextMeshProUGUI TMP_speed;

    private PlayerCharacter player;


    // Start is called before the first frame update
    void Awake()
    {
        BindPlayerUIToLocalClientPlayerObjectBTN.onClick.AddListener(() =>
        {
            Debug.Log(player);


            if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<PlayerCharacter>() == null)
            {
                Debug.Log("Player is null, PlayerUI:SetPlayer failed");
                return;
            }

            CallBindPlayerUIToPlayer();
        });


    }

    private void OnDisable()
    {
        BindPlayerUIToLocalClientPlayerObjectBTN.onClick.RemoveAllListeners();
        if (player != null )
        {
            player.onCharacterBaseStatLevelChange -= (object sender, CharacterBaseStatLevelChangeEventArgs e) => { UpdateCharacterBaseStatLevelDisplay(e.newBubbleCountLevel, e.newBubblePowerLevel, e.newMoveSpeedLevel); };
        }
        
        

    }

    
    public void CallBindPlayerUIToPlayer(PlayerCharacter newPlayer)
    {
        if(player  != null)
        {
            player.onCharacterBaseStatLevelChange -= (object sender, CharacterBaseStatLevelChangeEventArgs e) => { UpdateCharacterBaseStatLevelDisplay(e.newBubbleCountLevel, e.newBubblePowerLevel, e.newMoveSpeedLevel); };

        }
        player = newPlayer;
        BindPlayerUIToPlayer();
    }

    public void CallBindPlayerUIToPlayer()
    {
        //if paramater is null bind to local player
        if (player != null)
        {
            player.onCharacterBaseStatLevelChange -= (object sender, CharacterBaseStatLevelChangeEventArgs e) => { UpdateCharacterBaseStatLevelDisplay(e.newBubbleCountLevel, e.newBubblePowerLevel, e.newMoveSpeedLevel); };
        }

        player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<PlayerCharacter>();
        
        BindPlayerUIToPlayer();
    }

    private void BindPlayerUIToPlayer()
    {
        //Debug.Log("bind player ui to player is called");


        //Debug.Log(player);
        player.onCharacterBaseStatLevelChange += (object sender, CharacterBaseStatLevelChangeEventArgs e) => { UpdateCharacterBaseStatLevelDisplay(e.newBubbleCountLevel, e.newBubblePowerLevel, e.newMoveSpeedLevel); };

        player.CallChangeCharacterBaseStatLevelsServerRpc(0, 0, 0);
    }

    private void UpdateCharacterBaseStatLevelDisplay(int newBubbleCountLevel, int newBubblePowerLevel, int newMoveSpeedLevel)
    {
        TMP_bubble.text = "Bubble: " + newBubbleCountLevel.ToString();
        TMP_power.text = "Power:  " + newBubblePowerLevel.ToString();
        TMP_speed.text = "Speed: " + newMoveSpeedLevel.ToString();
    }
}
