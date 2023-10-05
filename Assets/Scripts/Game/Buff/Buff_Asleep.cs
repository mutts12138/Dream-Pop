using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_Asleep : Buff
{
    private readonly PlayerCharacter playerCharacter;
    Coroutine runningCoroutine;
    BuffSO_Asleep buffSO_Asleep;
    public Buff_Asleep(BuffSO buffSO, BuffHolder buffHolder) : base(buffSO, buffHolder)
    {
        buffSO_Asleep = (BuffSO_Asleep)buffSO;
        playerCharacter = buffHolder.GetComponent<PlayerCharacter>();
        Debug.Log(playerCharacter);
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
        playerCharacter.SetIsAsleep(true);
        playerCharacter.AddToMoveDisableStackClientRpc(1);
        playerCharacter.AddToPlaceDreamBubbleDisableStackClientRpc(1);
        playerCharacter.AddToUseAbilityDisableStackClientRpc(1);
        playerCharacter.AddToUseItemDisableStackClientRpc(1);
        runningCoroutine = playerCharacter.StartCoroutine(CheckASleepPlayerCollision());
    }


    IEnumerator CheckASleepPlayerCollision()
    {
        float sphereRadius = 1f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        int layerMask;
        int PlayerlayerNumber = 3;
        layerMask = 1 << PlayerlayerNumber;

        while(true)
        { 
            Collider[] playersOverlapped = Physics.OverlapSphere(playerCharacter.transform.position + Vector3.up, sphereRadius, layerMask);
            
            if (playersOverlapped.Length > 0)
            {
                foreach (Collider playerOverlapped in playersOverlapped)
                {

                    if (playerOverlapped.gameObject != playerCharacter.gameObject)
                    {

                        if (playerOverlapped.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter otherPlayerCharacter))
                        {
                            
                            if (otherPlayerCharacter.teamNumber.Value == playerCharacter.teamNumber.Value)
                            {
                                //get awaken:saved
                                Debug.Log("Saved");
                                //just let the debuff end

                                //otherPlayerCharacter: call saved
                                otherPlayerCharacter.InvokeSave();
                                //playerCharacter : call beingSaved
                                playerCharacter.InvokeBeingSaved();
                                
                            }
                            else
                            {
                                //get rude awaken:death
                                Debug.Log("death");
                                buffHolder.AddBuff(buffSO_Asleep.buffSO_Death.InitializeBuff(buffHolder));

                                //buffHolder.AddBuff(buff.buffSO_eliminated.InitializeBuff(buffHolder));
                                //player.SetIsEliminated(true);\
                                // apply is eliminated debuff

                                //otherPlayerCharacter: call kill
                                otherPlayerCharacter.InvokeKill();
                                //playerCharacter : call death_ this is called within the the death buff when apply to player character
                                
                            }

                            duration = 0;
                            yield break;

                        }

                    }
                        
                }

            }
            yield return waitForSeconds;
            

        }
    }

    public override void BuffEnded()
    {
        
        if (runningCoroutine != null)
        {
            playerCharacter.StopCoroutine(runningCoroutine);
        }

        playerCharacter.SetIsAsleep(false);
        playerCharacter.AddToMoveDisableStackClientRpc(-1);
        playerCharacter.AddToPlaceDreamBubbleDisableStackClientRpc(-1);
        playerCharacter.AddToUseAbilityDisableStackClientRpc(-1);
        playerCharacter.AddToUseItemDisableStackClientRpc(-1);
    }

    /*
    IEnumerator CheckASleepPlayerCollision()
    {
        float sphereRadius = 1f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        int layerMask;
        int PlayerlayerNumber = 3;
        layerMask = 1 << PlayerlayerNumber;

        List<PlayerCharacter> teammatesDetected = new List<PlayerCharacter>(0);
        List<PlayerCharacter> opponentsDetected = new List<PlayerCharacter>(0);

        while (true)
        {


            Collider[] playersOverlapped = Physics.OverlapSphere(player.transform.position + Vector3.up, sphereRadius, layerMask);

            if (playersOverlapped.Length > 0)
            {
                foreach (Collider playerOverlapped in playersOverlapped)
                {
                    if (playerOverlapped.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerCharacter))
                    {
                        if (playerCharacter.teamNumber.Value == player.teamNumber.Value)
                        {
                            if (playerOverlapped.gameObject != player.gameObject)
                            {
                                teammatesDetected.Add(playerCharacter);
                            }
                        }
                        else
                        {
                            opponentsDetected.Add(playerCharacter);
                        }
                    }
                }

            }

            if (teammatesDetected.Count == 0 && opponentsDetected.Count == 0)
            {
                yield return waitForSeconds;
            }
            else
            {
                if (teammatesDetected.Count >= opponentsDetected.Count)
                {
                    //get awaken:saved
                    Debug.Log("Saved " + "teammate count " + teammatesDetected + " opponentCount " + opponentsDetected);
                    //just let the debuff end
                }
                if (opponentsDetected.Count > teammatesDetected.Count)
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
    }*/
}
