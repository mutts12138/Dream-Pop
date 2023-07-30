using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    //underscore the private variables.
    [SerializeField] private GameInput gameInput;
    [SerializeField] private DreamBubble dreamBubble;

    //team
    [SerializeField] private float teamNumber;
    [SerializeField] private float playerNumber;

    //player stats
    //should organize in struct
    private float playerRadius = 0.5f;
    private float playerHeight = 3f;

    public int maxBubble = 1;
    private int currentBubble = 0;
    

    private float moveSpeed = 10f;
    private float rotateSpeed = 15f;

    private float jumpAcc = 20f;
    private float gravityAcc = -100f;


    //player state
    private float verticalVelocity = 0f;
    private bool isRunning = false;
    private bool isGrounded = true;
    private bool isAsleep = false;
    private bool isDead = false;
    

    private bool canMove = true;
    private bool canJump = true;
    private bool canPlaceDB = true;
    private bool canUseAbility = true;
    private bool canUseItem = true;
    private bool canRespawn = true;

    const float asleepTimeBase = 6f;
    private float asleepTimer = asleepTimeBase;

    const float respawnTimerBase = 10f;
    private float respawnTimer = respawnTimerBase;
    


    private void Start()
    {
            currentBubble = 0;

        if(gameInput != null)
        {
            gameInput.OnPlaceBubble += GameInput_OnPlaceBubble;
            gameInput.OnJump += GameInput_OnJump;
        }
        
    }

    private void Update()
    {   
    
        if (canMove)
        {
            HandleMovement();
        }
        
        CheckIsGrounded();
        AddGravity();
        HandleVerticalVelocity();

        if (isAsleep)
        {
            HandleAsleep();
        }
        
    }

    //INCLUDE JUMPPING INPUT?
    //movement

    private void GameInput_OnJump(object sender, System.EventArgs e)
    {
   
        if (canJump)
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


    //place bubble
    private void GameInput_OnPlaceBubble(object sender, System.EventArgs e)
    {
        if (currentBubble < maxBubble){
            currentBubble++;
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
                dreamBubbleTransform.SetPlayer(this);
                 //no snap dreambubble
                    /*dreamBubbleTransform = Instantiate(dreamBubble);
                    dreamBubbleTransform.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                    */
             }


            }
        }

    }

    public void BubblePopped(){
        currentBubble--;
    }

    private void HandleMovement()
    {
        //for detecting collision
        float moveMaxDistance = moveSpeed * Time.deltaTime;

        //get movement input
        Vector2 inputVector = Vector3.zero;
        if (gameInput != null)
        {
             inputVector = gameInput.GetMovementVectorNormalized();
        }

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        //lerp face forward
        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }


        //player capsule
        Vector3 capsuleBotPoint = new Vector3(transform.position.x, transform.position.y + (playerRadius) + 0.01f, transform.position.z);
        Vector3 capsuleTopPoint = transform.position + (Vector3.up * (playerHeight - (playerRadius) - 0.01f));
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
            
        //set animation state
        isRunning = moveDir != Vector3.zero;
        
        
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



    

    


    private void HandleAsleep()
    {
        
            //check collision for enemy or ally
            CheckAsleepCollision();
            asleepTimer -= Time.deltaTime;
            if( asleepTimer < 0)
            {
                SetIsAsleep(false);
                
            }
        
    }


    private void CheckAsleepCollision()
    {
        float asleepRadius = playerRadius;

        int layerMask;
        int layerNumber = 3;
        layerMask = 1 << layerNumber;

        Collider[] asleepOverlap = Physics.OverlapSphere(transform.position + Vector3.up, asleepRadius * 2, layerMask);
        if(asleepOverlap.Length > 0)
        {
            if (asleepOverlap[0].gameObject.GetComponent<Player>().GetTeamNumber() == teamNumber)
            {
                //get awaken:saved
                Debug.Log("Saved");
                SetIsAsleep(false);
            }
            else
            {
                //get rude awaken:death
                Death();
                SetIsAsleep(false);
                
                
            }
        }
        
    }




    private void Death()
    {
        isDead = true;
        ChangeToDeathState();
        Debug.Log("death");
    }

    private void Respawn()
    {
        isDead = false;
        ChangeToNormalState();
        Debug.Log("respawn");
    }



    private void ChangeToNormalState()
    {
        canMove = true;
        canJump = true;
        canUseAbility = true;
        canUseItem = true;
        canPlaceDB = false;

        

        Collider m_Collider = GetComponent<Collider>();
        m_Collider.enabled = true;
    }

    private void ChangeToAsleepState()
    {
        canMove = false;
        canJump = false;
        canUseAbility = false;
        canUseItem = false;
        canPlaceDB = false;

    }

    private void ChangeToDeathState()
    {
        canMove = false;
        canJump = false;
        canUseAbility = false;
        canUseItem = false;
        canPlaceDB = false;

        

        Collider m_Collider = GetComponent<Collider>();
        m_Collider.enabled = false;


        if(canRespawn == true)
        {
            //trigger respawn timmer
        }

    }



    public float GetPlayerNumber()
    {
        return playerNumber;
    }
    public float GetTeamNumber()
    {
        return teamNumber;
    }

    public bool GetIsRunning()
    {
        return isRunning;
    }

    public bool GetIsAsleep()
    {
        return isAsleep;
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    //when hit by popExplosion
    public void SetIsAsleep(bool Asleep)
    {
        if (Asleep)
        {
            isAsleep = true;
            asleepTimer = asleepTimeBase;
            //"playerAsleep" = 7
            gameObject.layer = LayerMask.NameToLayer("playerAsleep");
            //set animation
            //movementspeed
            //disable ability and bubble placement
            ChangeToAsleepState();
        }
        else
        {
            isAsleep = false;
            //"player" = 3
            gameObject.layer = LayerMask.NameToLayer("player");
            //set animation
            //revert movementspeed
            //enable ability and bubble placement
            if(isDead == false)
            {
                ChangeToNormalState();
            }
            
        }
    }
}
