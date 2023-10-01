using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using UnityEngine.UI;
using System;
using static UnityEngine.Rendering.BoolParameter;
using Unity.VisualScripting;
//using UnityEditor.PackageManager;

public class WaitingRoomManager : NetworkBehaviour
{
    public static WaitingRoomManager Instance { get; private set; }



    //temp
    [SerializeField] private PlayerCharacter playerPreFab;
    
    

    private void Awake()
    {
        if (Instance !=null)
        {
            
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        if (!IsServer) return;
        InitializeWaitingRoom();
        

        /*
        NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) =>
        {
            NetworkManager_OnLoadEventCompleted(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);

        };*/


        //This is for returning back to lobby after a game is complete: shouldnt be placed here
        //NetworkManager.SceneManager.OnLoadComplete += (ulong clientId, string sceneName, LoadSceneMode loadSceneMode) => { };
        //NetworkManager.SceneManager.OnLoadEventCompleted += (string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => { SpawnAllPlayerObjectsToLobby(clientsCompleted , clientsTimedOut); };


    }

    

    private void InitializeWaitingRoom()
    {
        
    }

    //calls when coming back to lobby after game ends
  

    public void LoadGameScene()
    {
        //What happens when you destroy GameObject in Unity?When you destroy a gameObject, it sets a "destroyed" flag in Unity's secret reference. At the end of the frame, Unity unhooks the real gameObject from it's list, letting it be garbage collected.
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);


        SceneLoader.LoadNetwork(SceneLoader.Scene.TestMap);

    }



    public override void OnDestroy()
    {
        if (IsServer)
        {
            if (gameObject.GetComponent<NetworkObject>().IsSpawned == true)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }

        }

        base.OnDestroy();
    }


}
