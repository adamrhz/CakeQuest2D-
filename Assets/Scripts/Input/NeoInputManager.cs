using System;

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;


//Input System used in BonbowBon. Courtesy of Strix

public static class InputUtilities
{
    public static Vector2 DiagonalizeVector2(this Vector2 inputVector)
    {
        if (inputVector.x != 0 && inputVector.y != 0)
        {
            if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
            {
                inputVector.y = 0;
            }
            else
            {
                inputVector.x = 0;
            }
            inputVector.Normalize();
        }
        return inputVector;
    }
}

public static class GameInput
{
    public static float GetAxis(string axisName) => NeoInputManager.Instance.GetAxis(axisName);
    public static Vector2 GetAxis2D(string axisName) => NeoInputManager.Instance.GetAxis2D(axisName);

    public static bool GetButton(string buttonName) => NeoInputManager.Instance.GetButton(buttonName);
    public static bool GetButtonDown(string buttonName) => NeoInputManager.Instance.GetButtonDown(buttonName);
    public static bool GetButtonUp(string buttonName) => NeoInputManager.Instance.GetButtonUp(buttonName);

    //Local Functions

    public static float GetAxis(this NeoInputManager Input, string axisName) => Input.GetAxis(axisName);
    public static Vector2 GetAxis2D(this NeoInputManager Input, string axisName) => Input.GetAxis2D(axisName);

    public static bool GetButton(this NeoInputManager Input, string buttonName) => Input.GetButton(buttonName);
    public static bool GetButtonDown(this NeoInputManager Input, string buttonName) => Input.GetButtonDown(buttonName);
    public static bool GetButtonUp(this NeoInputManager Input, string buttonName) => Input.GetButtonUp(buttonName);

    //-----------------------------------------------------------------------------------------//
    public static void BlockInput() => NeoInputManager.Instance.BlockInput = true;
    public static void UnblockInput() => NeoInputManager.Instance.BlockInput = false;

    public static PlayerInput InputObject;
}

public enum InputDeviceType
{
    Keyboard, Mouse, Dualsense, Xbox
}

public class NeoInputManager : MonoBehaviour
{
    public PlayerInput MainInput;

    public List<UButton> Buttons;
    public List<UAxis1D> Axis1D;
    public List<UAxis2D> Axis2D;

    [CanBeNull] public static NeoInputManager Instance;

    public bool BlockInput;

    public string CurrentMap;


    public void ChangeMap(string MapNameOrID)
    {
        MainInput.SwitchCurrentActionMap(MapNameOrID);
        CurrentMap = MapNameOrID;
    }
    public virtual void Awake()
    {
        MainInput = GetComponent<PlayerInput>();
        MainInput.onActionTriggered += OnActionTriggered;
        CurrentMap = MainInput.currentActionMap.name;
        Buttons = new List<UButton>(); Axis1D = new List<UAxis1D>(); Axis2D = new List<UAxis2D>();
        foreach (InputActionMap a in MainInput.actions.actionMaps)
        {
            foreach (InputAction s in a.actions)
            {
                switch (s.type)
                {
                    case InputActionType.Button:
                        UButton button = new UButton();
                        button.button = s;
                        button.Name = s.name;
                        button.MapName = a.name;
                        Buttons.Add(button);
                        break;

                    case InputActionType.Value:
                        switch (s.expectedControlType)
                        {
                            case "Axis":
                                UAxis1D axis1 = new UAxis1D();
                                axis1.Name = s.name;
                                axis1.MapName = a.name;
                                Axis1D.Add(axis1);
                                break;
                            case "Vector2":
                                UAxis2D axis2 = new UAxis2D();
                                axis2.Name = s.name;
                                axis2.MapName = a.name;
                                Axis2D.Add(axis2);
                                break;
                        }
                        break;
                }
            }
        }
    }

    internal UButton UBGet(string name, string map) => Buttons.Find(s => s.Name == name && s.MapName == map);
    internal string UBGetMap(string name) => Buttons.Find(s => s.Name == name).MapName;
    internal UAxis1D UA1Get(string name, string map) => Axis1D.Find(s => s.Name == name && s.MapName == map);
    internal string UA1GetMap(string name) => Axis1D.Find(s => s.Name == name).MapName;
    internal UAxis2D UA2Get(string name, string map) => Axis2D.Find(s => s.Name == name && s.MapName == map);
    internal string UA2GetMap(string name) => Axis2D.Find(s => s.Name == name).MapName;

    private void Update()
    {
        if (!Instance)
            Instance = this;
    }

    private void OnActionTriggered(InputAction.CallbackContext callbackContext)
    {
        if (CurrentMap != callbackContext.action.actionMap.name && CurrentMap != callbackContext.action.actionMap.id.ToString()) return;

        switch (callbackContext.action.type)
        {
            case InputActionType.Button:
                try
                {
                    UBGet(callbackContext.action.name, callbackContext.action.actionMap.name).hold = callbackContext.ReadValueAsButton();
                }
                catch
                {
                    //do absolutely nothing because I don't want to log an error out even though this works so IDK why its throwing errors to begin with
                }
                break;
            case InputActionType.Value:
                switch (callbackContext.action.expectedControlType)
                {
                    case "Axis":
                        UA1Get(callbackContext.action.name, callbackContext.action.actionMap.name).Value = callbackContext.ReadValue<float>();
                        break;
                    case "Vector2":
                        Vector2 Direction = callbackContext.ReadValue<Vector2>();
                        Vector2 oldDirection = UA2Get(callbackContext.action.name, callbackContext.action.actionMap.name).Value;
                        UA2Get(callbackContext.action.name, callbackContext.action.actionMap.name).Value = Direction;
                        bool newDirection = Direction.DiagonalizeVector2() != oldDirection.DiagonalizeVector2();
                        if (newDirection) { UA2Get(callbackContext.action.name, callbackContext.action.actionMap.name).hold = false;}
                        UA2Get(callbackContext.action.name, callbackContext.action.actionMap.name).hold = newDirection;
                        break;
                }
                break;
        }
    }

    public bool GetButtonDown(string name)
    {
        try
        {
            if (!BlockInput)
                return UBGet(name, CurrentMap).pressed;
            else return false;
        }
        catch
        {
            return false;
        }
    }

    public bool GetButtonUp(string name)
    {
        try
        {
            if (!BlockInput)
                return UBGet(name, CurrentMap).released;
            else return false;
        }
        catch
        {
            return false;
        }
    }

    public bool GetButton(string name)
    {
        try
        {
            if (!BlockInput)
                return UBGet(name, CurrentMap).hold;
            else return false;
        }
        catch
        {
            return false;
        }
    }

    public float GetAxis(string name)
    {
        try
        {
            if (!BlockInput)
                return UA1Get(name, CurrentMap).Value;
            else return 0;
        }
        catch
        {
            return 0;
        }
    }

    public Vector2 GetAxis2D(string name)
    {
        try
        {
            if (!BlockInput)
                return UA2Get(name, CurrentMap).Value;
            else return Vector2.zero;
        }
        catch
        {
            return Vector2.zero;
        }
    }
    public bool GetAxis2DDown(string name)
    {
        try
        {
            if (!BlockInput)
                if (UA2Get(name, CurrentMap).pressed)
                    return UA2Get(name, CurrentMap).pressed;
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool GetAnyButton(bool bypassesBlock = false)
    {
        try
        {
            if (!BlockInput || bypassesBlock)


                foreach (UButton button in Buttons)
                {
                    if (button.pressed)
                    {
                        return true;
                    }
                }
            return false;
        }
        catch
        {
            return false;
        }
    }
}


public class ButtonInfo
{
    public string Name;
    public string MapName;
    public float PressedTime, FPressedTime, ReleasedTime, FReleasedTime;
    [HideInInspector]
    public bool hold
    {
        get => _hold;
        set
        {
            if (_hold == value) return;
            _hold = value;
            if (_hold)
            {
                PressedTime = Time.unscaledTime;
                FPressedTime = Time.fixedUnscaledTime;
            }
            else
            {
                ReleasedTime = Time.unscaledTime;
                FReleasedTime = Time.fixedUnscaledTime;
            }
        }
    }
    public bool upressed;

    public bool pressed => !Time.inFixedTimeStep ? PressedTime == Time.unscaledTime : FPressedTime == Time.fixedUnscaledTime;
    public bool released => !Time.inFixedTimeStep ? ReleasedTime == Time.unscaledTime : FReleasedTime == Time.fixedUnscaledTime;

    bool _hold;
}

public class UAxis1D : ButtonInfo
{
    public float Value;
}
[System.Serializable]
public class UAxis2D : ButtonInfo
{
    public Vector2 Value;
}
[System.Serializable]
public class UButton : ButtonInfo
{
    public InputAction button;
    public Action press, release;

}