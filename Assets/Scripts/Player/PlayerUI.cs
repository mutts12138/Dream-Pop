using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using static Player;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Button BindPlayerUIToLocalClientPlayerObjectBTN;

    [SerializeField] private TextMeshProUGUI TMP_bubble;
    [SerializeField] private TextMeshProUGUI TMP_power;
    [SerializeField] private TextMeshProUGUI TMP_speed;

    private Player player;

    [SerializeField] private Button team1BTN;
    [SerializeField] private Button team2BTN;



    // Start is called before the first frame update
    void Awake()
    {
        BindPlayerUIToLocalClientPlayerObjectBTN.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject == null)
            {
                Debug.Log("Player is null, PlayerUI:SetPlayer failed");
                return;
            }
                
            Player currentLocalPlayerObject = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<Player>();
            BindPlayerUIToPlayer(currentLocalPlayerObject);
        });

        
    }

    private void OnDisable()
    {
        BindPlayerUIToLocalClientPlayerObjectBTN.onClick.RemoveAllListeners();
        player.onCharacterBaseStatLevelChange -= (object sender, CharacterBaseStatLevelChangeEventArgs e) => { UpdateCharacterBaseStatLevelDisplay(e.newBubbleCountLevel, e.newBubblePowerLevel, e.newMoveSpeedLevel); };
        team1BTN.onClick.RemoveAllListeners();
        team2BTN.onClick.RemoveAllListeners();

    }

    private void BindPlayerUIToPlayer(Player newPlayerObject)
    {
        ClearTeamSelectBTNListeners();

        player = newPlayerObject;
        if (player == null)
        {
            Debug.Log("Player is null, PlayerUI:SetPlayer failed");
            return;
        }
        
        player.onCharacterBaseStatLevelChange += (object sender, CharacterBaseStatLevelChangeEventArgs e) => { UpdateCharacterBaseStatLevelDisplay(e.newBubbleCountLevel, e.newBubblePowerLevel, e.newMoveSpeedLevel); };
        
        team1BTN.onClick.AddListener(() =>
        {
            player.SetTeamNumber(1);
        });

        team2BTN.onClick.AddListener(() =>
        {
            player.SetTeamNumber(2);
        });
    }

    private void ClearTeamSelectBTNListeners()
    {
        team1BTN.onClick.RemoveAllListeners();
        team2BTN.onClick.RemoveAllListeners();
    }

    private void UpdateCharacterBaseStatLevelDisplay(int newBubbleCountLevel, int newBubblePowerLevel, int newMoveSpeedLevel)
    {
        TMP_bubble.text = "Bubble: " + newBubbleCountLevel.ToString();
        TMP_power.text = "Power:  " + newBubblePowerLevel.ToString();
        TMP_speed.text = "Speed: " + newMoveSpeedLevel.ToString();
    }
}
