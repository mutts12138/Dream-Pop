using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : MonoBehaviour
{
    public static MapData Instance { get; private set; }


    [SerializeField] private PickUpPoolSO pickUpPoolSO;
    [SerializeField] private List<BlockObject> blockObjectList;
    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    [SerializeField] private float roundTime;


    [SerializeField] private float globalGravityAcc;
    [SerializeField] private float globalGravityMaxSpeed;
    private void Awake()
    {
        
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
    }


    public PickUpPoolSO GetPickUpPoolSO() 
    { 
        return pickUpPoolSO; 
    }

    public List<BlockObject> GetBlockObjectList()
    {
        return blockObjectList;
    }

    public List<SpawnPoint> GetSpawnPoints()
    {
        return spawnPoints;
    }

    public float GetGlobalGravityAcc()
    {
        return globalGravityAcc;
    }

    public float GetGlobalGravityMaxSpeed()
    {
        return globalGravityMaxSpeed;
    }

    public float GetRoundTime()
    {
        return roundTime;
    }
}
