using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManager : MonoBehaviour
{
    public static MapDataManager Instance { get; private set; }

    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
    }

    public List<SpawnPoint> GetSpawnPoints()
    {
        return spawnPoints;
    }
}
