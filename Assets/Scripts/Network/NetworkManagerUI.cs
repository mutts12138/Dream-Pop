using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBTN;
    [SerializeField] private Button serverBTN;
    [SerializeField] private Button clientBTN;

    [SerializeField] private GameObject connectionDataGUI;
    [SerializeField] private TMP_InputField connectionDataIPInputF;
    [SerializeField] private TMP_InputField connectionDataPortInputF;
    [SerializeField] private Button connectBTN;

    



    private string ipv4Address;
    private ushort port;
    
    
    private void Awake()
    {
        connectionDataGUI.gameObject.SetActive(false);

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
            //enable and set visbility of inputfields and connect button
            connectionDataGUI.gameObject.SetActive(true);
        });

        


        connectionDataIPInputF.onEndEdit.AddListener((string connectionDataIP) =>
        {
            ipv4Address = connectionDataIP;
        });

        connectionDataPortInputF.onEndEdit.AddListener((string connectionDataPort) =>
        {
            bool portValid = ushort.TryParse(connectionDataPort, out ushort connectionDataPortuShort);
            
            if (portValid)
            {
                port = connectionDataPortuShort;
            }
            else
            {
                Debug.Log("port format invalid");
            }
        });




        connectBTN.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port, null);
            Debug.Log("connecting to" + ipv4Address + "port" + port);
            NetworkManager.Singleton.StartClient();
            connectionDataGUI.gameObject.SetActive(false);


        });

        


        

        
    }

    [ServerRpc]
    private void PrintClientConnectedServerRpc()
    {

    }
}
