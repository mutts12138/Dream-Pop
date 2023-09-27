using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_Asleep : Buff
{
    private readonly PlayerCharacter player;
    Coroutine runningCoroutine;
    BuffSO_Asleep buffSO_Asleep;
    public Buff_Asleep(BuffSO buffSO, BuffHolder buffHolder) : base(buffSO, buffHolder)
    {
        buffSO_Asleep = (BuffSO_Asleep)buffSO;
        player = buffHolder.GetComponent<PlayerCharacter>();
        Debug.Log(player);
    }


    /* 
    private int moveDisableStack;
    private int placeDreamBubbleDisableStack;
    private int useAbilityDisableStack;
    private int useItemDisableStack;

    private bool isEliminated;
    private bool canRespawn;
    */
    protected override void ApplyBuffEffect()
    {
        player.SetIsAsleep(true);
        player.AddToMoveDisableStack(1);
        player.AddToPlaceDreamBubbleDisableStack(1);
        player.AddToUseAbilityDisableStack(1);
        player.AddToUseItemDisableStack(1);
        runningCoroutine = player.StartCoroutine(CheckASleepPlayerCollision());
    }

    IEnumerator CheckASleepPlayerCollision()
    {
        float sphereRadius = 1f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        int layerMask;
        int layerNumber = 3;
        layerMask = 1 << layerNumber;

        int teammateCount = 0;
        int opponentCount = 0;

        while(true)
        {
            

            Collider[] playersOverlapped = Physics.OverlapSphere(player.transform.position + Vector3.up, sphereRadius, layerMask);
            
            if (playersOverlapped.Length > 0)
            {
                foreach (Collider playerOverlapped in playersOverlapped)
                {
                    if (playerOverlapped.gameObject.GetComponent<PlayerCharacter>().teamNumber.Value == player.teamNumber.Value)
                    {
                        if (playerOverlapped.gameObject != player.gameObject)
                        {
                            teammateCount++;
                        }
                            
                    }
                    else
                    {

                        opponentCount++;

                    }
                }
                
            }

            if (teammateCount ==0 && opponentCount == 0)
            {
                yield return waitForSeconds;
            }
            else
            {
                if (teammateCount >= opponentCount)
                {
                    //get awaken:saved
                    Debug.Log("Saved " + "teammate count " + teammateCount + " opponentCount " + opponentCount);
                    //just let the debuff end
                }
                if (opponentCount > teammateCount)
                {
                    //get rude awaken:death
                    Debug.Log("death");
                    buffHolder.AddBuff(buffSO_Asleep.buffSO_Eliminated.InitializeBuff(buffHolder));

                    //buffHolder.AddBuff(buff.buffSO_eliminated.InitializeBuff(buffHolder));
                    //player.SetIsEliminated(true);\
                    // apply is eliminated debuff
                }

                duration = 0;
                yield break;
            }   
        }
    }

    public override void BuffEnded()
    {
        
        if (runningCoroutine != null)
        {
            Debug.Log("Saved");
            player.StopCoroutine(runningCoroutine);
        }

        player.SetIsAsleep(false);
        player.AddToMoveDisableStack(-1);
        player.AddToPlaceDreamBubbleDisableStack(-1);
        player.AddToUseAbilityDisableStack(-1);
        player.AddToUseItemDisableStack(-1);
    }
}
