using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public FixedString32Bytes playerId;
    public ulong clientId;

    public int winCount;
    public int loseCount;
    public int drawCount;

    public PlayerData(string newPlayerId)
    {
        playerId = newPlayerId;
        clientId = 0;
        winCount = 0;
        loseCount = 0;
        drawCount = 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref winCount);
        serializer.SerializeValue(ref drawCount);
        serializer.SerializeValue(ref loseCount);
    }



    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId;
    }
}
