using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBTN;
    [SerializeField] private Button serverBTN;
    [SerializeField] private Button clientBTN;
    [SerializeField] private Button team1BTN;
    [SerializeField] private Button team2BTN;

    private void Awake()
    {
        hostBTN.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        serverBTN.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        clientBTN.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

        team1BTN.onClick.AddListener(() =>
        {
            if(NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent<Player>(out Player player))
            {
                Debug.Log(player);
                player.SetTeamNumber(1);
            }  
        });

        team2BTN.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent<Player>(out Player player))
            {
                player.SetTeamNumber(2);
            }
        });
    }
}
