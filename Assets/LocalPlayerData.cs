using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalPlayerData : MonoBehaviour
{
    public static LocalPlayerData Instance;

    public PlayerData localPlayerData;


    void Start()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
 
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLocalPlayerData(PlayerData newLocalPlayerData)
    {
        localPlayerData = newLocalPlayerData;
    }

    public void UpdateClientId(ulong newClientId)
    {
        PlayerData newPlayerData = localPlayerData;
        newPlayerData.clientId = newClientId;
        localPlayerData = newPlayerData;
    }

    [Command]
    public void PrintLocalPlayerData()
    {
        Debug.Log("PlayerId: " + localPlayerData.playerId);
        Debug.Log("ClientId: " + localPlayerData.clientId);
        Debug.Log("kills: " + localPlayerData.totalKillCount);
        Debug.Log("deaths: " + localPlayerData.totalDeathCount);
        Debug.Log("saves: " + localPlayerData.totalSaveCount);
    }



    
    public PlayerData GetLocalPlayerDataAssignNewClientId(ulong newClientId)
    {
        UpdateClientId(newClientId);
        return localPlayerData;
    }
}
