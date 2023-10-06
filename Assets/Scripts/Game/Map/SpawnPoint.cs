using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private int groupNumber;
    public int associatedTeamNumber { get; private set; }

    public bool isTaken = false;

    public int GetGroupNumber()
    {
        return groupNumber;
    }

    public void SetAssociatedTeamNumber(int teamNumber)
    {
        associatedTeamNumber = teamNumber;
    }

    public Vector3 SetIsTakenAndReturnPosition()
    {
        isTaken = true;
        return transform.position;
    }
}
