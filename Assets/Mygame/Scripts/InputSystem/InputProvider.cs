using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyInput;
//GlobalAccessbleClass
public static partial class GA
{
    public static InputProvider Input = new();
}
/// <summary>InputSystemの機能をサポート登録されたして入力に応じてコールバックを行うクラス</summary>
public class InputProvider
{
    ///// <summary>/// ボタン入力コールバック登録用/// </summary>
    public InputToken Regist(InputType inputType ,ExecuteType execute, Action action)
    {
        Inputter inputter = new Inputter(inputType, execute);
        if (!_inputContainers.ContainsKey(inputter))
        {
            _inputContainers.Add(inputter, null);
        }
        _inputContainers[inputter] += action;
        InputToken inputToken = new InputToken(inputter , action);
        return inputToken;
    }
    /// <summary> CS0121エラーが出る可能性があるので、呼び出し時にデリゲートをキャストする必要がある</summary>
    public InputToken Regist(InputType inputType, ExecuteType execute, Action<float> action)
    {
        Inputter inputter = new Inputter(inputType, execute);
        if (!_inputValueContainers.ContainsKey(inputter))
        {
            _inputValueContainers.Add(inputter, null);
        }
        _inputValueContainers[inputter] += action;
        InputToken inputToken = new InputToken(inputter, action);
        return inputToken;
    }
    /// <summary> CS0121エラーが出る可能性があるので、呼び出し時にデリゲートをキャストする必要がある</summary>
    public InputToken Regist(InputType inputType, ExecuteType execute, Action<Vector2> action)
    {
        Inputter inputter = new Inputter(inputType, execute);
        if (!_inputVector2Containers.ContainsKey(inputter))
        {
            _inputVector2Containers.Add(inputter, null);
        }
        _inputVector2Containers[inputter] += action;
        InputToken inputToken = new InputToken(inputter, action);
        return inputToken;
    }
    /// <summary>/// ボタン入力コールバック登録解除用/// </summary>
    public void UnRegist(InputToken token)
    {
        if(token.Action is Action action)
        {
            _inputContainers.TryGetValue(token.Inputter, out Action delegete);
            if (delegete != null) return;
            delegete -= action;
        }
        else if (token.Action is Action<float> valueAction)
        {
            _inputValueContainers.TryGetValue(token.Inputter, out Action<float> delegete);
            if (delegete != null) return;
            delegete -= valueAction;
        }
        else if (token.Action is Action<Vector2> vec2Action)
        {
            _inputVector2Containers.TryGetValue(token.Inputter, out Action<Vector2> delegete);
            if (delegete != null) return;
            delegete -= vec2Action;
        }
    }
    /// <summary>入力モード変更/// </summary>
    public void ModeChange(InputMode inputMode)
    {
        _mode = inputMode;
        ResetDicCondition();
    }
    /// <summary>操作の入力状況を返す,※Updateのタイミングを合わせて利用する</summary>
    public ExecuteType GetKeyCondition(ActionType actionType, UpdateMode updateMode)
    {
        InputType inputType = new InputType(_mode, actionType, updateMode);
        return _mir[inputType];
    }
    /// <summary>入力状態をリセットする。</summary>
    void ResetDicCondition()
    {
        _actionConditionContainer.Values.Select(x => x.ExecuteType = ExecuteType.Waiting);
        //配列化してKeyを受け取る場合は例外にならない
        foreach (var item in _inputValueDic.Keys.ToList())
        {
            _inputValueDic[item] = 0f;
        }
        foreach (var item in _inputVector2Dic.Keys.ToList())
        {
            _inputVector2Dic[item] = Vector2.zero;
        }
    }

    class MyInputUpdator : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        //_actionConditionContainerがもともとここで宣言されていたが、外に出したせいでインデクサーとの関係がわかりにくくなった。
        public ExecuteType this[InputType key]
        {
            get
            {
                _actionConditionContainer.TryGetValue(key, out var condition);
                return condition != null ?  condition.ExecuteType : ExecuteType.None ;
            }
            set
            {
                _actionConditionContainer[key].ExecuteType = value;
                //Enterの時または初期化時のみコルーチンを起動
                if (_actionConditionContainer[key].Coroutine == null && (value.HasFlag(ExecuteType.Enter) || value.HasFlag(ExecuteType.Waiting)))
                    _actionConditionContainer[key].Coroutine = StartCoroutine(InputCallBack(key));
            }
        }
        // Unityの各Updateフレームを待ったら次の状態に遷移する。
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
                    yield return null;
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
            //print($"{key.ActionType}のコルーチンが呼ばれました");
            Inputter invoker = new Inputter(key, _actionConditionContainer[key].ExecuteType);
            //登録されていないならdefault値が帰ってくる
            _inputContainers.TryGetValue(invoker, out Action action);
            _inputValueContainers.TryGetValue(invoker, out Action<float> valueAction);
            _inputVector2Containers.TryGetValue(invoker, out Action<Vector2> vec2Action);

            action?.Invoke();
            valueAction?.Invoke(_inputValueDic[key]);
            vec2Action?.Invoke(_inputVector2Dic[key]);
            //Actionが登録されていないかつキー入力がないときはコルーチンを終了させる。
            if(ExecuteTypeExtensions.CustomEquals( _actionConditionContainer[key].ExecuteType, ExecuteType.Waiting))
            {
                if(action == null && valueAction == null && vec2Action == null)
                {
                    StopCoroutine(_actionConditionContainer[key].Coroutine);
                    _actionConditionContainer[key].Coroutine = null;
                    //print($"{key.ActionType}のコルーチンが終了しました");
                }
            }
        }
    }
    static MyInputUpdator _mir;
    public class MyConditierValue
    {
        public ExecuteType ExecuteType;
        public Coroutine Coroutine;
        public MyConditierValue(ExecuteType executeType, Coroutine coroutine)
        {
            ExecuteType = executeType;
            Coroutine = coroutine;
        }
    }
    /// <summary>　登録された入力の状態を保持するコンテナ </summary>
    static Dictionary<InputType, MyConditierValue> _actionConditionContainer = new();

    /// <summary> 現在の入力値を保持して返す用の辞書 </summary>
    static Dictionary<InputType, float> _inputValueDic = new();
    public static Dictionary<InputType, float> InputValueDic => _inputValueDic;
    static Dictionary<InputType, Vector2> _inputVector2Dic = new();
    public static Dictionary<InputType, Vector2> InputVector2Dic => _inputVector2Dic;
    /// <summary> 入力コールバック登録用コンテナ </summary>
    static Dictionary<Inputter, Action> _inputContainers = new();
    static Dictionary<Inputter, Action<float>> _inputValueContainers = new();
    static Dictionary<Inputter, Action<Vector2>> _inputVector2Containers = new();
    /// <summary> コンストラクタ </summary>
    public InputProvider() => Initialize();
    InputController _controller = default;
    public InputController Controller => _controller;
    static InputMode _mode = InputMode.InGame;
    public static InputMode Mode => _mode;
    /// <summary>/// 初期化を行う/// </summary>
    void Initialize()
    {
        var obj = new GameObject("PlayerInput");
        _mir = obj.AddComponent<MyInputUpdator>();
        InitializeContainer();
        _controller = new InputController();
        _controller.Enable();
        _controller.InputMap.Move.performed += context => CheckContainer(ExecuteType.Enter, ActionType.Move, context.ReadValue<Vector2>());
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
    //ConditionContainerの初期登録
    void InitializeContainer()
    {
        foreach (InputMode inputMode in Enum.GetValues(typeof(InputMode)))
        {
            foreach (ActionType actionType in Enum.GetValues(typeof(ActionType)))
            {
                foreach (UpdateMode updateMode in Enum.GetValues(typeof(UpdateMode)))
                {
                    InputType inputType = new InputType(inputMode, actionType, updateMode);
                    _actionConditionContainer.Add(inputType, new MyConditierValue(ExecuteType.Waiting, null));
                    _inputValueDic[inputType] = 0f;
                    _inputVector2Dic[inputType] = Vector2.zero;
                }
            }
        }
    }
    /// <summary>コンテナに登録されたコールバックの呼び出し/// </summary>
    void CheckContainer(ExecuteType executeType, ActionType actionType)
    {
        foreach (UpdateMode updateMode in Enum.GetValues(typeof(UpdateMode)))
        {
            InputType inputType = new InputType(_mode, actionType, updateMode);
            _mir[inputType] = executeType;
        }
    }
    void CheckContainer(ExecuteType executeType, ActionType actionType, float value)
    {
        foreach (UpdateMode updateMode in Enum.GetValues(typeof(UpdateMode)))
        {
            InputType inputType = new InputType(_mode, actionType, updateMode);
            _inputValueDic[inputType] = value;
            _mir[inputType] = executeType;
        }
    }
    void CheckContainer(ExecuteType executeType, ActionType actionType, Vector2 value)
    {
        foreach (UpdateMode updateMode in Enum.GetValues(typeof(UpdateMode)))
        {
            InputType inputType = new InputType(_mode, actionType, updateMode);
            _inputVector2Dic[inputType] = value;
            _mir[inputType] = executeType;
        }
    }
}

namespace MyInput
{
    /// <summary>ゲーム中のモード</summary>
    public enum InputMode
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
        None = 0,
        /// <summary> 入力時 </summary>
        Enter = 1 << 0,
        /// <summary> 入力中 </summary>
        Performed = 1 << 1,
        /// <summary> 入力終了時 </summary>
        Exit = 1 << 2,
        /// <summary> 入力待ち </summary>
        Waiting = 1 << 3,
        /// <summary> 常に </summary>
        Always = Enter | Performed | Exit | Waiting,
    }
    //列挙型のEqualsと==がオーバーライドできないため、ビットフラグの等価判定には以下を利用する。
    public static class ExecuteTypeExtensions
    {
        public static bool CustomEquals(this ExecuteType key1, ExecuteType key2)
        {
            return (key1 & key2) != 0;
        }
        public static int CustomGetHashCode(this ExecuteType value)
        {
            return 0;
        }
    }
    /// <summary>入力アクションの種類 </summary>
    public enum ActionType
    {
        /// <summary> ジャンプ入力 </summary>
        Jump,
        /// <summary> キャンセル入力 </summary>
        Cancel,
        /// <summary> 攻撃入力１ </summary>
        Fire1,
        /// <summary> ズーム　右クリ </summary>
        Zoom,
        /// <summary> カメラオフセット </summary>
        Scroll,
        /// <summary> 移動入力 </summary>
        Move,
        /// <summary> カメラ入力 </summary>
        Look,

    }
    public enum UpdateMode
    {
        Update,
        FixedUpdate,
    }
    //dictionaryのKeyとして利用する際にボックス化を引き起こさないように、IEquatableを継承する。
    /// <summary>入力種別</summary>
    /// <param name="mode"></param><param name="executeType"></param><param name="actionType"></param></summary>
    public struct InputType : IEquatable<InputType>
    {
        private InputMode _inputMode;
        private ActionType _actionType;
        private UpdateMode _updateMode;
        public InputMode InputMode => _inputMode;
        public ActionType ActionType => _actionType;

        public UpdateMode UpdateMode => _updateMode;
        public InputType(InputMode inputMode, ActionType actionType, UpdateMode updateMode)
        {
            _inputMode = inputMode;
            _actionType = actionType;
            _updateMode = updateMode;
        }
        //列挙型のGetHashCodeを返すより、intにキャストしたほうが早い？らしい。
        public override int GetHashCode()
        {
            return (int)_inputMode ^ (int)_actionType << 10 ^ (int)UpdateMode << 20;
        }
        public bool Equals(InputType other)
        {
            if (this._inputMode == other._inputMode
                && this._actionType == other._actionType
                && this._updateMode == other._updateMode)
            {
                return true;
            }
            return false;
        }
    }
    /// <summary> 入力登録用</summary>
    public struct Inputter : IEquatable<Inputter>
    {
        private InputType _inputType;
        private ExecuteType _executeType;
        public InputType InputType => _inputType;
        public InputMode InputMode => InputType.InputMode;
        public ActionType ActionType => _inputType.ActionType;
        public UpdateMode UpdateMode => _inputType.UpdateMode;
        public ExecuteType ExecuteType => _executeType;

        public Inputter(InputType inputType, ExecuteType execute)
        {
            _inputType = inputType;
            _executeType = execute;
        }
        public Inputter(InputMode inputMode, ActionType actionType, UpdateMode updateMode, ExecuteType execute)
        {
            _inputType = new InputType(inputMode, actionType, updateMode);
            _executeType = execute;
        }
        public override int GetHashCode()
        {
            return InputType.GetHashCode();
        }

        public bool Equals(Inputter other)
        {
            if (this.InputType.Equals(other.InputType) && this.ExecuteType.CustomEquals(other.ExecuteType))
            {
                return true;
            }
            return false;
        }
    }
    public class InputToken
    {
        private Inputter _inputter;
        private Delegate _action;
        public Inputter Inputter => _inputter;
        public Delegate Action => _action;
        public InputToken(Inputter inputter , Delegate action)
        {
            _inputter = inputter;
            _action = action;
        }
    }
}
