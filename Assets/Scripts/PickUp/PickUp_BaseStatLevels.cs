using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp_BaseStatLevels : PickUp
{
    private readonly Player player;

    public PickUp_BaseStatLevels(PickUpSO pickUpSO, PickUpHolder pickUpHolder) : base(pickUpSO, pickUpHolder)
    {
        //initialize additional variables
        player = pickUpHolder.GetComponent<Player>();
    }

    public override void ApplyEffect()
    {
        PickUpSO_BaseStatLevels baseStatLevelsPickUpSO = (PickUpSO_BaseStatLevels) pickUpSO;
        player.ChangeCharacterBaseStatLevelsClientRpc(baseStatLevelsPickUpSO.deltaMoveSpeedLevel, baseStatLevelsPickUpSO.deltaBubbleCountLevel, baseStatLevelsPickUpSO.deltaBubblePowerLevel);

        //get data from SO
        //get ref to player variable
        



    }
}
