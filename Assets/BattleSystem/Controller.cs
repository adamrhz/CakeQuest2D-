using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Controller : NeoInputManager
{

    //public Vector2 wasdInput = Vector2.zero;
    public bool jump;
    public bool attack;

    public bool canControl = true;

    public bool canInteract = true;

    public delegate void EventHandler();


    public delegate void MovementHandler(Vector2 movement);
    public MovementHandler OnMovementPressed;
    public MovementHandler OnMovementHeld;
    public MovementHandler OnMovementStopped;


    public EventHandler OnJumpPressed;
    public EventHandler OnJumpRelease;
    public EventHandler OnReturnPressed;
    public EventHandler OnReturnReleased;
    public EventHandler OnSelectPressed;
    public EventHandler OnSelectReleased;


    public EventHandler OnSecretSelectPressed;

    public EventHandler OnPausedPressed;
    public EventHandler OnPausedReleased;


    public Vector2 movement;

    public override void Awake()
    {
        base.Awake();
    }
    public void CanInteract(bool _canInteract)
    {
        canInteract = _canInteract;
    }
    public void CanMove(bool _canMove)
    {
        canControl = _canMove;
    }
    public bool AttackContains(Action interact)
    {
        if (OnSelectPressed == null)
        {
            return false;
        }
        return OnSelectPressed.GetInvocationList().Contains(interact);
    }


}
