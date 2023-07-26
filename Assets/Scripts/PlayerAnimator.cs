using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Player player;

    private Animator animator;

    //animator parameters
    private const string IS_RUNNING = "isRunning";
    private const string IS_ASLEEP = "isAsleep";
    private const string IS_DEAD = "isDead";


    private void Awake()
    {
        animator = GetComponent <Animator>();
    }

   
    void Update()
    {
        animator.SetBool(IS_RUNNING, player.GetIsRunning());
        animator.SetBool(IS_ASLEEP, player.GetIsAsleep());
        animator.SetBool(IS_DEAD, player.GetIsDead());
    }
}
