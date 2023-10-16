using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyInput;

public static partial class GA
{
    public static InputProvider Input = new();
}
public class InputProvider
{
    private static Vector2 _moveDir = Vector2.zero;
    public static Vector2 MoveDir => _moveDir;
    ///// <summary> 移動入力コールバック登録用 </summary>
    public UnityEvent<Vector2> MoveCallback = new();
    /// <summary> マウス入力コールバック登録用 </summary>
    public UnityEvent<Vector2> MouseCallback = new();
    /// <summary>/// ボタン入力コールバック登録用/// </summary>
    public void Regist(in Inputter inputter)
    {
        
        switch (inputter.InputMode)
        {
            case InputModeType.InGame:
                if (!_inGameInputContainer.ContainsKey(inputter.InputType))
                    _inGameInputContainer.Add(inputter.InputType , null);
                _inGameInputContainer[inputter.InputType] += inputter.Action;
                break;
            case InputModeType.Menu:
                if (!_menuInputContainer.ContainsKey(inputter.InputType))
                    _menuInputContainer.Add(inputter.InputType, null);
                _menuInputContainer[inputter.InputType] += inputter.Action;
                break;
            case InputModeType.Other:
                if (!_menuInputContainer.ContainsKey(inputter.InputType))
                    _menuInputContainer.Add(inputter.InputType, null);
                _otherInputContainer[inputter.InputType] += inputter.Action;
                break;
            default:
                break;
        }
    }
    /// <summary>/// ボタン入力コールバック登録解除用/// </summary>
    public void UnRegist(Inputter inputter)
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
    public void ModeChange(InputModeType inputMode) => _mode = inputMode;
    
    class MyInputUpdator : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        /// <summary> その他入力コールバック登録用コンテナ </summary>
        public Dictionary<ActionUpdateMode, ExecuteType> ActionConditionContainer = new();
        public ExecuteType this[(ActionUpdateMode , InputModeType) key]
        {
            get 
            { 
                return ActionConditionContainer[key.Item1]; 
            }
            set
            {
                ActionConditionContainer[key.Item1] = value;
                if(value == ExecuteType.Enter)
                    StartCoroutine(ChangeMode(key.Item1));
            }
        }
        void FixedUpdate()
        {
            GA.Input.MoveCallback?.Invoke(_moveDir);      
        }
        // Unityの各Updateフレームを待ったら次の状態に遷移したい。
        void InvokeCallBack(ExecuteType executeType ,InputActionType inputActionType , UpdateMode updateMode)
        {
            var inputType = new InputType(executeType,inputActionType,updateMode);
            switch (GA.Input._mode)
            {
                case InputModeType.InGame:
                    if (GA.Input._inGameInputContainer.ContainsKey(inputType))
                        GA.Input._inGameInputContainer[inputType]?.Invoke();
                    break;
                case InputModeType.Menu:
                    if (GA.Input._menuInputContainer.ContainsKey(inputType))
                        GA.Input._menuInputContainer[inputType]?.Invoke();
                    break;
                case InputModeType.Other:
                    if (GA.Input._otherInputContainer.ContainsKey(inputType))
                        GA.Input._otherInputContainer[inputType]?.Invoke();
                    break;
                default:
                    break;
            }
        }
        IEnumerator ChangeMode(ActionUpdateMode key)
        {
            while (true)
            {
                switch (ActionConditionContainer[key])
                {
                    case ExecuteType.Enter:
                        yield return WaitUpdateFrame(key);
                        ActionConditionContainer[key] = ExecuteType.Performed;
                        break;
                    case ExecuteType.Exit:
                        yield return WaitUpdateFrame(key);
                        ActionConditionContainer[key] = ExecuteType.Waiting;
                        yield break;
                    default:
                        yield return WaitUpdateFrame(key);
                        break;
                }
            }
        }
        IEnumerator WaitUpdateFrame(ActionUpdateMode key)
        {
            switch (key.UpdateMode)
            {
                case UpdateMode.Update:
                    InvokeCallBack(ActionConditionContainer[key], key.InputActionType, UpdateMode.Update);
                    yield return new WaitForEndOfFrame();
                    break;
                case UpdateMode.FixedUpdate:
                    InvokeCallBack(ActionConditionContainer[key], key.InputActionType, UpdateMode.FixedUpdate);
                    yield return new WaitForFixedUpdate();
                    break;
                default:
                    break;
            }
        }
    }
    static MyInputUpdator _mr;
    //各コンテナ収容用
    Dictionary<InputType, Action> _inGameInputContainer = new();
    Dictionary<InputType, Action> _menuInputContainer = new();
    Dictionary<InputType, Action> _otherInputContainer = new();
    Dictionary<ActionUpdateMode, ExecuteType> @ActionConditionContainer => _mr.ActionConditionContainer;
    /// <summary> コンストラクタ </summary>
    public InputProvider() => Initialize();
     InputController _controller = default;
    public InputController Controller => _controller;
     InputModeType _mode = InputModeType.InGame;
    /// <summary>/// 初期化を行う/// </summary>
    void Initialize()
    {
        var obj = new GameObject("PlayerInput");
        _mr = obj.AddComponent<MyInputUpdator>();
        _controller = new InputController();
        _controller.Enable();
        InitializeContainer();
        _controller.Player.Move.performed += context =>  _moveDir = context.ReadValue<Vector2>();
        _controller.Player.Move.canceled += context => { _moveDir = Vector2.zero; MoveCallback?.Invoke(Vector2.zero); };
        _controller.Player.Look.performed += context => MouseCallback?.Invoke(context.ReadValue<Vector2>());
        _controller.Player.Look.canceled += context => MouseCallback?.Invoke(Vector2.zero);
        _controller.Player.Jump.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Jump);
        _controller.Player.Jump.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Jump);
        _controller.Player.Fire1.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Fire1);
        _controller.Player.Fire1.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Fire1);
    }
    /// <summary>コンテナの初期化/// </summary>
    void InitializeContainer()
    {
        foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
        {
            foreach (UpdateMode updateMode in Enum.GetValues(typeof(UpdateMode)))
            {
                ActionConditionContainer.Add(new ActionUpdateMode(inputActionType, updateMode), ExecuteType.Waiting);
            }
        }
    }
    /// <summary>コンテナに登録されたコールバックの呼び出し/// </summary>
    void CheckContainer(InputModeType mode, ExecuteType excuteType, InputActionType inputActionType)
    {
        _mr[(new ActionUpdateMode(inputActionType, UpdateMode.Update),mode)] = excuteType;
        _mr[(new ActionUpdateMode(inputActionType, UpdateMode.FixedUpdate), mode)] = excuteType;
    }
}


namespace MyInput
{
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
        /// <summary> 入力待ち </summary>
        Waiting,
    }
    /// <summary>入力アクションの種類 </summary>
    public enum InputActionType
    {
        ///// <summary> 決定入力 </summary>
        //Submit,
        ///// <summary> キャンセル入力 </summary>
        //Cancel,
        /// <summary> ジャンプ入力 </summary>
        Jump,
        ///// <summary> モードチェンジ入力 </summary>
        //ChangeMode,
        ///// <summary> ターゲットチェンジ入力 </summary>
        //ChangeTarget,
        /// <summary> 攻撃入力１ </summary>
        Fire1,
        ///// <summary> 攻撃入力２ </summary>
        //Fire2,
        ///// <summary> 攻撃入力３ </summary>
        //Fire3,
        ///// <summary> 攻撃入力４ </summary>
        //Fire4,
        ///// <summary> ブースター入力 </summary>
        //Booster,
    }
    public enum UpdateMode
    {
        Update,
        FixedUpdate,
    }
    /// <summary>入力種別</summary>
    /// <param name="mode"></param><param name="executeType"></param><param name="inputActionType"></param>
    public struct InputType
    {
        private ExecuteType _excuteType;
        private ActionUpdateMode _actionUpdateMode;
        public ExecuteType ExecuteType => _excuteType;
        public ActionUpdateMode ActionUpdateMode => _actionUpdateMode;
        public InputType(ExecuteType excuteType, InputActionType inputActionType , UpdateMode update)
        {
            _excuteType = excuteType;
            _actionUpdateMode = new ActionUpdateMode(inputActionType , update);
        }
        public InputType(ExecuteType excuteType, ActionUpdateMode actionUpdateMode)
        {
            _excuteType = excuteType;
            _actionUpdateMode = actionUpdateMode;
        }
    }
    
    public struct ActionUpdateMode
    {
        private InputActionType _inputActionType;
        private UpdateMode _updateMode;
        public InputActionType InputActionType => _inputActionType;
        public UpdateMode UpdateMode => _updateMode;
        public ActionUpdateMode(InputActionType inputActionType, UpdateMode updateMode)
        {
            _inputActionType = inputActionType;
            _updateMode = updateMode;
        }
    }
    /// <summary>入力アクション識別用クラス</summary>
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
        public Inputter(InputModeType mode, InputActionType inputActionType, ExecuteType excuteType, UpdateMode updateMode, Action action)
        {
            _mode = mode;
            _inputType = new InputType(excuteType, inputActionType, updateMode);
            _action = action;
        }
    }

}
