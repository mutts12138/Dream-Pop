using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_Eliminated : Buff
{
    private readonly PlayerCharacter player;

    public Buff_Eliminated(BuffSO buffSO, BuffHolder buffHolder) : base(buffSO, buffHolder)
    {
        player = buffHolder.GetComponent<PlayerCharacter>();
        Debug.Log(player);
    }

    protected override void ApplyBuffEffect()
    {
        

        player.AddToMoveDisableStack(1);
        player.AddToPlaceDreamBubbleDisableStack(1);
        player.AddToUseAbilityDisableStack(1);
        player.AddToUseItemDisableStack(1);

        //remove all removable buff/debuff
        //drop items/power ups if enabled
        player.SetIsEliminated(true);
        //change player layer to eliminated: 8, no need to disable collider since layer changed
        player.SetCurrentLayer(8);
        

        //Add scoring
        
    }

    public override void BuffEnded()
    {
        //Because this is on the client side, Lag gives unfair advantage, making asleep effect last longer than intented because player needs to call to end
        //Need to make this server sided when necessary


        player.AddToMoveDisableStack(-1);
        player.AddToPlaceDreamBubbleDisableStack(-1);
        player.AddToUseAbilityDisableStack(-1);
        player.AddToUseItemDisableStack(-1);
        player.SetIsEliminated(false);
        //change layer back to player: 3
        player.SetCurrentLayer(3);

        //respawn
        //give a invincible buff
    }
}
