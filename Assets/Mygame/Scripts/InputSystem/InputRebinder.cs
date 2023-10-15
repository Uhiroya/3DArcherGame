using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputRebinder
{
    static InputActionRebindingExtensions.RebindingOperation _rebindOperation = null;
    public static string FindKeyName(string actionName) => 
        InputProvider.Controller.FindAction(actionName).GetBindingDisplayString();
    public static int GetKeyBindIndex(string actionName) =>
        InputProvider.Controller.FindAction(actionName).GetBindingIndex();

    public static void ReBind(string actionName , Action<string> displayAction)
    {
        InputProvider.Controller.Disable();
        _rebindOperation = InputProvider.Controller.FindAction(actionName).
            PerformInteractiveRebinding()
            .WithTargetBinding(GetKeyBindIndex(actionName))
            .OnComplete( _ => {
                displayAction(FindKeyName(actionName));
                FinishBinding();
                })
            .OnCancel( _ => FinishBinding())
            .Start();
    }
    static void FinishBinding()
    {
        _rebindOperation.Dispose();
        _rebindOperation = null;
        InputProvider.Controller.Enable();
    }
}
