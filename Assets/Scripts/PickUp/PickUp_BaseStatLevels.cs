using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp_BaseStatLevels : PickUp
{
    private readonly PlayerCharacter player;
    PickUpSO_BaseStatLevels baseStatLevelsPickUpSO;
    public PickUp_BaseStatLevels(PickUpSO pickUpSO, PickUpHolder pickUpHolder) : base(pickUpSO, pickUpHolder)
    {
        //initialize additional variables
        baseStatLevelsPickUpSO = (PickUpSO_BaseStatLevels)pickUpSO;
        player = pickUpHolder.GetComponent<PlayerCharacter>();
    }

    public override void ApplyEffect()
    {
        
        player.CallChangeCharacterBaseStatLevelsClientRpc(baseStatLevelsPickUpSO.deltaMoveSpeedLevel, baseStatLevelsPickUpSO.deltaBubbleCountLevel, baseStatLevelsPickUpSO.deltaBubblePowerLevel);

        //get data from SO
        //get ref to player variable
        
    }


}
