using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UIElements;

public class DB_popExplosion : MonoBehaviour
{
    //THIS IS NOT IN USE?
    
    //visual
    

    private void Update()
    {

    }

    public void PopExplosion(float[] explosionRange)
    {
        //visual
        //explosionRange[]: 0:up, 1:down, 2:left, 3:right
        Transform[] childTransformsArray = GetComponentsInChildren<Transform>(true);
        
        //change scale of each direction of popExplosion based on pre determined range from DreamBubble
        foreach(Transform objT in childTransformsArray)
        {
            if(objT.name == "DB_popExplosion_up")
            {
                //visual placeholder
                objT.localScale += new Vector3(0.75f, 0.75f, explosionRange[0] - 1f);
                //Hit all players, items

            }
            if (objT.name == "DB_popExplosion_down")
            {
                objT.localScale += new Vector3(0.75f, 0.75f, explosionRange[1] - 1f);
            }
            if (objT.name == "DB_popExplosion_left")
            {
                objT.localScale += new Vector3(explosionRange[2] - 1f, 0.75f, 0.75f);
            }
            if (objT.name == "DB_popExplosion_right")
            {
                objT.localScale += new Vector3(explosionRange[3] - 1f, 0.75f, 0.75f);
            }
        }

        
        
    }


}
