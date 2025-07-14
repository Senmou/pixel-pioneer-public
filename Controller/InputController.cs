using System.Collections.Generic;
using UnityEngine.InputSystem;
using TarodevController;
using UnityEngine;
using System;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    public event EventHandler<InteractEventArgs> OnScrollWheel;
    public event EventHandler<InteractEventArgs> OnDoubleClicked;
    public event EventHandler<InteractEventArgs> OnShiftLeftClicked;
    public event EventHandler<InteractEventArgs> OnCtrlScrollWheel;
    public class InteractEventArgs : EventArgs
    {
        public InputAction.CallbackContext context;
    }

    public float HoldEscapeButtonDuration
    {
        get
        {
            return _inputActions.Intro.HoldEscape.GetTimeoutCompletionPercentage();
        }
    }
    private MyPlayerInputActions _inputActions;

    private void Awake()
    {
        Instance = this;

        _inputActions = new MyPlayerInputActions();
        _inputActions.UI_Actions.ScrollWheel.performed += ScrollWheel_Performed;
        _inputActions.UI_Actions.DoubleClick.performed += DoubleClick_Performed;
        _inputActions.UI_Actions.ShiftLeftClick.performed += ShiftLeftClick_Performed;
        _inputActions.Enable();
        _inputActions.UI_Actions.Enable();
    }

    private void OnDestroy()
    {
        _inputActions.Player_Actions.Disable();
        _inputActions.UI_Actions.Disable();
        _inputActions.Intro.Disable();

        _inputActions.UI_Actions.ScrollWheel.performed -= ScrollWheel_Performed;
        _inputActions.UI_Actions.DoubleClick.performed -= DoubleClick_Performed;
        _inputActions.UI_Actions.ShiftLeftClick.performed -= ShiftLeftClick_Performed;
        _inputActions.Dispose();
    }

    public void UseActionMap_Intro()
    {
        _inputActions.Player_Actions.Disable();
        _inputActions.UI_Actions.Disable();
        _inputActions.Intro.Enable();
    }

    public void UseActionMap_Player()
    {
        _inputActions.Player_Actions.Enable();
        _inputActions.UI_Actions.Enable();
        _inputActions.Intro.Disable();
    }

    public void UseActionMap_Rocket()
    {
        _inputActions.Player_Actions.Disable();
        _inputActions.UI_Actions.Disable();
        _inputActions.Intro.Disable();
    }

    public bool IsPressed_Escape => _inputActions.Intro.HoldEscape.IsPressed();
    public bool WasPressed_Escape => _inputActions.UI_Actions.Escape.WasPressedThisFrame();
    public bool WasPressed_BuildMenu => _inputActions.UI_Actions.BuildMenu.WasPressedThisFrame();
    public bool WasPressed_CharacterMenu => _inputActions.UI_Actions.CharacterMenu.WasPressedThisFrame();
    public bool IsJumpPressed => _inputActions.Player_Actions.Jump.IsPressed();
    public bool IsPressed_LeftMouseButton => _inputActions.Player_Actions.LeftClick.IsPressed();
    public bool WasPressed_LeftMouseButton => _inputActions.Player_Actions.LeftClick.WasPressedThisFrame();
    public bool IsPressed_RightMouseButton => _inputActions.Player_Actions.RightClick.IsPressed();
    public bool WasPressed_RightMouseButton => _inputActions.Player_Actions.RightClick.WasPressedThisFrame();
    public bool IsReleased_LeftMouseButton => _inputActions.Player_Actions.LeftClick.WasReleasedThisFrame();
    public bool IsReleased_RightMouseButton => _inputActions.Player_Actions.RightClick.WasReleasedThisFrame();
    public bool IsPressed_ShiftLeftClick => _inputActions.UI_Actions.ShiftLeftClick.WasPressedThisFrame();
    public bool WasPressed_DoubleClick => _inputActions.UI_Actions.DoubleClick.WasPerformedThisFrame();

    public bool GetPressedKey(List<KeyCode> keys, out KeyCode pressedKey)
    {
        foreach (var keyCode in keys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                pressedKey = keyCode;
                return true;
            }
        }
        pressedKey = KeyCode.None;
        return false;
    }

    public bool GetHoldKey(List<KeyCode> keys, out KeyCode holdKey)
    {
        foreach (var keyCode in keys)
        {
            if (Input.GetKey(keyCode))
            {
                holdKey = keyCode;
                return true;
            }
        }
        holdKey = KeyCode.None;
        return false;
    }

    private void ScrollWheel_Performed(InputAction.CallbackContext context)
    {
        if (Input.GetKey(KeyCode.LeftControl))
            OnCtrlScrollWheel?.Invoke(this, new InteractEventArgs { context = context });
        else
            OnScrollWheel?.Invoke(this, new InteractEventArgs { context = context });
    }

    private void DoubleClick_Performed(InputAction.CallbackContext context)
    {
        OnDoubleClicked?.Invoke(this, new InteractEventArgs { context = context });
    }

    private void ShiftLeftClick_Performed(InputAction.CallbackContext context)
    {
        OnShiftLeftClicked?.Invoke(this, new InteractEventArgs { context = context });
    }
}
