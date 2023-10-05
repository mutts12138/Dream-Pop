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

    public FixedString32Bytes name;

    public int currentTeamNumber;
    public int currentCharacterIndex;


    public int currentKillCount;
    public int currentDeathCount;
    public int currentSaveCount;

    public int totalKillCount;
    public int totalDeathCount;
    public int totalSaveCount;

    

    public PlayerData(string newPlayerId)
    {
        playerId = newPlayerId;
        clientId = 0;
        name = "Null";

        currentTeamNumber = 1;
        currentCharacterIndex = 0;

        currentKillCount = 0;
        currentDeathCount = 0;
        currentSaveCount = 0;
        
        totalKillCount = 0;
        totalDeathCount = 0;
        totalSaveCount = 0;

        
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);

        serializer.SerializeValue(ref currentTeamNumber);
        serializer.SerializeValue(ref currentCharacterIndex);

        serializer.SerializeValue(ref currentKillCount);
        serializer.SerializeValue(ref currentDeathCount);
        serializer.SerializeValue(ref currentSaveCount);

        serializer.SerializeValue(ref totalKillCount);
        serializer.SerializeValue(ref totalDeathCount);
        serializer.SerializeValue(ref totalSaveCount);

        
    }


    public void ClearCurrentScore()
    {
        currentKillCount = 0;
        currentDeathCount = 0;
        currentSaveCount = 0;
    }

    public bool Equals(PlayerData other)
    {
        return playerId == other.playerId;
    }
}
