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
    public event EventHandler<CharacterBaseStatLevelChangeEventArgs> OnCharacterBaseStatLevelChange;
    public class CharacterBaseStatLevelChangeEventArgs : EventArgs
    {
        public int newMoveSpeedLevel;
        public int newBubblePowerLevel;
        public int newBubbleCountLevel;
    }

    public event EventHandler OnKill;
    public event EventHandler OnDeath;
    public event EventHandler OnSave;
    public event EventHandler OnBeingSaved;
    public event EventHandler OnRespawn;

    

    //underscore the private variables.
    private GameInput gameInput;
    [SerializeField] private DreamBubble dreamBubble;

    //team

    public NetworkVariable<ulong> ownerClientId { get; private set; }
    public NetworkVariable<int> teamNumber { get; private set; }

    //set via map data
    public Vector3 spawnPosition { get; private set; }
    private float gravityAcc;
    private float gravityMaxSpeed;


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
    public NetworkVariable<bool> isDead { get; private set; }


    
    private float baseMoveSpeed;
    private int currentMoveSpeedLevel;
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
        ownerClientId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        teamNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        isAsleep = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        //currentPlayerState = new NetworkVariable<PlayerStates>(PlayerStates.normal, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        currentLayer = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        movementLayerMaskIgnore = LayerMask.GetMask("pickUp");
        movementLayerMaskIgnore += LayerMask.GetMask("eliminated");
    }
    public override void OnNetworkSpawn()
    {
        

        //Get layermask list string name
        layersList = Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index)).Where(l => !string.IsNullOrEmpty(l)).ToArray();


        //event subscribe
        currentLayer.OnValueChanged += ChangeLayer;

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
        
        //set character and Stats
        SetInitialStats();
    }

     private void OnDisable()
    {
        
        
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


        //Set info based on mapdata if exist.
        ApplyMapData(teamNumber.Value);
    }

    private void ApplyMapData(int teamNumber)
    {
        if (MapData.Instance != null)
        {
            //spawnPosition
            foreach (SpawnPoint spawnPoint in MapData.Instance.GetSpawnPoints())
            {
                if (spawnPoint.GetIsTaken() == false && spawnPoint.GetTeamNumber() == teamNumber)
                {
                    spawnPosition = spawnPoint.transform.position;
                    gameObject.transform.position = spawnPosition;
                }
            }

            //over map property: gravity
            gravityAcc = MapData.Instance.GetGlobalGravityAcc();
            gravityMaxSpeed = MapData.Instance.GetGlobalGravityMaxSpeed();
            
        }
        else
        {
            
            spawnPosition = Vector3.zero;

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
            Debug.Log(currentBubblePowerLevel);
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
            SpawnDreamBubbleObject(dreamBubbleLocation, bubblePowerLevel);
            ChangeBubbleCountClientRpc(1);
        }

        //no snap dreambubble
        /*dreamBubbleTransform = Instantiate(dreamBubble);
        dreamBubbleTransform.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        */
    }

    private void SpawnDreamBubbleObject(Vector3 spawnPosition, int bubblePowerLevel)
    {
        DreamBubble dreamBubbleTransform = Instantiate(dreamBubble);
        dreamBubbleTransform.transform.position = spawnPosition;
        dreamBubbleTransform.GetComponent<NetworkObject>().Spawn(true);

        //Debug.Log(this);
        

        dreamBubbleTransform.SetPlayer(this);
        dreamBubbleTransform.SetPopPowerDistance(bubblePowerLevel);
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
        OnCharacterBaseStatLevelChange?.Invoke(this, new CharacterBaseStatLevelChangeEventArgs { newBubbleCountLevel = currentBubbleCountLevel, newMoveSpeedLevel = currentMoveSpeedLevel, newBubblePowerLevel = currentBubblePowerLevel });
    }

    //make sure only the server calls this
    public void SetCurrentLayer(int newLayer)
    {
        currentLayer.Value = newLayer;
    }

    //subscribed to currentlayer.valuechange
    public void ChangeLayer(int previousLayerNumber, int newLayerNumber)
    {

        gameObject.layer = LayerMask.NameToLayer(layersList[newLayerNumber]);
        Debug.Log("playerObject on layer" + gameObject.layer);
    }

    //called when ever a change to stats or buffs/debuffs happen
    




    //get and set

    public void SetOwnerClientId(ulong clientID)
    {
        this.ownerClientId.Value = clientID;
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


    [ClientRpc]
    public void AddToMoveDisableStackClientRpc(int deltaValue)
    {
        moveDisableStack += deltaValue;
    }
    [ClientRpc]
    public void AddToPlaceDreamBubbleDisableStackClientRpc(int deltaValue)
    {
        placeDreamBubbleDisableStack += deltaValue;
    }
    [ClientRpc]
    public void AddToUseAbilityDisableStackClientRpc(int deltaValue)
    {
        useAbilityDisableStack += deltaValue;
    }
    [ClientRpc]
    public void AddToUseItemDisableStackClientRpc(int deltaValue)
    {
        useItemDisableStack += deltaValue;
    }
    [ClientRpc]
    public void AddToInvincibleStackClientRpc(int deltaValue)
    {
        invincibleStack += deltaValue;
    }


    public void SetIsAsleep(bool deltaValue)
    {
        isAsleep.Value = deltaValue;
    }




    public void InvokeKill()
    {
        OnKill?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeDeath()
    {
        isDead.Value = true;

        OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeRespawn()
    {
        isDead.Value = false;

        OnRespawn?.Invoke(this, EventArgs.Empty);
        Debug.Log("respawn");
    }

    public void InvokeSave()
    {
        OnSave?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeBeingSaved()
    {
        OnBeingSaved?.Invoke(this, EventArgs.Empty);
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

    public override void OnDestroy()
    {
        if (gameInput != null)
        {
            gameInput.OnPlaceBubble -= GameInput_OnPlaceBubble;
        }

        currentLayer.OnValueChanged -= ChangeLayer;


        if (IsServer)
        {
            if ( gameObject.GetComponent<NetworkObject>().IsSpawned == true ) 
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
                
            }
            
        }

        base.OnDestroy();
    }
}
