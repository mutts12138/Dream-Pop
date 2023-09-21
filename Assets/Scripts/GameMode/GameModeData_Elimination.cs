using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class GameModeData_Elimination : GameModeData
{


    
    public GameModeData_Elimination() : base()
    {
        gameModeName = "Elimination";
    }
    public override void ImplementGameModeFeatures()
    {
        //no additional features for Elimination mode
        //bind checkwincondition to particular events
    }

    public override bool IsWinConditionAchieved()
    {
        //check to see if players are eliminated for each team
        //return true and declare gameover when only one team is left

        //dictionary<teamnumber, playeralivecount>
        int[] PlayersAliveCount_inEachTeam = new int[8];

        int teamsRemainingCount = 8;
        //get the amount of players alive in each team
        foreach (PlayerCharacter playerObj in GameManager.Instance.GetPlayerObjArray())
        {
            if(playerObj != null)
            {
                if (playerObj.isEliminated.Value == true)
                {
                    PlayersAliveCount_inEachTeam[playerObj.teamNumber.Value]++;
                }
            }
            
        }
        //get remaining teams
        for (int i = 0; i < PlayersAliveCount_inEachTeam.Length; i++)
        {
            if (PlayersAliveCount_inEachTeam[i] == 0)
            {
                teamsRemainingCount--;
            }
        }

        if(teamsRemainingCount <= 1)
        {
            return true;
        }

        return false;

    }

    public override int[] GetWinnerTeams()
    {
        //display which team won
        //if multiple teams, then display draw
        //Add to player scores, win, draw, or lost
        List<int> winnerTeams = new List<int>();

        int[] PlayersAliveCount_inEachTeam = new int[8];

        //get the amount of players alive in each team
        foreach (PlayerCharacter playerObj in GameManager.Instance.GetPlayerObjArray())
        {
            if (playerObj != null)
            {
                if (playerObj.isEliminated.Value == true)
                {
                    PlayersAliveCount_inEachTeam[playerObj.teamNumber.Value]++;
                }
            }

        }

        //put in teams with players alive to int[]
        for (int i = 0; i < PlayersAliveCount_inEachTeam.Length; i++)
        {
            if(PlayersAliveCount_inEachTeam[i] != 0)
            {
                winnerTeams.Add(i);
            }

        }

        return winnerTeams.ToArray();
    }
}
