using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    private float rotateSpeed = 15f;
    private float jumpAcc = 25f;
    private float gravityAcc = -100f;
    private bool isGrounded = true;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private DreamBubble dreamBubble;

    private float playerRadius = 0.75f;
    private float playerHeight = 3f;
    private float verticalVelocity = 0f;

    private void Start()
    {
        gameInput.OnPlaceBubble += GameInput_OnPlaceBubble;
        gameInput.OnJump += GameInput_OnJump;
    }

    private void Update()
    {
        HandleMovement();
        CheckIsGrounded();
        AddGravity();
        HandleVerticalVelocity();
        //Debug.Log(verticalVelocity);
    }

    //INCLUDE JUMPPING INPUT?
    //movement
    

    private void HandleMovement()
    {
        //for detecting collision
        float moveMaxDistance = moveSpeed * Time.deltaTime;

        //get movement input
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        //lerp face forward
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);

        //player capsule
        Vector3 capsuleBotPoint = new Vector3(transform.position.x, transform.position.y + (playerRadius) + 0.01f, transform.position.z);
        Vector3 capsuleTopPoint = transform.position + (Vector3.up * (playerHeight - (playerRadius) -0.01f));
        float capsuleRadius = playerRadius;

        //detect collision
        bool canMove = !Physics.CapsuleCast(capsuleBotPoint, capsuleTopPoint, capsuleRadius, moveDir, moveMaxDistance);

        if (!canMove)
        {
            //test x only
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = !Physics.CapsuleCast(capsuleBotPoint, capsuleTopPoint, capsuleRadius, moveDirX, moveMaxDistance);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                //test z only
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
        
                canMove = !Physics.CapsuleCast(capsuleBotPoint, capsuleTopPoint, capsuleRadius, moveDirZ, moveMaxDistance);
                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }

        }
        
        if (canMove)
        {
            //execute movement
            transform.position += moveDir * moveMaxDistance;
        }
    }


    private void CheckIsGrounded()
    {
        if (!(verticalVelocity > 0))
        {
            if (!Physics.CapsuleCast(new Vector3(transform.position.x, transform.position.y + playerRadius + 0.01f, transform.position.z), transform.position + Vector3.up * playerHeight, playerRadius, Vector3.down, out RaycastHit raycastHit, 0.50f))
            {
                isGrounded = false;

            }
            else
            {
                if (isGrounded == false)
                {
                    verticalVelocity = 0f;
                    transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y), transform.position.z);
                }
                isGrounded = true;
            }
        } 
    }
    private void AddGravity()
    {
        if (!isGrounded)
        {
            float maxGravity = -10f;
            if(verticalVelocity > maxGravity)
            {
                verticalVelocity += gravityAcc * Time.deltaTime;
            }
        }
    }

    private void HandleVerticalVelocity()
    {
        transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
    }


    //place bubble
    private void GameInput_OnPlaceBubble(object sender, System.EventArgs e)
    {
        DreamBubble dreamBubbleTransform;


        //check if theres a dreambubble directly beneath the player
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2.01f, transform.position.z), Vector3.down, out RaycastHit raycastHit))
        {
            //check and get dreamBubble class
            if (raycastHit.transform.TryGetComponent(out dreamBubbleTransform))
            {
                //check if inflated
                /*if(dreamBubbleTransform.GetInflated() == false)
                {
                    dreamBubbleTransform.InflateBubble();
                }*/
            }
            else
            {
                //snap dreambubble
                dreamBubbleTransform = Instantiate(dreamBubble);
                dreamBubbleTransform.transform.position = new Vector3(MathF.Round(gameObject.transform.position.x / 2) * 2, gameObject.transform.position.y, MathF.Round(gameObject.transform.position.z / 2) * 2);
                
                //no snap dreambubble
                /*dreamBubbleTransform = Instantiate(dreamBubble);
                dreamBubbleTransform.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                */
            }


        }

    }

    private void GameInput_OnJump(object sender, System.EventArgs e)
    {

        if (isGrounded)
        {
            verticalVelocity += jumpAcc;
            //Debug.Log("Jumped");
            HandleVerticalVelocity();
            isGrounded = false;
        }
    }
}
