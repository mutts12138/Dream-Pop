using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStatLevelsPickUp : PickUp
{
    private readonly Player player;

    public BaseStatLevelsPickUp(PickUpSO pickUpSO, PickUpHolder pickUpHolder) : base(pickUpSO, pickUpHolder)
    {
        //initialize additional variables
        player = pickUpHolder.GetComponent<Player>();
    }

    public override void ApplyEffect()
    {
        BaseStatLevelsPickUpSO baseStatLevelsPickUpSO = (BaseStatLevelsPickUpSO) pickUpSO;
        player.ChangeCharacterBaseStatLevelsClientRpc(baseStatLevelsPickUpSO.deltaMoveSpeedLevel, baseStatLevelsPickUpSO.deltaBubbleCountLevel, baseStatLevelsPickUpSO.deltaBubblePowerLevel);

        //get data from SO
        //get ref to player variable
        



    }
}
