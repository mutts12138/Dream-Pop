using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PickUps/PickUpSOListSO")]
public class PickUpSOListSO : ScriptableObject
{
    //this is for converting to index, and index to pickupSO
    //used to transfer data over the netcode

    public List<PickUpSO> pickUpSOList;
}
