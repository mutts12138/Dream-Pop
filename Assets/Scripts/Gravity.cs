using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    //no use right now, maybe refactor in later

    private float gravityAcc;
    private float gravityMaxSpeed;
    private float verticalVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddGravity()
    {
        float maxGravity = -10f;
        if (verticalVelocity > maxGravity)
        {
            verticalVelocity += gravityAcc * Time.deltaTime;
        }
    }

}
