using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientID;
    public int teamNumber;

    public int winCount;
    public int loseCount;
    public int drawCount;

    public PlayerData(ulong newClientID)
    {
        clientID = newClientID;
        teamNumber = -1;
        winCount = 0;
        loseCount = 0;
        drawCount = 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref teamNumber);
        serializer.SerializeValue(ref winCount);
        serializer.SerializeValue(ref drawCount);
        serializer.SerializeValue(ref loseCount);
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
