using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_Death : Buff
{
    private readonly PlayerCharacter player;

    public Buff_Death(BuffSO buffSO, BuffHolder buffHolder) : base(buffSO, buffHolder)
    {
        player = buffHolder.GetComponent<PlayerCharacter>();
        Debug.Log(player);
    }

    protected override void ApplyBuffEffect()
    {
        

        player.AddToMoveDisableStackClientRpc(1);
        player.AddToPlaceDreamBubbleDisableStackClientRpc(1);
        player.AddToUseAbilityDisableStackClientRpc(1);
        player.AddToUseItemDisableStackClientRpc(1);

        //remove all removable buff/debuff
        //drop items/power ups if enabled
        player.InvokeDeath();
        //change player layer to eliminated: 8, no need to disable collider since layer changed
        player.SetCurrentLayer(8);
        

        //Add scoring
        
    }

    public override void BuffEnded()
    {
        //Because this is on the client side, Lag gives unfair advantage, making asleep effect last longer than intented because player needs to call to end
        //Need to make this server sided when necessary


        player.AddToMoveDisableStackClientRpc(-1);
        player.AddToPlaceDreamBubbleDisableStackClientRpc(-1);
        player.AddToUseAbilityDisableStackClientRpc(-1);
        player.AddToUseItemDisableStackClientRpc(-1);
        player.InvokeRespawn();
        //change layer back to player: 3
        player.SetCurrentLayer(3);

        //respawn
        //give a invincible buff
    }
}
