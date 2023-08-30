using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public bool isConnected;
    public ulong clientID;


    public PlayerData(ulong newClientID)
    {
        isConnected = false;
        clientID = newClientID; 
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isConnected);
        serializer.SerializeValue(ref clientID);
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

    public bool Equals(PlayerData other)
    {
        return clientID == other.clientID;
    }
}
