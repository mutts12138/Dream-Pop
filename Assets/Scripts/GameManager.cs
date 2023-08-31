using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    [SerializeField] private PlayerUI playerUI;

    //maybe get it from map later
    [SerializeField] private float globalGravityAcc = -100f;
    [SerializeField] private float globalGravityMaxSpeed = -10f;

    ulong[] clientIds = new ulong[8];

    delegate void StartGame();
    private event StartGame OnStartGame;

    private ulong localClientID;

    //OnServerConnect
    //OnServerAddPlayer
    //when connected
    //clientIDs array

    //players hold playerNumbers
    //playerNumbers
    private void Awake()
    {
        //gameObject.GetComponent<NetworkObject>().Spawn(true);

        if (!IsServer) return;

        
        //get playerdata from lobbymanager(doesnt destroy on load)
        //lobbymanager will spawn the player objects
        //LobbyManager.Instance.SpawnAllPlayerObjects();




    }

    public override void OnNetworkSpawn()
    {
        localClientID = NetworkManager.Singleton.LocalClientId;
        //initialize players

        //LobbyManager.Instance.SpawnPlayerObjectClientRpc(localClientID);

        if(!IsServer) return;

       

    }

    public float GetGlobalGravityAcc()
    {
        return globalGravityAcc;
    }

    public float GetGlobalGravityMaxSpeed()
    {
        return globalGravityMaxSpeed;
    }


    
    
}