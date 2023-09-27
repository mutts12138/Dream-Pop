using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private int teamNumber;

    private bool isTaken = false;

    public int GetTeamNumber()
    {
        return teamNumber;
    }

    public void SetTeamNumber(int newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }

    public bool GetIsTaken()
    {
        return isTaken;
    }

    public void SetIsTaken(bool newIsTaken)
    {
        isTaken = newIsTaken;
    }
}
