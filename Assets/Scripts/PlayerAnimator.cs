using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField] private Player player;

    private Animator animator;

    //animator parameters
    private const string IS_RUNNING = "isRunning";
    private const string IS_ASLEEP = "isAsleep";
    private const string IS_DEAD = "isDead";


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        animator = GetComponent <Animator>();
    }

   
    void Update()
    {
        if (!IsOwner) return;
        animator.SetBool(IS_RUNNING, player.GetIsRunning());
        animator.SetBool(IS_ASLEEP, player.GetCurrentPlayerState() == Player.PlayerStates.asleep);
        animator.SetBool(IS_DEAD, player.GetCurrentPlayerState() == Player.PlayerStates.death);
  
    }
}
