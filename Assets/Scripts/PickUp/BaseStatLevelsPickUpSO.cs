using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PickUps/BaseStatLevelsPickUp")]
public class BaseStatLevelsPickUpSO : PickUpSO
{
    //hold datas.
    public int deltaBubbleCountLevel;
    public int deltaBubblePowerLevel;
    public int deltaMoveSpeedLevel;

    public override PickUp InitializePickUpEffect(PickUpHolder pickUpHolder)
    {
        
        return new BaseStatLevelsPickUp(this, pickUpHolder);
    }
}
