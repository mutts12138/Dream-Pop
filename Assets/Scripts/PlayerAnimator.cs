using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Player player;

    private Animator animator;

    //animator parameters
    private const string IS_RUNNING = "isRunning";


    
    private void Awake()
    {
        animator = GetComponent <Animator>();
    }

   
    void Update()
    {
        animator.SetBool(IS_RUNNING, player.GetIsRunning());
    }
}
