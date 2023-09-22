using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField] private PlayerCharacter player;

    private Animator animator;
    private NetworkAnimator networkAnimator;

    //animator parameters
    private const string IS_RUNNING = "isRunning";
    private const string IS_ASLEEP = "isAsleep";
    private const string IS_ELIMINATED = "isEliminated";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }
    public override void OnNetworkSpawn()
    {
        
    }

   
    void Update()
    {
        if (!IsOwner) return;
        animator.SetBool(IS_RUNNING, player.isMoving);
        animator.SetBool(IS_ASLEEP, player.isAsleep.Value);
        animator.SetBool(IS_ELIMINATED, player.isEliminated.Value);
        //animator.
        //animator.SetBool(IS_ASLEEP, player.GetCurrentPlayerState() == Player.PlayerStates.asleep);
        //animator.SetBool(IS_DEAD, player.GetCurrentPlayerState() == Player.PlayerStates.death);
        
        
    }
}
