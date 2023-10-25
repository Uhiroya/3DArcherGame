using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyInput;

public static partial class GA
{
    public static InputProvider Input = new();
}
/// <summary>InputSystem�̋@�\���T�|�[�g�o�^���ꂽ���ē��͂ɉ����ăR�[���o�b�N���s���N���X</summary>
public class InputProvider
{
    ///// <summary>/// �{�^�����̓R�[���o�b�N�o�^�p/// </summary>
    public void Regist(Inputter inputter, Action action)
    {
        if (IsNotRegisted(inputter) || !_inputContainers.ContainsKey(inputter))
        {
            _inputContainers.Add(inputter, null);
        }
        _inputContainers[inputter] += action;
    }
    /// <summary> CS0121�G���[���o��\��������̂ŁA�f���Q�[�g���L���X�g����K�v�����邩��</summary>
    public void Regist(Inputter inputter, Action<float> action)
    {
        if (IsNotRegisted(inputter) || !_inputValueContainers.ContainsKey(inputter))
        {
            _inputValueContainers.Add(inputter, null);
            _inputValueDic.Add(inputter, 0f);
        }
        _inputValueContainers[inputter] += action;
    }
    /// <summary> CS0121�G���[���o��\��������̂ŁA�f���Q�[�g���L���X�g����K�v�����邩��</summary>
    public void Regist(Inputter inputter, Action<Vector2> action)
    {
        if (IsNotRegisted(inputter) || !_inputVector2Containers.ContainsKey(inputter))
        {
            _inputVector2Containers.Add(inputter, null);
            _inputVector2Dic.Add(inputter, Vector2.zero);
        }
        _inputVector2Containers[inputter] += action;
    }
    public void Regist(List<(Inputter, Action)> inputterList)
    {
        foreach (var inputter in inputterList)
        {
            Regist(inputter.Item1, inputter.Item2);
        }
    }
    /// <summary>/// �{�^�����̓R�[���o�b�N�o�^�����p/// </summary>
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
    /// <summary>���̓��[�h�ύX/// </summary>
    public void ModeChange(InputMode inputMode)
    {
        _mode = inputMode;
        ResetDicCondition();
    }
    void ResetDicCondition()
    {
        _actionConditionContainer.Values.Select(x => x.ExecuteType = ExecuteType.Waiting);
        //�z�񉻂���Value��ς��镪�ɂ͗ǂ��炵��
        foreach (var item in _inputValueDic.Keys.ToList())
        {
            _inputValueDic[item] = 0f;
        }
        foreach (var item in _inputVector2Dic.Keys.ToList())
        {
            _inputVector2Dic[item] = Vector2.zero;
        }
    }
    bool IsNotRegisted(Inputter inputter)
    {
        if (!_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _actionConditionContainer.Add(inputter.InputType, new MyConditierValue(ExecuteType.Waiting, null));
            return true;
        }
        return false;
    }
    class MyInputUpdator : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        //_actionConditionContainer�����Ƃ��Ƃ����Ő錾����Ă������A�O�ɏo���������ŃC���f�N�T�[�Ƃ̊֌W���킩��ɂ����Ȃ����B
        //�C���f�N�T�[�ɖ��O���������������߂����ɂ���...
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
                //Enter�̎��̂݃R���[�`�����N��
                if (_actionConditionContainer[key].Coroutine == null && (value.HasFlag(ExecuteType.Enter) || value.HasFlag(ExecuteType.Waiting)))
                    _actionConditionContainer[key].Coroutine = StartCoroutine(InputCallBack(key));
            }
        }
        // Unity�̊eUpdate�t���[����҂����玟�̏�ԂɑJ�ڂ������B
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
            Inputter invoker = new Inputter(key, _actionConditionContainer[key].ExecuteType);
            _inputContainers.TryGetValue(invoker, out Action action);
            action?.Invoke();
            _inputValueContainers.TryGetValue(invoker, out Action<float> valueAction);
            valueAction?.Invoke(_inputValueDic[invoker]);
            _inputVector2Containers.TryGetValue(invoker, out Action<Vector2> vec2Action);
            vec2Action?.Invoke(_inputVector2Dic[invoker]);
            if(_actionConditionContainer[key].ExecuteType == ExecuteType.Waiting)
            {
                if(action == null && valueAction == null && vec2Action == null)
                {
                    StopCoroutine(_actionConditionContainer[key].Coroutine);
                    _actionConditionContainer[key].Coroutine = null;
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
    /// <summary>�@�o�^���ꂽ���͂̏�Ԃ�ێ�����R���e�i </summary>
    static Dictionary<InputType, MyConditierValue> _actionConditionContainer = new();
    /// <summary>�@�o�^����Ă��Ȃ����̂ɃA�N�Z�X�����None���A���Ă���</summary>
    public ExecuteType GetKeyCondition(InputType inputType)
    {
        return _mir[inputType];
    }
    /// <summary> ���݂̓��͒l��ێ����ĕԂ��p�̎��� </summary>
    static Dictionary<Inputter, float> _inputValueDic = new();
    public static Dictionary<Inputter, float> InputValueDic => _inputValueDic;
    static Dictionary<Inputter, Vector2> _inputVector2Dic = new();
    public static Dictionary<Inputter, Vector2> InputVector2Dic => _inputVector2Dic;
    /// <summary> ���̓R�[���o�b�N�o�^�p�R���e�i </summary>
    static Dictionary<Inputter, Action> _inputContainers = new();
    static Dictionary<Inputter, Action<float>> _inputValueContainers = new();
    static Dictionary<Inputter, Action<Vector2>> _inputVector2Containers = new();
    /// <summary> �R���X�g���N�^ </summary>
    public InputProvider() => Initialize();
    InputController _controller = default;
    public InputController Controller => _controller;
    static InputMode _mode = InputMode.InGame;
    public static InputMode Mode => _mode;
    /// <summary>/// ���������s��/// </summary>
    void Initialize()
    {
        var obj = new GameObject("PlayerInput");
        _mir = obj.AddComponent<MyInputUpdator>();
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
    //���p����Input�̏����o�^�������Ȃ��ƁAContainsKey��TryGetValue�������ɍ������ޕK�v���o�Ă��Ă��܂����߁A
    //�S�Ă�Key���ŏ�����o�^���Ă��������������y�����B�Ǝv�������A����̏�������Waiting���o�^�����ƁA
    //Update�����FixedUpdate�̃^�C�~���O�ōX�V���鏈�������Ă��邽�߁A�Ӗ��̂Ȃ��R���[�`���������N�������B
    //    void InitializeContainer()
    //    {
    //        foreach (InputMode inputMode in Enum.GetValues(typeof(InputMode)))
    //        {
    //            foreach (UpdateMode updateMode in Enum.GetValues(typeof(UpdateMode)))
    //            {
    //                foreach (ActionType actionType in Enum.GetValues(typeof(ActionType)))
    //                {
    //                    InputType inputType = new InputType(inputMode, actionType, updateMode);
    //                    _actionConditionContainer.Add(inputType , new MyConditierValue(ExecuteType.Waiting, null));
    //                    foreach (ExecuteType executeType in Enum.GetValues(typeof(ExecuteType)))
    //                    {
    //                        Inputter inputter = new Inputter(inputType ,executeType);
    //                        _inputContainers.Add(inputter, null);
    //                    }
    //                }
    //                foreach (ActionType actionType in new ActionType[] { ActionType.Scroll })
    //                {
    //                    InputType inputType = new InputType(inputMode, actionType, updateMode);
    //                    _inputVector2Containers.Add()
    //                }
    //                foreach (ActionType actionType in new ActionType[]{ ActionType.Move , ActionType.Look})
    //                {
    //                    InputType inputType = new InputType(inputMode, actionType, updateMode);
    //                }
    //        }           
    //    }
    /// <summary>�R���e�i�ɓo�^���ꂽ�R�[���o�b�N�̌Ăяo��/// </summary>
    void CheckContainer(ExecuteType executeType, ActionType actionType)
    {
        InputType inputType = new InputType(_mode, actionType, UpdateMode.Update);
        if (_actionConditionContainer.ContainsKey(inputType))
        {
            _mir[inputType] = executeType;
        }
        inputType = new InputType(_mode, actionType, UpdateMode.FixedUpdate);
        if (_actionConditionContainer.ContainsKey(inputType))
        {
            _mir[inputType] = executeType;
        }
    }
    void CheckContainer(ExecuteType executeType, ActionType actionType, float value)
    {
        Inputter inputter = new Inputter(_mode, actionType, executeType, UpdateMode.Update);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputValueDic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
        inputter = new Inputter(_mode, actionType, executeType, UpdateMode.FixedUpdate);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputValueDic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
    }
    void CheckContainer(ExecuteType executeType, ActionType actionType, Vector2 value)
    {
        Inputter inputter = new Inputter(_mode, actionType, executeType, UpdateMode.Update);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputVector2Dic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
        inputter = new Inputter(_mode, actionType, executeType, UpdateMode.FixedUpdate);
        if (_actionConditionContainer.ContainsKey(inputter.InputType))
        {
            _inputVector2Dic[inputter] = value;
            _mir[inputter.InputType] = executeType;
        }
    }
}

namespace MyInput
{
    /// <summary>�Q�[�����̃��[�h</summary>
    public enum InputMode
    {
        /// <summary> �Q�[�������샂�[�h </summary>
        InGame,
        /// <summary> ���j���[���t�h���샂�[�h </summary>
        Menu,
        /// <summary> ���̑��ꎞ���s���̑��샂�[�h </summary>
        Other,
    }
    /// <summary>���s�^�C�v</summary>
    [Flags]
    public enum ExecuteType
    {
        None = 0,
        /// <summary> ���͎� </summary>
        Enter = 1 << 0,
        /// <summary> ���͒� </summary>
        Performed = 1 << 1,
        /// <summary> ���͏I���� </summary>
        Exit = 1 << 2,
        /// <summary> ���͑҂� </summary>
        Waiting = 1 << 3,
        /// <summary> ��� </summary>
        Always = Enter | Performed | Exit  | Waiting,
    }
    //�񋓌^��Equals��==���I�[�o�[���C�h�ł��Ȃ����߁A�r�b�g�t���O�̓�������ɂ͈ȉ��𗘗p����B
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
    /// <summary>���̓A�N�V�����̎�� </summary>
    public enum ActionType
    {
        /// <summary> �W�����v���� </summary>
        Jump,
        /// <summary> �L�����Z������ </summary>
        Cancel,
        /// <summary> �U�����͂P </summary>
        Fire1,
        /// <summary> �Y�[���@�E�N�� </summary>
        Zoom,
        /// <summary> �J�����I�t�Z�b�g </summary>
        Scroll,
        /// <summary> �ړ����� </summary>
        Move,
        /// <summary> �J�������� </summary>
        Look,

    }
    public enum UpdateMode
    {
        Update,
        FixedUpdate,
    }
    //dictionary��Key�Ƃ��ė��p����ۂɃ{�b�N�X���������N�����Ȃ��悤�ɁAIEquatable���p������B
    /// <summary>���͎��</summary>
    /// <param name="mode"></param><param name="executeType"></param><param name="actionType"></param></summary>
    public struct InputType : IEquatable<InputType>
    {
        private InputMode _inputMode;
        private ActionType _actionType;
        private UpdateMode _updateMode;
        public InputMode InputMode => _inputMode;
        public ActionType ActionType => _actionType;
        
        public UpdateMode UpdateMode => _updateMode;
        public InputType(InputMode inputMode ,ActionType actionType ,UpdateMode updateMode)
        {
            _inputMode = inputMode;
            _actionType = actionType;
            _updateMode = updateMode;
        }
        //�񋓌^��GetHashCode��Ԃ����Aint�ɃL���X�g�����ق��������H�炵���B
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
    /// <summary> ���͓o�^�p</summary>
    public struct Inputter : IEquatable<Inputter>
    {
        private InputType _inputType;
        private ExecuteType _executeType;
        public InputType InputType => _inputType;
        public InputMode InputMode => InputType.InputMode;
        public ActionType ActionType => _inputType.ActionType;
        public UpdateMode UpdateMode => _inputType.UpdateMode;
        public ExecuteType ExecuteType => _executeType;

        public Inputter( InputType inputType , ExecuteType execute) 
        {
            _inputType = inputType;
            _executeType = execute;
        }
        public Inputter(InputMode inputMode, ActionType actionType, ExecuteType execute, UpdateMode updateMode)
        {
            _inputType = new InputType(inputMode, actionType ,updateMode);
            _executeType = execute;
        }
        public override int GetHashCode()
        {
            return InputType.GetHashCode() ;
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
    
}
