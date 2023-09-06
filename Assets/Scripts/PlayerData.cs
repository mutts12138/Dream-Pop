using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public bool isConnected;
    public ulong clientID;
    public int teamNumber;


    public PlayerData(ulong newClientID)
    {
        isConnected = true;
        clientID = newClientID;
        teamNumber = 1;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isConnected);
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref teamNumber);
    }


    public void Connected()
    {
        isConnected = true;
    }


    public void Disconnected()
    {
        isConnected = false;
        clientID = 404;
    }

    public void SetTeamNumber(int newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }



    public bool Equals(PlayerData other)
    {
        return clientID == other.clientID;
    }
}
