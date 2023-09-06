using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpHolder : MonoBehaviour
{

    //holds all the items here in dictionary
    private readonly Dictionary<PickUpSO, PickUp> _pickUps = new Dictionary<PickUpSO, PickUp>();

    public void AddPickUpToInventory(PickUp pickUp)
    {
        if (_pickUps.ContainsKey(pickUp.pickUpSO))
        {
            _pickUps[pickUp.pickUpSO].OnAddPickUp();
        }
        else
        {
            _pickUps.Add(pickUp.pickUpSO, pickUp);
            _pickUps[pickUp.pickUpSO].OnAddPickUp();
        }
    }
}
