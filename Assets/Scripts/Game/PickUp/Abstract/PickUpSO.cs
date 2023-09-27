using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PickUpSO : ScriptableObject
{
    //holds the pickup visual,sound and values of stat changes?
    public string pickUpName;
    public Transform prefab;

    //attributes
    public bool IsPickUpStacked;

    public abstract PickUp InitializePickUpEffect(PickUpHolder pickUpHolder);
}
