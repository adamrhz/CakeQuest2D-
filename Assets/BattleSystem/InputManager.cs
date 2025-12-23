using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using System.Linq;

public struct ButtonName
{
    public static string Move = "Move";
    public static string Navigate = "Navigate";
    public static string Interact = "Interact";
    public static string SecondAct = "SecondAct";
    public static string Pause = "Pause";
}

[RequireComponent(typeof(PlayerInput))]
public class InputManager : Controller
{
    public static InputManager inputManager;

    public static string controlSettings = "keyboard";
    public override void Awake()
    {
        inputManager = this;
        base.Awake();
    }
    public void SetMove()
    {
        Vector2 wasdInput = GetAxis2D("Move");

        // Round wasdInput to the closest cardinal or diagonal direction
        Vector2 roundedInput = RoundToCardinalDiagonal(wasdInput);
        Vector2 cardinalInput = RoundToCardinal(wasdInput);

        // Only update if movement is different from the previous movement
        if (roundedInput == Vector2.zero)
        {
            if (movement != Vector2.zero)
            {
                OnMovementStopped?.Invoke(roundedInput);
            }
        }
        else
        {
            if (movement == Vector2.zero || RoundToCardinal(movement) != cardinalInput)
            {
                OnMovementPressed?.Invoke(cardinalInput);
            }
        }

        if (movement != roundedInput)
        {
            movement = roundedInput;
            OnMovementHeld?.Invoke(roundedInput);
        }
    }

    private Vector2 RoundToCardinalDiagonal(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector2.zero;

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        angle = Mathf.Round(angle / 45f) * 45f; // Round to the nearest 45 degrees

        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    private Vector2 RoundToCardinal(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector2.zero;

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        angle = Mathf.Round(angle / 90f) * 90f; // Round to the nearest 90 degrees (cardinal directions only)

        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }


    public void OnInputChange(PlayerInput playerInput)
    {
        if (playerInput.currentControlScheme == "KeyboardControls")
        {
            controlSettings = "keyboard";
        }
        else
        {
            controlSettings = "controller";
        }
    }
    private void OnDestroy()
    {

        Destroy(GetComponent<PlayerInput>());
    }


    public void GetPauseButton()
    {
        GetButton(ButtonName.Pause);
    }


    public void Update()
    {

        if (GetButtonDown(ButtonName.Pause))
        {
            OnPausedPressed?.Invoke();
        }
        if (GetButtonUp(ButtonName.Pause))
        {
            OnPausedReleased?.Invoke();
        }

        if (canInteract)
        {

            if (GetButtonDown(ButtonName.Interact))
            {
                OnSelectPressed?.Invoke();
            }
            if (GetButtonUp(ButtonName.Interact))
            {
                OnSelectReleased?.Invoke();
            }
        }


        if (GetButtonDown(ButtonName.Interact))
        {
            OnSecretSelectPressed?.Invoke();
        }


        if (GetButtonDown(ButtonName.SecondAct))
        {
            OnReturnPressed?.Invoke();
        }
        if (GetButtonUp(ButtonName.SecondAct))
        {
            OnReturnReleased?.Invoke();
        }
        SetMove();
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpPressed?.Invoke();
        }
        if (context.canceled)
        {
            OnJumpRelease?.Invoke();
        }
        jump = context.action.triggered;
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (canInteract)
        {

            if (context.performed)
            {
                OnSelectPressed?.Invoke();
            }
            if (context.canceled)
            {
                OnSelectReleased?.Invoke();
            }
            attack = context.action.triggered;
        }


        if (context.performed)
        {
            OnSecretSelectPressed?.Invoke();
        }
    }

    public void OnReturn(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnReturnPressed?.Invoke();
        }
        if (context.canceled)
        {
            OnReturnReleased?.Invoke();
        }
        attack = context.action.triggered;
    }
    public void OnPause(InputAction.CallbackContext callback)
    {
        // Check if the interaction is a press (button down)
        if (callback.started)
        {
            OnPausedPressed?.Invoke();
        }
        if (callback.canceled)
        {
            OnPausedReleased?.Invoke();
        }
    }

}
