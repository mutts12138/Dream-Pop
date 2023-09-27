using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PickUps/BaseStatLevelsPickUp")]
public class PickUpSO_BaseStatLevels : PickUpSO
{
    //hold datas.
    public int deltaBubbleCountLevel;
    public int deltaBubblePowerLevel;
    public int deltaMoveSpeedLevel;

    public override PickUp InitializePickUpEffect(PickUpHolder pickUpHolder)
    {
        
        return new PickUp_BaseStatLevels(this, pickUpHolder);
    }
}
