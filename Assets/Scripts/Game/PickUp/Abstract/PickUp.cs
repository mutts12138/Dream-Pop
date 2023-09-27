using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickUp 
{
    //holds stat changes, effects
    public PickUpSO pickUpSO { get; }

    protected readonly PickUpHolder pickUpHolder;

    protected int pickUpStacks;


    public PickUp (PickUpSO pickUpSO, PickUpHolder pickUpHolder)
    {
        this.pickUpSO = pickUpSO;
        this.pickUpHolder = pickUpHolder;
    }


    public void OnAddPickUp()
    {
        if (pickUpSO.IsPickUpStacked == true || pickUpStacks <= 0)
        {
            ApplyEffect();
            pickUpStacks++;
        }
        //if not stackable, return it back to pick up pool?

    }
    public abstract void ApplyEffect();
}
