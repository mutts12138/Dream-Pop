using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameModeData : MonoBehaviour
{
    public static GameModeData Instance { get; private set; }
    // game modes: turf war, Dream frag collect, elimination, qiang bao zi, tea, death match
    public string gameModeName;
    //customize data: player life count, target kill count, target dream frag count, item pool
    //use add component for this?

    public GameModeData()
    {

    }

    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
    }

    //hold game rules
    //win condition
    public abstract void ImplementGameModeFeatures();
    public abstract bool IsWinConditionAchieved();

    public abstract int[] GetWinnerTeams();

    

}
