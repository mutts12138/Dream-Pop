using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Player;

public class PlayerUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI TMP_bubble;
    [SerializeField] private TextMeshProUGUI TMP_power;
    [SerializeField] private TextMeshProUGUI TMP_speed;

    private Player player;

    // Start is called before the first frame update
    void Awake()
    {
        //ref player first
        
    }

    // Update is called once per frame
    void Update()

    {
        
    }

    public void SetPlayer(Player newPlayer)
    {
        player = newPlayer;
        if (player != null )
        {
            player.onCharacterBaseStatLevelChange += (object sender, CharacterBaseStatLevelChangeEventArgs e) => { UpdateCharacterBaseStatLevelDisplay(e.newBubbleCountLevel, e.newBubblePowerLevel, e.newMoveSpeedLevel); };
        }
        
    }

    private void UpdateCharacterBaseStatLevelDisplay(int newBubbleCountLevel, int newBubblePowerLevel, int newMoveSpeedLevel)
    {
        TMP_bubble.text = "Bubble: " + newBubbleCountLevel.ToString();
        TMP_power.text = "Power:  " + newBubblePowerLevel.ToString();
        TMP_speed.text = "Speed: " + newMoveSpeedLevel.ToString();
    }
}
