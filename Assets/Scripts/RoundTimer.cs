using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoundTimer : NetworkBehaviour
{

    [SerializeField] private float gameStartCountDown = 3;

    private NetworkVariable<float> roundTimer;



    // Start is called before the first frame update
    void Start()
    {
        SetRoundTimer(MapData.Instance.GetRoundTime());
    }

    // Update is called once per frame
    void Update()
    {
        

        roundTimer.Value -= Time.deltaTime;
            
        
    }

    private void SetRoundTimer(float time)
    {
        roundTimer.Value = time;
    }
}
