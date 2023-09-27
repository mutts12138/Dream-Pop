using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;


public class PlayerCharacter : NetworkBehaviour
{
    //event
    public event EventHandler<CharacterBaseStatLevelChangeEventArgs> onCharacterBaseStatLevelChange;
    public event EventHandler onEliminated;

    public class CharacterBaseStatLevelChangeEventArgs : EventArgs
    {
        public int newMoveSpeedLevel;
        public int newBubblePowerLevel;
        public int newBubbleCountLevel;
    }




    //underscore the private variables.
    private GameInput gameInput;
    [SerializeField] private DreamBubble dreamBubble;

    //team

    public NetworkVariable<ulong> ownerClientID { get; private set; }
    public NetworkVariable<int> teamNumber { get; private set; }




    //player stats
    //const
    private const float playerRadius = 0.5f;
    private const float playerHeight = 2.5f;
    private const float rotateSpeed = 15f;
    private const float jumpAcc = 20f;

    //variables, make these stacks
    //== 0 is enabled, != 0 is disabled
    private int moveDisableStack;
    private int placeDreamBubbleDisableStack;
    private int useAbilityDisableStack;
    private int useItemDisableStack;

    private int invincibleStack;

    public NetworkVariable<bool> isAsleep { get; private set; }
    public NetworkVariable<bool> isEliminated { get; private set; }




    private Vector3 spawnPosition;

    
    private float baseMoveSpeed;
    public int currentMoveSpeedLevel;
    private int minMoveSpeedLevel;
    private int maxMoveSpeedLevel;

    //currentBubbleCount used to count bubbles players placed in scene
    private int currentBubbleCount;
    
    private int currentBubbleCountLevel;
    private int minBubbleCountLevel;
    private int maxBubbleCountLevel;

    //bubble power
    private int currentBubblePowerLevel;
    private int minBubblePowerLevel;
    private int maxBubblePowerLevel;
   


    //resource in using abilities
    private int dreamFragCount;
    private int maxDreamFragCount;

    //movement collision detection layermask
    LayerMask movementLayerMaskIgnore;
    

    //gravity numbers get from game manager
    private float gravityAcc;
    private float gravityMaxSpeed;



    //player state
    //normal: 0, asleep: 1, death: 2
    /*
    public enum PlayerStates 
    {   normal,
        asleep,
        death
    };*/
    private string[] layersList;
    private NetworkVariable<int> currentLayer;

    //not yet implemented
    //private NetworkVariable<bool> playerColliderEnabled = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float verticalVelocity = 0f;


    //player state for player animator
    public bool isMoving { get; private set; }
    public bool isGrounded { get; private set; }



    private void Awake()
    {
        ownerClientID = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        teamNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        isAsleep = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        isEliminated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        //currentPlayerState = new NetworkVariable<PlayerStates>(PlayerStates.normal, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        currentLayer = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        movementLayerMaskIgnore = LayerMask.GetMask("pickUp");
        movementLayerMaskIgnore += LayerMask.GetMask("eliminated");
    }
    public override void OnNetworkSpawn()
    {
        

        //Get layermask list string name
        layersList = Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index)).Where(l => !string.IsNullOrEmpty(l)).ToArray();


        //event subscribe
        currentLayer.OnValueChanged += (int previousLayer, int newLayer) => { ChangeLayer(currentLayer.Value); };

        //if owner
        if (!IsOwner) return;

        //ownerClientID.Value = NetworkManager.Singleton.LocalClientId;
        //connect game manager

        //connect input 
        gameInput = FindObjectOfType<GameInput>();

        if (gameInput != null)
        {
            gameInput.OnPlaceBubble += GameInput_OnPlaceBubble;
            //gameInput.OnJump += GameInput_OnJump;
        }

        //event subscribe
        //currentPlayerState.OnValueChanged += (PlayerStates previousState, PlayerStates newState) => { ApplyPlayerState(); };


        if(GameMultiplayer.Instance != null)
        {
            teamNumber.OnValueChanged += (int previousTeamNumber, int newTeamNumber) => { GameMultiplayer.Instance.SetPlayerTeamNumberServerRpc(ownerClientID.Value, newTeamNumber); };
        }
        
        //set character and Stats
        SetInitialStats();
    }

     private void OnDisable()
    {
        currentLayer.OnValueChanged -= (int previousLayer, int newLayer) => { ChangeLayer(currentLayer.Value); };
        if (gameInput != null)
        {
            gameInput.OnPlaceBubble -= GameInput_OnPlaceBubble;
        }
        
        //currentPlayerState.OnValueChanged -= (PlayerStates previousState, PlayerStates newState) => { ApplyPlayerState(); };
    }

    private void SetInitialStats()
    {
        //set character and abilities
        baseMoveSpeed = 5f;
        //set initial max stats : movementspeed,bubble count, bubble power
        maxBubbleCountLevel = 10;
        maxBubblePowerLevel = 10;
        maxMoveSpeedLevel = 10;
        //set initial stats : movementspeed,bubble count, bubble power
        CallChangeCharacterBaseStatLevelsServerRpc(3, 2, 2);


        //gravity
        if(MapData.Instance != null)
        {
            gravityAcc = MapData.Instance.GetGlobalGravityAcc();
            gravityMaxSpeed = MapData.Instance.GetGlobalGravityMaxSpeed();
        }
        else
        {
            gravityAcc = -100f;
            gravityMaxSpeed = -10f;
        }
        

        
    }


    private void Update()
    {
        if (IsServer)
        {
            
        }
        

        if (!IsOwner) return;

        //updates and computes transform if owner
        //else networktransform will synchonize
        if (moveDisableStack == 0)
        {
            HandleMovement();
        }
        
        CheckIsGrounded();
        AddGravity();
        HandleVerticalVelocity();

        

        //player state

        //make this coroutine
        /*
        if (currentPlayerState.Value == PlayerStates.asleep)
        {
            HandleAsleep();
        }
        */
        
    }

    //INCLUDE JUMPPING INPUT?
    //movement
    private void HandleMovement()
    {
        //for detecting collision
        //7 == pickup
        //int layerNumber = 7;
        
        
        //layerMaskIgnore = 1 << layerNumber;


        float moveMaxDistance = baseMoveSpeed * currentMoveSpeedLevel * Time.deltaTime;

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
        bool canMove = !Physics.CapsuleCast(capsuleBotPoint, capsuleTopPoint, capsuleRadius, moveDir, moveMaxDistance, ~movementLayerMaskIgnore);

        if (!canMove)
        {
            //test x only
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = !Physics.CapsuleCast(capsuleBotPoint, capsuleTopPoint, capsuleRadius, moveDirX, moveMaxDistance, ~movementLayerMaskIgnore);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                //test z only
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = !Physics.CapsuleCast(capsuleBotPoint, capsuleTopPoint, capsuleRadius, moveDirZ, moveMaxDistance, ~movementLayerMaskIgnore);
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
        isMoving = moveDir != Vector3.zero;


    }

    /*
    private void GameInput_OnJump(object sender, System.EventArgs e)
    {
        if (jumpDisableStack == 0)
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
    */

    //place bubble
    private void GameInput_OnPlaceBubble(object sender, System.EventArgs e)
    {
        //Debug.Log(currentBubbleCount);
        //Debug.Log(currentBubbleCountLevel);
        //check to see if player is grounded
        //check to see if player has reached bubbleCountLimit
        if (placeDreamBubbleDisableStack != 0) return;

        if (currentBubbleCount < currentBubbleCountLevel)
        {
            PlaceDreamBubbleServerRpc(currentBubblePowerLevel);
        }

        
    }

    [ServerRpc]
    private void PlaceDreamBubbleServerRpc(int bubblePowerLevel)
    {
        
        //make it active.deactive, transform out of sight, avoid instantiate.destroy, instantiate only when bubbleUp
        //snap dreambubble
        //check if a dreambubble already exist at current location
        Vector3 dreamBubbleLocation = new Vector3(MathF.Round(gameObject.transform.position.x / 2) * 2, gameObject.transform.position.y +1, MathF.Round(gameObject.transform.position.z / 2) * 2);

        int layerMask;
        int layerNumber = 6;
        layerMask = 1 << layerNumber;

        if(Physics.OverlapSphere(dreamBubbleLocation, 0.5f, layerMask).Length <= 0)
        {
            
            //GameMultiplayer.Instance.SpawnDreamBubbleObject();
            SpawnDreamBubbleObject();
            ChangeBubbleCountClientRpc(1);
        }

        //no snap dreambubble
        /*dreamBubbleTransform = Instantiate(dreamBubble);
        dreamBubbleTransform.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        */
    }

    private void SpawnDreamBubbleObject()
    {
        DreamBubble dreamBubbleTransform = Instantiate(dreamBubble);
        dreamBubbleTransform.transform.position = transform.position;
        dreamBubbleTransform.GetComponent<NetworkObject>().Spawn(true);

        //Debug.Log(this);
        

        dreamBubbleTransform.SetPlayer(this);
        dreamBubbleTransform.SetPopPowerDistance(currentBubblePowerLevel);
    }

    [ClientRpc]
    public void ChangeBubbleCountClientRpc(int deltaBubbleCount)
    {
        if (!IsOwner) return;
        currentBubbleCount += deltaBubbleCount;
        Mathf.Clamp(currentBubbleCount, 0, currentBubbleCountLevel);
    }

    [ClientRpc]
    public void InflateBubblePushUpClientRpc()
    {
        if (!IsOwner) return;
        transform.position += new Vector3(0, 2, 0);
    }






    private void CheckIsGrounded()
    {
        

        if (!(verticalVelocity > 0))
        {
            if (!Physics.CapsuleCast(new Vector3(transform.position.x, transform.position.y + playerRadius + 0.01f, transform.position.z), transform.position + Vector3.up * playerHeight, playerRadius, Vector3.down, out RaycastHit raycastHit, 0.50f, ~movementLayerMaskIgnore))
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




   


    //is this unnecessay
    [ServerRpc]
    public void CallChangeCharacterBaseStatLevelsServerRpc(int deltaMoveSpeedLevel, int deltaBubbleCountLevel, int deltaBubblePowerLevel)
    {
        CallChangeCharacterBaseStatLevelsClientRpc(deltaMoveSpeedLevel, deltaBubbleCountLevel, deltaBubblePowerLevel);
    }

    [ClientRpc]
    public void CallChangeCharacterBaseStatLevelsClientRpc(int deltaMoveSpeedLevel, int deltaBubbleCountLevel, int deltaBubblePowerLevel)
    {
        ChangeCharacterBaseStatLevels(deltaMoveSpeedLevel, deltaBubbleCountLevel, deltaBubblePowerLevel);
    }

    public void ChangeCharacterBaseStatLevels(int deltaMoveSpeedLevel, int deltaBubbleCountLevel, int deltaBubblePowerLevel)
    {
        if (!IsOwner) return;

        currentMoveSpeedLevel += deltaMoveSpeedLevel;
        Mathf.Clamp(currentMoveSpeedLevel, 0, maxMoveSpeedLevel);

        currentBubbleCountLevel += deltaBubbleCountLevel;
        Mathf.Clamp(currentBubbleCountLevel, 0, maxBubbleCountLevel);

        currentBubblePowerLevel += deltaBubblePowerLevel;
        Mathf.Clamp(currentBubblePowerLevel, 0, maxBubblePowerLevel);

        //updates UI
        onCharacterBaseStatLevelChange?.Invoke(this, new CharacterBaseStatLevelChangeEventArgs { newBubbleCountLevel = currentBubbleCountLevel, newMoveSpeedLevel = currentMoveSpeedLevel, newBubblePowerLevel = currentBubblePowerLevel });
    }

    

    /*
    private void HandleAsleep()
    {

        //check collision for enemy or ally
        
        asleepTimer -= Time.deltaTime;

        if (asleepTimer < 0)
        {
            SetCurrentPlayerState(PlayerStates.normal);
        }
        else
        {
            CheckAsleepCollision();
        }
        
            
        
    }

    
    
        
    }*/




    private void Respawn()
    {
        //ChangeToNormalState();
        Debug.Log("respawn");
    }




    /*
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
        //ChangeLayer(3);
        currentLayer.Value = 3;

        canMove = true;
        //canJump = true;
        canUseAbility = true;
        canUseItem = true;
        canPlaceDB = false;

        Collider m_Collider = GetComponent<Collider>();
        m_Collider.enabled = true;
    }

    //make it into a debuff
    private void ChangeToAsleepState()
    {
        //"playerAsleep" = 7
        //ChangeLayer(7);
        currentLayer.Value = 7;

        canMove = false;
        //canJump = false;
        canUseAbility = false;
        canUseItem = false;
        canPlaceDB = false;

        asleepTimer = asleepTimeBase;

    }

    //make it into a debuff
    private void ChangeToDeathState()
    {
        //"playerDead" = 8
        //ChangeLayer(8);
        currentLayer.Value = 8;

        

        canMove = false;
        //canJump = false;
        canUseAbility = false;
        canUseItem = false;
        canPlaceDB = false;

        //remove buffs and debuffs

        Collider m_Collider = GetComponent<Collider>();
        m_Collider.enabled = false;


        if(canRespawn == true)
        {
            //trigger respawn timmer
        }

    }
    */

    public void SetCurrentLayer(int newLayer)
    {
        currentLayer.Value = newLayer;
    }

    //subscribed to currentlayer.valuechange
    public void ChangeLayer(int layerNumber)
    {

        gameObject.layer = LayerMask.NameToLayer(layersList[layerNumber]);
        Debug.Log("playerObject on layer" + gameObject.layer);
    }

    //called when ever a change to stats or buffs/debuffs happen
    




    //get and set

    public void SetOwnerClientId(ulong clientID)
    {
        this.ownerClientID.Value = clientID;
    }


    [ServerRpc]
    public void SetTeamNumberServerRpc(int newTeamNumber)
    {
        teamNumber.Value = newTeamNumber;
        //Debug.Log(GetTeamNumber());
    }

    public void SetTeamNumber(int newTeamNumber)
    {
        teamNumber.Value = newTeamNumber;
        //Debug.Log(GetTeamNumber());
    }



    //player status get and set
    /*
     * private int MoveDisableStack;

    private int PlaceDreamBubbleDisableStack;
    private int UseAbilityDisableStack;
    private int UseItemDisableStack;

    private bool isEliminated;
    private bool canRespawn;
    */
    public int GetMoveDisableStack()
    {
        return moveDisableStack;
    }

    public int GetPlaceDreamBubbleDisableStack()
    {
        return placeDreamBubbleDisableStack;
    }

    public int GetUseAbilityDisableStack()
    {
        return useAbilityDisableStack;
    }

    public int GetUseItemDisableStack()
    {
        return useItemDisableStack;
    }

    public int GetInvincibleStack()
    {
        return invincibleStack;
    }

    private bool GetIsEliminated()
    {
        return isEliminated.Value;
    }

    public void AddToMoveDisableStack(int deltaValue)
    {
        moveDisableStack += deltaValue;
    }
    public void AddToPlaceDreamBubbleDisableStack(int deltaValue)
    {
        placeDreamBubbleDisableStack += deltaValue;
    }
    public void AddToUseAbilityDisableStack(int deltaValue)
    {
        useAbilityDisableStack += deltaValue;
    }
    public void AddToUseItemDisableStack(int deltaValue)
    {
        useItemDisableStack += deltaValue;
    }

    public void AddToInvincibleStack(int deltaValue)
    {
        invincibleStack += deltaValue;
    }

    public void SetIsAsleep(bool deltaValue)
    {
        isAsleep.Value = deltaValue;
    }

    public void SetIsEliminated(bool deltaValue)
    {
        isEliminated.Value = deltaValue;
        if(deltaValue )
        {
            onEliminated?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /*
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

    }*/

    [ClientRpc]
    public void SetPlayerPositionClientRpc(Vector3 newPosition)
    {
        if (!IsOwner) return;
        transform.position = newPosition;
    }
     
   
}
