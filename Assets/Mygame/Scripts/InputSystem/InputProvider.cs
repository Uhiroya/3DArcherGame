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
    /// <summary> 移動入力監視用 </summary>
    public static Vector2 MoveDir => _moveDir;
    ////キーボード入力の場合入力変化がないため1度しか呼ばれないので、MoveDirを監視してもらう。
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
    
    class MyConditioner : MonoBehaviour
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
                StartCoroutine( ChangeMode(key.Item1, value));
            }
        }
        void Update()
        {
            switch (GA.Input._mode)
            {
                case InputModeType.InGame:
                    foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
                    {
                        GA.Input._inGameInputContainer[new InputType(ActionConditionContainer[new ActionUpdateMode(inputActionType, UpdateMode.Update)], inputActionType , UpdateMode.Update)]?.Invoke();
                    }
                    break;
                case InputModeType.Menu:
                    foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
                    {
                        GA.Input._menuInputContainer[new InputType(ActionConditionContainer[new ActionUpdateMode(inputActionType, UpdateMode.Update)], inputActionType, UpdateMode.Update)]?.Invoke();
                    }
                    break;
                case InputModeType.Other:
                    foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
                    {
                        GA.Input._otherInputContainer[new InputType(ActionConditionContainer[new ActionUpdateMode(inputActionType, UpdateMode.Update)], inputActionType, UpdateMode.Update)]?.Invoke();
                    }
                    break;
                default:
                    break;
            }
        }
        void FixedUpdate()
        {
            switch (GA.Input._mode)
            {
                case InputModeType.InGame:
                    foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
                    {
                        GA.Input._inGameInputContainer[new InputType(ActionConditionContainer[new ActionUpdateMode(inputActionType, UpdateMode.FixedUpdate)], inputActionType, UpdateMode.FixedUpdate)]?.Invoke();
                    }
                    break;
                case InputModeType.Menu:
                    foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
                    {
                        GA.Input._menuInputContainer[new InputType(ActionConditionContainer[new ActionUpdateMode(inputActionType, UpdateMode.FixedUpdate)], inputActionType, UpdateMode.FixedUpdate)]?.Invoke();
                    }
                    break;
                case InputModeType.Other:
                    foreach (InputActionType inputActionType in Enum.GetValues(typeof(InputActionType)))
                    {
                        GA.Input._otherInputContainer[new InputType(ActionConditionContainer[new ActionUpdateMode(inputActionType, UpdateMode.FixedUpdate)], inputActionType, UpdateMode.FixedUpdate)]?.Invoke();
                    }
                    break;
                default:
                    break;
            }
        }
        // Unityの各Updateフレームを待ったら次の状態に遷移したい。
        IEnumerator ChangeMode(ActionUpdateMode key, ExecuteType executeType)
        {
            switch (key.UpdateMode)
            {
                case UpdateMode.Update:
                    yield return new WaitForEndOfFrame();
                    break;
                case UpdateMode.FixedUpdate:
                    yield return new WaitForFixedUpdate();
                    break;
                default:
                    break;
            }
            switch (executeType)
            {
                case ExecuteType.Enter:
                    executeType = ExecuteType.Performed;
                    ActionConditionContainer[key] = executeType;
                    break;
                case ExecuteType.Exit:
                    executeType = ExecuteType.Waiting;
                    ActionConditionContainer[key] = executeType;
                    break;
                default:
                    break;
            }
        }
    }
    static MyConditioner _mc;
    //各コンテナ収容用
    Dictionary<InputType, Action> _inGameInputContainer = new();
    Dictionary<InputType, Action> _menuInputContainer = new();
    Dictionary<InputType, Action> _otherInputContainer = new();
    Dictionary<ActionUpdateMode, ExecuteType> @ActionConditionContainer => _mc.ActionConditionContainer;
    public bool GetKeyDown(InputActionType inputActionType, UpdateMode updateMode)
        => ActionConditionContainer[new ActionUpdateMode(inputActionType, updateMode)] == ExecuteType.Enter;
    public bool GetKey(InputActionType inputActionType, UpdateMode updateMode) 
        => ActionConditionContainer[new ActionUpdateMode(inputActionType, updateMode)] == ExecuteType.Performed;
    public bool GetKeyUp(InputActionType inputActionType, UpdateMode updateMode)
        => ActionConditionContainer[new ActionUpdateMode(inputActionType, updateMode)] == ExecuteType.Exit;
    /// <summary> コンストラクタ </summary>
    public InputProvider() => Initialize();
     InputController _controller = default;
    public InputController Controller => _controller;
     InputModeType _mode = InputModeType.InGame;
    /// <summary>/// 初期化を行う/// </summary>
    void Initialize()
    {
        var obj = new GameObject("PlayerInput");
        _mc = obj.AddComponent<MyConditioner>();
        _controller = new InputController();
        _controller.Enable();
        InitializeContainer();
        _controller.Player.Move.performed += context => { MoveCallback?.Invoke(context.ReadValue<Vector2>()); };
        _controller.Player.Move.canceled += context => MoveCallback?.Invoke(Vector2.zero);
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
                foreach (ExecuteType executeType in Enum.GetValues(typeof(ExecuteType)))
                {
                    _inGameInputContainer.Add(new InputType(executeType, inputActionType ,updateMode), null);
                    _menuInputContainer.Add(new InputType(executeType, inputActionType, updateMode), null);
                    _otherInputContainer.Add(new InputType(executeType, inputActionType, updateMode), null);
                }
                ActionConditionContainer.Add(new ActionUpdateMode(inputActionType, updateMode), ExecuteType.Waiting);
            }
        }
    }
    /// <summary>コンテナに登録されたコールバックの呼び出し/// </summary>
    void CheckContainer(InputModeType mode, ExecuteType excuteType, InputActionType inputActionType)
    {

        _mc[(new ActionUpdateMode(inputActionType, UpdateMode.Update),mode)] = excuteType;
        _mc[(new ActionUpdateMode(inputActionType, UpdateMode.FixedUpdate), mode)] = excuteType;
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
