using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;


public class Player : NetworkBehaviour
{
    //underscore the private variables.
    private GameInput gameInput;
    [SerializeField] private DreamBubble dreamBubble;

    //team
    private NetworkVariable<int> teamNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    

    [SerializeField] private float playerNumber;

    //player stats
    //should organize in struct
    const float playerRadius = 0.5f;
    const float playerHeight = 3f;
    const float rotateSpeed = 15f;
    const float jumpAcc = 20f;
    const float gravityAcc = -100f;

    private float moveSpeed = 10f;

    //player state
    //normal: 0, asleep: 1, death: 2
    public enum PlayerStates 
    {   normal,
        asleep,
        death
    };
    
    private NetworkVariable<PlayerStates> currentPlayerState = new NetworkVariable<PlayerStates>(PlayerStates.normal, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    private float verticalVelocity = 0f;
    private bool isRunning = false;
    private bool isGrounded = true;


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



    public override void OnNetworkSpawn()
    {
        //event subscribe
        if (!IsOwner) return;

        currentPlayerState.OnValueChanged += (PlayerStates previousState, PlayerStates newState) => { ApplyPlayerState(); };

        //connect input if owner
        gameInput = FindObjectOfType<GameInput>();

        if(gameInput != null)
        {
            gameInput.OnPlaceBubble += GameInput_OnPlaceBubble;
            gameInput.OnJump += GameInput_OnJump;
        }
        
    }

    private void Update()
    {
        if (!IsOwner) return;

        //updates and computes transform if owner
        //else networktransform will synchonize
        if (canMove)
        {
            HandleMovement();
        }
        
        CheckIsGrounded();
        AddGravity();
        HandleVerticalVelocity();

        //player state

        if (currentPlayerState.Value == PlayerStates.asleep)
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

        //check if theres a dreambubble directly beneath the player
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2.01f, transform.position.z), Vector3.down, out RaycastHit raycastHit))
        {
            //check and get dreamBubble class
            if (raycastHit.transform.TryGetComponent(out DreamBubble outDreamBubble))
            {
                //check if inflated
                /*if(dreamBubbleTransform.GetInflated() == false)
                {
                    dreamBubbleTransform.InflateBubble();
                }*/
            }
            else
            {
                PlaceDreamBubbleServerRpc();
            }


        }

    }

    [ServerRpc]
    private void PlaceDreamBubbleServerRpc()
    {
        //snap dreambubble
        DreamBubble dreamBubbleTransform = Instantiate(dreamBubble);
        dreamBubbleTransform.transform.position = new Vector3(MathF.Round(gameObject.transform.position.x / 2) * 2, gameObject.transform.position.y, MathF.Round(gameObject.transform.position.z / 2) * 2);
        dreamBubbleTransform.GetComponent<NetworkObject>().Spawn(true);

        //no snap dreambubble
        /*dreamBubbleTransform = Instantiate(dreamBubble);
        dreamBubbleTransform.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        */
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
                SetCurrentPlayerState(PlayerStates.normal);   
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
            if (asleepOverlap[0].gameObject.GetComponent<Player>().GetTeamNumber() == teamNumber.Value)
            {
                //get awaken:saved
                Debug.Log("Saved");
                SetCurrentPlayerState(PlayerStates.normal);
            }
            else
            {
                //get rude awaken:death
                SetCurrentPlayerState(PlayerStates.death);
                
                
            }
        }
        
    }


    private void Respawn()
    {
        ChangeToNormalState();
        Debug.Log("respawn");
    }


    private void ApplyPlayerState()
    {
        switch (currentPlayerState.Value)
        {
            case PlayerStates.normal:
                ChangeToNormalState();
                
                break;
            case PlayerStates.asleep:
                ChangeToAsleepState();
                
                break;
            case PlayerStates.death:
                ChangeToDeathState();
                
                break;
        }
    }
    
    private void ChangeToNormalState()
    {
        //"player" = 3
        gameObject.layer = LayerMask.NameToLayer("player");

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
        //"playerAsleep" = 7
        gameObject.layer = LayerMask.NameToLayer("playerAsleep");

        canMove = false;
        canJump = false;
        canUseAbility = false;
        canUseItem = false;
        canPlaceDB = false;

        asleepTimer = asleepTimeBase;

    }

    private void ChangeToDeathState()
    {
        //"player" = 3
        gameObject.layer = LayerMask.NameToLayer("player");

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

        return teamNumber.Value;
        
    }

    
    public void SetTeamNumber(int newTeamNumber)
    {
        teamNumber.Value = newTeamNumber;
        Debug.Log(GetTeamNumber());
    }

    public bool GetIsRunning()
    {
        return isRunning;
    }

 

    public PlayerStates GetCurrentPlayerState()
    {
        return currentPlayerState.Value;
    }

    public void SetCurrentPlayerState(PlayerStates newPlayerState)
    {
        if (!IsOwner) return;
        currentPlayerState.Value = newPlayerState;

    }

    [ClientRpc]
    public void SetCurrentPlayerStateClientRpc(PlayerStates newPlayerState)
    {
        if (!IsOwner) return;
        currentPlayerState.Value = newPlayerState;

    }

    

}
