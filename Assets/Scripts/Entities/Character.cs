using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviour
{

    public static Character Player;
    CharacterBehaviour playerBehaviour;
    Movement playerMovement;
    public CharacterInventory inventory;
    public bool canGetInteract = true;
    public Vector2 input = Vector2.zero;
    //public event Action OnInteractEvent;
    public Controller inputManager;
    public Party heroParty;
    private CharacterBehaviour previousBehaviour;
    public SpriteRenderer Sprite;

    [Header("Components to Enable/Disable")]
    [SerializeField] private List<Component> componentsToToggle;


    // Start is called before the first frame update
    private void Awake()
    {
        playerMovement = GetComponent<Movement>();
        inputManager = GetComponent<Controller>();



        ToggleNothingState();
        if (inputManager)
        {
            Player = this;
        }
        FadeScreen.AddOnEndFadeEvent(TogglePlayableState);


    }

    public void DisableCharacter()
    {
        foreach (var component in componentsToToggle)
        {
            if (component is Behaviour behaviour)
            {
                behaviour.enabled = false;
            }
            else if (component is Renderer renderer)
            {
                renderer.enabled = false;
            }
        }
    }

    public void EnableCharacter()
    {
        foreach (var component in componentsToToggle)
        {
            if (component is Behaviour behaviour)
            {
                behaviour.enabled = true;
            }
            else if (component is Renderer renderer)
            {
                renderer.enabled = true;
            }
        }
    }

    public void SetPosition(Vector2 newPosition)
    {
        transform.position = newPosition;
    }
    public void ActivateControls(bool on = true)
    {
        if (on)
        {

            inputManager.OnReturnPressed += delegate { Run(true); };
            inputManager.OnReturnReleased += delegate { Run(false); };
            inputManager.OnPausedPressed += delegate { PauseMenu.Singleton?.OnPausePressed(); };

        }
        else
        {

            inputManager.OnReturnPressed = null;
            inputManager.OnReturnReleased = null;
            inputManager.OnPausedPressed = null;
        }
    }

    public void Move(Vector2 input)
    {
        playerMovement.SetInput(input);
    }

    public void CanMove(bool v)
    {
        if (v)
        {
            inputManager.OnMovementHeld += Move;
        }
        else
        {
            inputManager.OnMovementHeld -= Move;
        }
    }

    public static void ActivatePlayer()
    {
        Player.TogglePlayableState();
    }

    public static void DeactivatePlayer()
    {
        Player.ToggleNothingState();
    }

    public bool CanInteraction()
    {
        return canGetInteract;
    }

    public void SetInteraction(bool interaction)
    {
        canGetInteract = interaction;
    }

    public void AddToInventory(InventoryItem content, int amount = 1)
    {
        if (inventory)
        {
            inventory.AddToInventory(content, amount);
        }
    }

    public bool HasObject(InventoryItem content, int amount = 1)
    {

        return inventory.HasObject(content, amount);
    }


    public int AmountObject(InventoryItem content)
    {
        return inventory.AmountObject(content);

    }
    public bool RemoveFromInventory(InventoryItem content, int amount = 1)
    {


        return inventory.RemoveFromInventory(content, amount);
    }

    public Direction GetFacing()
    {
        Vector2 vector = playerMovement.lookDirection;
        if (vector.y > 0)
            return Direction.Top;
        else if (vector.y < 0)
            return Direction.Bottom;
        else if (vector.x > 0)
            return Direction.Right;
        else if (vector.x < 0)
            return Direction.Left;
        else
            return Direction.Bottom;
    }

    public void Run(bool running)
    {
        playerMovement.Run(running);
    }

    public void LookToward(Vector2 direction)
    {
        playerMovement.LookAt(direction);
    }

    public void LookAt(GameObject target)
    {

        LookToward((target.transform.position - transform.position).normalized);
    }



    public void SetPosition(Vector3 newPosition)
    {
        if (!playerMovement)
        {
            playerMovement = GetComponent<Movement>();
        }


        playerMovement?.SetPosition(newPosition);
    }

    public string GetState()
    {
        return GetCurrentBehaviour().GetType().ToString();
    }

    public void ChangeState(CharacterBehaviour newBehaviour)
    {

        playerBehaviour?.OnExit();
        previousBehaviour = playerBehaviour;
        playerBehaviour = newBehaviour;
        LogStateChange(previousBehaviour);
        newBehaviour.OnEnter(this);
    }
    public void LogStateChange(CharacterBehaviour previousBehaviour)
    {
        string state = "No State";

        if (previousBehaviour != null)
        {
            state = previousBehaviour.GetType().ToString();
        }
        if (this == Player && ConsoleToGui.doShow) Debug.Log($"{name}  State changed : {state} replaced by {GetCurrentBehaviour()}", this);
    }
    public CharacterBehaviour GetCurrentBehaviour()
    {
        return playerBehaviour;
    }

    private void FixedUpdate()
    {
        playerBehaviour.Handle();
    }

    public void TogglePlayableState()
    {
        if (Player == this)
        {
           ChangeState(new PlayerControlsBehaviour());
            

        }
        else
        {

            ChangeState(new PatrollingBehaviour());

        }
    }

    public void ToggleInteractionState()
    {
        ChangeState(new InteractingBehaviour());
    }


    public void ToggleCutsceneState()
    {
        ChangeState(new NothingBehaviour());
    }

    public void TogglePreviousState()
    {
        ChangeState(previousBehaviour);
    }

    public void ToggleNothingState()
    {
        ChangeState(new NothingBehaviour());
    }

    public void CutsceneMoving(Vector3 startPosition, Vector3 destination)
    {
        if(Vector3.Distance(startPosition, destination) > 0.01f)
        {
            Vector2 direction = (destination - startPosition).normalized;
            playerMovement?.SetInput(direction);

        }
        else
        {
            playerMovement?.SetInput(Vector3.zero);
        }
    }
}
