using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_Asleep : Buff
{
    private readonly Player player;
    Coroutine runningCoroutine;
    public Buff_Asleep(BuffSO buffSO, BuffHolder buffHolder) : base(buffSO, buffHolder)
    {
        player = buffHolder.GetComponent<Player>();
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
        player.AddToMoveDisableStack(1);
        player.AddToPlaceDreamBubbleDisableStack(1);
        player.AddToUseAbilityDisableStack(1);
        player.AddToUseItemDisableStack(1);
        runningCoroutine = player.StartCoroutine(CheckASleepPlayerCollision());
    }

    IEnumerator CheckASleepPlayerCollision()
    {
        float sphereRadius = 0.75f;

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
                    if (playerOverlapped.gameObject.GetComponent<Player>().GetTeamNumber() == player.GetTeamNumber())
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
                    player.SetIsEliminated(true);
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
        
        player.AddToMoveDisableStack(-1);
        player.AddToPlaceDreamBubbleDisableStack(-1);
        player.AddToUseAbilityDisableStack(-1);
        player.AddToUseItemDisableStack(-1);
    }
}
