using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public struct PlayerScore : INetworkSerializable, IEquatable<PlayerScore>
{
    //NO LONGER IN USE, HAS BEEN INCORPORATED INTO PLAYERDATA


    public ulong clientId;

    public int team;
    public FixedString32Bytes name;
    public int kill;
    public int death;
    public int save;
    public int score;

    public PlayerScore(PlayerCharacter playerCharacter)
    {
        clientId = playerCharacter.ownerClientId.Value;
        team = playerCharacter.teamNumber.Value;
        name = "Null";
        kill = 0;
        death = 0;
        save = 0;
        score = 0;
    }

     

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref kill);
        serializer.SerializeValue(ref death);
        serializer.SerializeValue(ref save);
        serializer.SerializeValue(ref score);

    }

    public bool Equals(PlayerScore playerScore)
    {
        return this.clientId == playerScore.clientId;
    }

}
