using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PickUps/PickUpSOList")]
public class PickUpSOListSO : ScriptableObject
{
    public List<PickUpSO> pickUpSOList;
}
