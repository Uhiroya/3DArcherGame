using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.StandaloneInputModule;
using UnityEngine.InputSystem.Users;

public class InputProvider : SingletonBase<InputProvider> 
{
    /// <summary> その他入力コールバック登録用 </summary>
    static Dictionary<InputType, Action> _inGameInputContainer = new();
    static Dictionary<InputType, Action> _menuInputContainer = new();
    static Dictionary<InputType, Action> _otherInputContainer = new();
    static InputController _controls = default;
    static InputModeType _mode = InputModeType.InGame;

    /// <summary> 移動入力コールバック登録用 </summary>
    public static UnityEvent<Vector2> MoveCallback = new();
    /// <summary> マウス入力コールバック登録用 </summary>
    public static UnityEvent<Vector2> MouseCallback = new();
    /// <summary>/// ボタン入力コールバック登録用/// </summary>
    public static void Regist(in Inputter inputter)
    {
        switch (inputter.InputMode)
        {
            case InputModeType.InGame:
                _inGameInputContainer[inputter.InputType] += inputter.Action;
                break;
            case InputModeType.Menu:
                _menuInputContainer[inputter.InputType] += inputter.Action;
                break;
            case InputModeType.Other:
                _otherInputContainer[inputter.InputType] += inputter.Action;
                break;
            default:
                break;
        }
    }
    /// <summary>/// ボタン入力コールバック登録解除用/// </summary>
    public static void UnRegist(Inputter inputter)
    {
        switch (inputter.InputMode)
        {
            case InputModeType.InGame:
                _inGameInputContainer[inputter.InputType] = null;
                break;
            case InputModeType.Menu:
                _menuInputContainer[inputter.InputType] = null;
                break;
            case InputModeType.Other:
                _otherInputContainer[inputter.InputType] = null;
                break;
            default:
                break;
        }
    }
    /// <summary>入力モード変更/// </summary>
    public static void ModeChange(InputModeType inputMode) => _mode = inputMode;
    protected override void AwakeFunction() => Initialize();
    /// <summary>/// 初期化を行う/// </summary>
    void Initialize()
    {
        _controls = new InputController();
        _controls.Enable();
        InitializeContainer();
        _controls.Player.Move.performed += context => { MoveCallback?.Invoke(context.ReadValue<Vector2>()); };
        MoveCallback.AddListener((Vector2) => print(Vector2));
        _controls.Player.Move.canceled += context => MoveCallback?.Invoke(Vector2.zero);
        _controls.Player.Look.performed += context => MouseCallback?.Invoke(context.ReadValue<Vector2>());
        _controls.Player.Look.canceled += context => MouseCallback?.Invoke(Vector2.zero);
        _controls.Player.Jump.performed += context => CheckContainer(_mode, ExecuteType.Enter , InputActionType.Jump);
        _controls.Player.Jump.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Jump);
        _controls.Player.Fire1.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Fire1);
        _controls.Player.Fire1.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Fire1);
    }
    /// <summary>コンテナの初期化/// </summary>
    void InitializeContainer()
    {
        foreach (ExecuteType executeType in Enum.GetValues(typeof(ExecuteType))) 
        {
            foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
            {
                _inGameInputContainer.Add(new InputType(executeType ,inputActionType), null);
                _menuInputContainer.Add(new InputType(executeType ,inputActionType), null);
                _otherInputContainer.Add(new InputType(executeType ,inputActionType), null);
            }
        }
    }
    /// <summary>コンテナに登録されたコールバックの呼び出し/// </summary>
    void CheckContainer(InputModeType mode ,ExecuteType excuteType , InputActionType inputActionType)
    {
        switch (mode)
        {
            case InputModeType.InGame:
                _inGameInputContainer[new InputType(excuteType,inputActionType)]?.Invoke();
                break;
            case InputModeType.Menu:
                _menuInputContainer[new InputType(excuteType,inputActionType)]?.Invoke();
                break;
            case InputModeType.Other:
                _otherInputContainer[new InputType(excuteType,inputActionType)]?.Invoke();
                break;
            default:
                break;
        }
    }
    
}
/// <summary>ゲーム中のモード</summary>
public enum InputModeType
{
    /// <summary> ゲーム中操作モード </summary>
    InGame,
    /// <summary> メニュー等ＵＩ操作モード </summary>
    Menu,
    /// <summary> その他一時実行時の操作モード </summary>
    Other,
}
/// <summary>実行タイプ</summary>
public enum ExecuteType
{
    /// <summary> 入力時 </summary>
    Enter,
    /// <summary> 入力中 </summary>
    Performed,
    /// <summary> 入力終了時 </summary>
    Exit,
}
/// <summary>入力アクションの種類 </summary>
public enum InputActionType
{
    /// <summary> 決定入力 </summary>
    Submit,
    /// <summary> キャンセル入力 </summary>
    Cancel,
    /// <summary> ジャンプ入力 </summary>
    Jump,
    /// <summary> モードチェンジ入力 </summary>
    ChangeMode,
    /// <summary> ターゲットチェンジ入力 </summary>
    ChangeTarget,
    /// <summary> 攻撃入力１ </summary>
    Fire1,
    /// <summary> 攻撃入力２ </summary>
    Fire2,
    /// <summary> 攻撃入力３ </summary>
    Fire3,
    /// <summary> 攻撃入力４ </summary>
    Fire4,
    /// <summary> ブースター入力 </summary>
    Booster,
}
/// <summary>入力種別</summary>
/// <param name="mode"></param><param name="executeType"></param><param name="inputActionType"></param>
public struct InputType
{
    private ExecuteType _excuteType;
    private InputActionType _inputActionType;
    public ExecuteType ExecuteType => _excuteType;
    public InputActionType InputActionType => _inputActionType;
    public InputType(ExecuteType excuteType, InputActionType inputActionType)
    {
        _excuteType = excuteType;
        _inputActionType = inputActionType;
    }
}
public class Inputter
{
    InputType _inputType;
    InputModeType _mode;
    Action _action;
    public InputType InputType => _inputType;
    public InputModeType InputMode => _mode;
    public Action Action => _action;
    public Inputter(InputModeType mode, InputType inputType, Action action)
    {
        _mode = mode;
        _inputType = inputType;
        _action = action;
    }
    public Inputter(InputModeType mode, InputActionType inputActionType, ExecuteType excuteType, Action action)
    {
        _mode = mode;
        _inputType = new InputType(excuteType, inputActionType);
        _action = action;
    }
}