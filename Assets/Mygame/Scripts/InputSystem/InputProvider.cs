using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyInput;

public static partial class GA
{
    public static InputProvider Input = new();
}
public class InputProvider
{

    //private static Vector2 _moveDir = Vector2.zero;
    //private static Vector2 _mouseDir = Vector2.zero;
    //private static float _scrollValue = 0f;
    /////// <summary> 移動入力コールバック登録用 </summary>
    //public Action<Vector2> MoveCallback = null;
    //public Action<Vector2> MoveFixedCallback = null;
    ///// <summary> 始点入力コールバック登録用 </summary>
    //public Action<Vector2> LookCallback = null;
    //public Action<Vector2> LookFixedCallback = null;
    //public Action<float> LookOffsetCallback = null;
    //public Action<float> LookOffsetFixedCallback = null;
    public bool IsNotRegisted(Inputter inputter)
    {
        if (!_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _actionConditionContainer.Add(inputter.InputType, new MyConditierValue(ExecuteType.Waiting , null));
            return true;
        }
        return false;
    }

    ///// <summary>/// ボタン入力コールバック登録用/// </summary>
    public void Regist(Inputter inputter , Action action)
    {
        if (IsNotRegisted(inputter) || !_inputContainers.ContainsKey(inputter))
        {
            _inputContainers.Add(inputter, null);
        }
        _inputContainers[inputter] += action;
    }
    public void Regist(Inputter inputter, Action<float> action)
    {
        if (IsNotRegisted(inputter) || !_inputValueContainers.ContainsKey(inputter))
        {
            _inputValueContainers.Add(inputter, null);
            _inputValueDic.Add(inputter, 0f);
        }
        _inputValueContainers[inputter] += action;
    }
    public void Regist(Inputter inputter, Action<Vector2> action)
    {
        if (IsNotRegisted(inputter) || !_inputVector2Containers.ContainsKey(inputter))
        {
            _inputVector2Containers.Add(inputter, null);
            _inputVector2Dic.Add(inputter, Vector2.zero);
        }
        _inputVector2Containers[inputter] += action;
    }
    public void Regist(List<(Inputter , Action)> inputterList)
    {
        foreach(var inputter  in inputterList)
        {
            Regist(inputter.Item1 , inputter.Item2);
        }
    }
    /// <summary>/// ボタン入力コールバック登録解除用/// </summary>
    public void UnRegist(Inputter inputter, Action action)
    {
        if (_inputContainers.ContainsKey(inputter))
        {
            _inputContainers[inputter] -= action;
        } 
    }
    public void UnRegist(Inputter inputter, Action<float> action)
    {
        if (_inputValueContainers.ContainsKey(inputter))
        {
            _inputValueContainers[inputter] -= action;
        }
    }
    public void UnRegist(Inputter inputter, Action<Vector2> action)
    {
        if (_inputVector2Containers.ContainsKey(inputter))
        {
            _inputVector2Containers[inputter] -= action;
        }
    }
    public void UnRegist(List<(Inputter, Action)> inputterList)
    {
        foreach (var inputter in inputterList)
        {
            UnRegist(inputter.Item1, inputter.Item2);
        }
    }
    /// <summary>入力モード変更/// </summary>
    public void ModeChange(InputModeType inputMode) => _mode = inputMode;
    class MyConditierValue
    {
        public ExecuteType ExecuteType;
        public Coroutine Coroutine;
        public MyConditierValue(ExecuteType executeType, Coroutine coroutine)
        {
            ExecuteType = executeType;
            Coroutine = coroutine;
        }
    }
    private class MyInputUpdator : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        /// <summary> その他入力コールバック登録用コンテナ </summary>

        public ExecuteType this[InputType key]
        {
            get
            {
                return _actionConditionContainer[key].ExecuteType;
            }
            set
            {
                ////Alwaysは初回のみ起動
                //if (_actionConditionContainer[key] != ExecuteType.Always && value == ExecuteType.Always)
                //    StartCoroutine(InputCallBack(key));
                _actionConditionContainer[key].ExecuteType = value;
                //Enterの時のみコルーチンを起動
                if (_actionConditionContainer[key].Coroutine == null && (value.HasFlag(ExecuteType.Enter) || value.HasFlag(ExecuteType.Waiting)))
                    _actionConditionContainer[key].Coroutine =  StartCoroutine(InputCallBack(key));      
            }
        }
        // Unityの各Updateフレームを待ったら次の状態に遷移したい。
        IEnumerator InputCallBack(InputType key)
        {
            while (true)
            {
                yield return WaitUpdateFrame(key);
                
                switch (_actionConditionContainer[key].ExecuteType)
                {
                    case ExecuteType.Enter:    
                        _actionConditionContainer[key].ExecuteType = ExecuteType.Performed;
                        break;
                    case ExecuteType.Performed:
                        break;
                    case ExecuteType.Exit:
                        _actionConditionContainer[key].ExecuteType = ExecuteType.Waiting;
                        break;
                    case ExecuteType.Waiting:
                        //コンテナにキーがないときコルーチンを抜ける
                        Inputter invokker = new Inputter(key, _actionConditionContainer[key].ExecuteType);
                        if (!_inputContainers.ContainsKey(invokker) &&
                            !_inputValueContainers.ContainsKey(invokker) &&
                            !_inputVector2Containers.ContainsKey(invokker))
                        {
                            _actionConditionContainer[key].Coroutine = null;
                            yield break;
                        }
                        else
                            break;
                    default:                      
                        break;
                }

            }
        }
        IEnumerator WaitUpdateFrame(InputType key)
        {
            switch (key.UpdateMode)
            {
                case UpdateMode.Update:          
                    yield return new WaitForEndOfFrame();
                    InvokeCallBack(key);
                    break;
                case UpdateMode.FixedUpdate:
                    yield return new WaitForFixedUpdate();
                    InvokeCallBack(key);
                    break;
                default:
                    break;
            }
        }
        void InvokeCallBack(InputType key)
        {
            //print(_actionConditionContainer[key]);
            Inputter invoker = new Inputter(key, _actionConditionContainer[key].ExecuteType);
            if (_inputContainers.ContainsKey(invoker))
                _inputContainers[invoker]?.Invoke();
            if (_inputValueContainers.ContainsKey(invoker))
                _inputValueContainers[invoker]?.Invoke(_inputValueDic[invoker]);
            if (_inputVector2Containers.ContainsKey(invoker))
                _inputVector2Containers[invoker]?.Invoke(_inputVector2Dic[invoker]);
        }
    }
    static MyInputUpdator _mir;
    static Dictionary<InputType, MyConditierValue> _actionConditionContainer = new();
    Dictionary<InputType, MyConditierValue> ActionConditionContainer => _actionConditionContainer;
    private static Dictionary<Inputter, float> _inputValueDic = new();
    private static Dictionary<Inputter, Vector2> _inputVector2Dic = new();
    static Dictionary<Inputter, Action> _inputContainers = new();
    static Dictionary<Inputter, Action<float>> _inputValueContainers = new();
    static Dictionary<Inputter, Action<Vector2>> _inputVector2Containers = new();

    
    public bool GetKeyDown(Inputter inputter)
    => ActionConditionContainer[inputter.InputType].ExecuteType == ExecuteType.Enter;
    public bool GetKey(Inputter inputter)
        => ActionConditionContainer[inputter.InputType].ExecuteType == ExecuteType.Performed;
    public bool GetKeyUp(Inputter inputter)
        => ActionConditionContainer[inputter.InputType].ExecuteType == ExecuteType.Exit;
    /// <summary> コンストラクタ </summary>
    public InputProvider() => Initialize();
     InputController _controller = default;
    public InputController Controller => _controller;
    static InputModeType _mode = InputModeType.InGame;
    public static InputModeType Mode => _mode;
    /// <summary>/// 初期化を行う/// </summary>
    void Initialize()
    {
        var obj = new GameObject("PlayerInput");
        _mir = obj.AddComponent<MyInputUpdator>();
        _controller = new InputController();
        _controller.Enable();
        //InitializeContainer();
        _controller.InputMap.Move.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Move ,context.ReadValue<Vector2>());
        _controller.InputMap.Move.canceled += context => CheckContainer(ExecuteType.Exit, ActionType.Move, Vector2.zero);
        _controller.InputMap.Look.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Look, context.ReadValue<Vector2>());
        _controller.InputMap.Look.canceled += context => CheckContainer(ExecuteType.Exit, ActionType.Look, Vector2.zero);
        _controller.InputMap.Scroll.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Scroll, context.ReadValue<float>());
        _controller.InputMap.Scroll.canceled += context => CheckContainer(ExecuteType.Exit, ActionType.Scroll, 0f);
        _controller.InputMap.Jump.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Jump);
        _controller.InputMap.Jump.canceled += context => CheckContainer(ExecuteType.Exit, ActionType.Jump);
        _controller.InputMap.Fire1.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Fire1);
        _controller.InputMap.Fire1.canceled += context => CheckContainer(ExecuteType.Exit, ActionType.Fire1);
        _controller.InputMap.Zoom.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Zoom);
        _controller.InputMap.Zoom.canceled += context => CheckContainer(ExecuteType.Exit, ActionType.Zoom);
        _controller.InputMap.Cancel.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Cancel);
        _controller.InputMap.Cancel.canceled += context => CheckContainer(ExecuteType.Exit, ActionType.Cancel);
    }
    /// <summary>コンテナに登録されたコールバックの呼び出し/// </summary>
    void CheckContainer( ExecuteType executeType , ActionType inputActionType)
    {
        InputType inputType = new InputType(_mode, inputActionType,UpdateMode.Update);
        if (_actionConditionContainer.ContainsKey(inputType))
        {
            _mir[inputType] = executeType;
        }
        inputType = new InputType(_mode, inputActionType, UpdateMode.FixedUpdate);
        if (_actionConditionContainer.ContainsKey(inputType))
        {
            _mir[inputType] = executeType;
        }
    }
    void CheckContainer(ExecuteType executeType, ActionType inputActionType, float value)
    {
        Inputter inputter = new Inputter(_mode, inputActionType, executeType, UpdateMode.Update);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputValueDic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
        inputter = new Inputter(_mode, inputActionType, executeType, UpdateMode.FixedUpdate);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputValueDic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
    }
    void CheckContainer(ExecuteType executeType, ActionType inputActionType, Vector2 value)
    {
        Inputter inputter = new Inputter(_mode, inputActionType, executeType, UpdateMode.Update);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputVector2Dic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
        inputter = new Inputter(_mode, inputActionType, executeType, UpdateMode.FixedUpdate);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputVector2Dic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
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
    [Flags]
    public enum ExecuteType
    {
        /// <summary> 入力時 </summary>
        Enter = 1 << 0,
        /// <summary> 入力中 </summary>
        Performed = 1 << 1,
        /// <summary> 入力終了時 </summary>
        Exit = 1 << 2,
        /// <summary> 入力待ち </summary>
        Waiting = 1 << 3,
        /// <summary> 常に </summary>
        Always = Enter | Performed | Exit  | Waiting,
    }
    /// <summary>入力アクションの種類 </summary>
    public enum ActionType
    {
        ///// <summary> 決定入力 </summary>
        //Submit,
        /// <summary> ジャンプ入力 </summary>
        Jump,
        /// <summary> キャンセル入力 </summary>
        Cancel,

        ///// <summary> モードチェンジ入力 </summary>
        //ChangeMode,
        ///// <summary> ターゲットチェンジ入力 </summary>
        //ChangeTarget,
        /// <summary> 攻撃入力１ </summary>
        Fire1,
        /// <summary> ズーム　右クリ </summary>
        Zoom,
        /// <summary> カメラオフセット </summary>
        Scroll,
        ///// <summary> 攻撃入力２ </summary>
        //Fire2,
        ///// <summary> 攻撃入力３ </summary>
        //Fire3,
        ///// <summary> 攻撃入力４ </summary>
        //Fire4,
        ///// <summary> ブースター入力 </summary>
        //Booster,
        Move,
        Look,

    }
    public enum UpdateMode
    {
        Update,
        FixedUpdate,
    }
    /// <summary>入力種別</summary>
    /// <param name="mode"></param><param name="executeType"></param><param name="inputActionType"></param></summary>
    public struct InputType 
    {
        private InputModeType _inputModeType;
        private ActionType _inputActionType;
        private UpdateMode _updateMode;
        public InputModeType InputModeType => _inputModeType;
        public ActionType InputActionType => _inputActionType;
        
        public UpdateMode UpdateMode => _updateMode;
        public InputType(InputModeType inputModeType ,ActionType inputActionType ,UpdateMode updateMode)
        {
            _inputModeType = inputModeType;
            _inputActionType = inputActionType;
            _updateMode = updateMode;
        }
        public override int GetHashCode()
        {
            return _inputModeType.GetHashCode() ^ _inputActionType.GetHashCode() ^ UpdateMode.GetHashCode();   
        }
        public override bool Equals(object obj)
        {
            if (obj is not InputType other) return false;
            if (this._inputModeType == other._inputModeType 
                && this._inputActionType == other._inputActionType 
                && this._updateMode == other._updateMode )
            {
                return true;
            }
            return false;
        }
    }
    public struct Inputter 
    {
        private InputType _inputType;
        private ExecuteType _executeType;
        public InputType InputType => _inputType;
        public ExecuteType ExecuteType => _executeType;

        public Inputter( InputType inputType , ExecuteType execute) 
        {
            _inputType = inputType;
            _executeType = execute;
        }
        public Inputter(InputModeType inputModeType, ActionType inputActionType, ExecuteType execute, UpdateMode updateMode)
        {
            _inputType = new InputType(inputModeType, inputActionType ,updateMode);
            _executeType = execute;
        }

        public override int GetHashCode()
        {
            return InputType.GetHashCode() ^ ExecuteType.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is not Inputter other) return false;
            if (this.InputType.Equals(other.InputType) && this.ExecuteType.HasFlag(other.ExecuteType))  
            {
                return true;
            }
            return false;
        }
    }
    
}
