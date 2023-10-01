using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput instance;

    private PlayerInputActions playerInputActions;

    public event EventHandler OnPlaceBubble;
    public event EventHandler OnChargingBubble;

    public event EventHandler OnJump;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
       
        playerInputActions.Player.PlaceBubble.performed += PlaceBubble_performed;
        playerInputActions.Player.Jump.performed += Jump_performed;
    }

    private void OnDisable()
    {
        playerInputActions.Player.PlaceBubble.performed -= PlaceBubble_performed;
        playerInputActions.Player.Jump.performed -= Jump_performed;
    }

    public void EnablePlayerInputAction(bool enable)
    {
        if (enable)
        {
            playerInputActions.Player.Enable();
        }
        else
        {
            playerInputActions.Player.Disable();
        }
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnJump?.Invoke(this, EventArgs.Empty);
        
    }

 

    private void PlaceBubble_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPlaceBubble?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;
        return inputVector;
    }

    
}
