using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PickUps/PickUpPoolSO")]
public class PickUpPoolSO : ScriptableObject
{
    //item pool for the map/round
    //to be stored in mapdata, game manager would access it to create a pickup pool list, then assigns them to blocks in the map

    public List<PickUpSO> pickUpSOList;


}
