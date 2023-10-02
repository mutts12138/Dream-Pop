using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp_BaseStatLevels : PickUp
{
    private readonly PlayerCharacter playerCharacter;
    PickUpSO_BaseStatLevels baseStatLevelsPickUpSO;
    public PickUp_BaseStatLevels(PickUpSO pickUpSO, PickUpHolder pickUpHolder) : base(pickUpSO, pickUpHolder)
    {
        //initialize additional variables
        baseStatLevelsPickUpSO = (PickUpSO_BaseStatLevels)pickUpSO;
        if(pickUpHolder.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerCharacter))
        {
            this.playerCharacter = playerCharacter;
        }
        
    }

    public override void ApplyEffect()
    {
        if(playerCharacter != null)
        {
            playerCharacter.CallChangeCharacterBaseStatLevelsClientRpc(baseStatLevelsPickUpSO.deltaMoveSpeedLevel, baseStatLevelsPickUpSO.deltaBubbleCountLevel, baseStatLevelsPickUpSO.deltaBubblePowerLevel);
        }
        

        //get data from SO
        //get ref to player variable
        
    }


}
