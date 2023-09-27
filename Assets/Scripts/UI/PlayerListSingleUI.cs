using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListSingleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text PlayerNameText;

    private Player player;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            
        });
    }

    public void SetPlayer(Player newPlayer)
    {
        player = newPlayer;
        PlayerNameText.text = player.Data["PlayerName"].Value;
    }
}
