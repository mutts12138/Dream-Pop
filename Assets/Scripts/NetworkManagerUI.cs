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
    }
}
