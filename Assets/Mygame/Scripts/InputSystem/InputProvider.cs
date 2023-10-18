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
    private static Vector2 _mouseDir = Vector2.zero;
    ///// <summary> �ړ����̓R�[���o�b�N�o�^�p </summary>
    public UnityEvent<Vector2> MoveCallback = new();
    public UnityEvent<Vector2> MoveFixedCallback = new();
    /// <summary> �}�E�X���̓R�[���o�b�N�o�^�p </summary>
    public UnityEvent<Vector2> LookCallback = new();
    public UnityEvent<Vector2> MouseFixedCallback = new();
    /// <summary>/// �{�^�����̓R�[���o�b�N�o�^�p/// </summary>
    public void Regist(Inputter inputter)
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
    public void Regist(List<Inputter> inputterList)
    {
        foreach(var inputter  in inputterList)
        {
            Regist(inputter);
        }
    }
    /// <summary>/// �{�^�����̓R�[���o�b�N�o�^�����p/// </summary>
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
    public void UnRegist(List<Inputter> inputterList)
    {
        foreach (var inputter in inputterList)
        {
            UnRegist(inputter);
        }
    }
    /// <summary>���̓��[�h�ύX/// </summary>
    public void ModeChange(InputModeType inputMode) => _mode = inputMode;
    
    private class MyInputUpdator : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        /// <summary> ���̑����̓R�[���o�b�N�o�^�p�R���e�i </summary>
        public Dictionary<ActionUpdateMode, ExecuteType> ActionConditionContainer = new();
        public ExecuteType this[ActionUpdateMode key]
        {
            get 
            { 
                return ActionConditionContainer[key]; 
            }
            set
            {
                ActionConditionContainer[key] = value;
                if(value == ExecuteType.Enter)
                    StartCoroutine(InputCallBack(key));
            }
        }
        public ExecuteType this[(ActionUpdateMode,int) key]
        {
            get
            {
                return ActionConditionContainer[key.Item1];
            }
            set
            {
                ActionConditionContainer[key.Item1] = value;
                if (value == ExecuteType.Enter)
                    StartCoroutine(InputCallBack(key.Item1));
            }
        }
        void Update()
        {
            if(GA.Input._mode == InputModeType.InGame)
            {
                if (GA.Input.MoveCallback != null)
                    GA.Input.MoveCallback?.Invoke(_moveDir);
                if (GA.Input.MoveCallback != null)
                    GA.Input.LookCallback?.Invoke(_mouseDir);
            }
        }
        void FixedUpdate()
        {
            if (GA.Input._mode == InputModeType.InGame)
            {
                if (GA.Input.MoveFixedCallback != null)
                    GA.Input.MoveFixedCallback?.Invoke(_moveDir);
                if (GA.Input.MoveFixedCallback != null)
                    GA.Input.MouseFixedCallback?.Invoke(_mouseDir);
            }
        }
        // Unity�̊eUpdate�t���[����҂����玟�̏�ԂɑJ�ڂ������B

        IEnumerator InputCallBack(ActionUpdateMode key)
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
        void InvokeCallBack(ExecuteType executeType, InputActionType inputActionType, UpdateMode updateMode)
        {
            var inputType = new InputType(executeType, inputActionType, updateMode);
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
    }
    static MyInputUpdator _mr;
    // �����ς̃f���Q�[�g��錾
    //public delegate void VariableAction(params object[] args);
    //// �錾�����f���Q�[�g�����C���X�^���X�ϐ�
    //VariableAction action1 = null;
    //void a()
    //{
    //    action1.Invoke(3,"aaa",5);
    //}
//�e�R���e�i���e�p
    Dictionary<InputType, Action> _inGameInputContainer = new();
    Dictionary<InputType, Action> _menuInputContainer = new();
    Dictionary<InputType, Action> _otherInputContainer = new();
    //Dictionary<InputType, Action<float>> _inGameInputAxisContainer = new();
    //Dictionary<InputType, Action<float>> _menuInputAxisContainer = new();
    //Dictionary<InputType, Action<float>> _otherInputAxisContainer = new();
    Dictionary<ActionUpdateMode, ExecuteType> @ActionConditionContainer => _mr.ActionConditionContainer;
    /// <summary> �R���X�g���N�^ </summary>
    public InputProvider() => Initialize();
     InputController _controller = default;
    public InputController Controller => _controller;
     InputModeType _mode = InputModeType.InGame;
    /// <summary>/// ���������s��/// </summary>
    void Initialize()
    {
        var obj = new GameObject("PlayerInput");
        _mr = obj.AddComponent<MyInputUpdator>();
        _controller = new InputController();
        _controller.Enable();
        InitializeContainer();
        _controller.InputMap.Move.performed += context => { _moveDir = context.ReadValue<Vector2>(); };
        _controller.InputMap.Move.canceled += context => { _moveDir = Vector2.zero; };
        _controller.InputMap.Look.performed += context => { _mouseDir = context.ReadValue<Vector2>();};
        _controller.InputMap.Look.canceled += context => { _mouseDir = Vector2.zero;  };
        _controller.InputMap.Jump.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Jump);
        _controller.InputMap.Jump.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Jump);
        _controller.InputMap.Fire1.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Fire1);
        _controller.InputMap.Fire1.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Fire1);
        _controller.InputMap.Zoom.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Zoom);
        _controller.InputMap.Zoom.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Zoom);
        _controller.InputMap.Cancel.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Cancel);
        _controller.InputMap.Cancel.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Cancel);
        _controller.InputMap.Scroll.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Scroll);
        _controller.InputMap.Scroll.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Cancel);
    }
    /// <summary>�R���e�i�̏�����/// </summary>
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
    /// <summary>�R���e�i�ɓo�^���ꂽ�R�[���o�b�N�̌Ăяo��/// </summary>
    void CheckContainer(InputModeType mode, ExecuteType excuteType, InputActionType inputActionType)
    {
        _mr[new ActionUpdateMode(inputActionType, UpdateMode.Update)] = excuteType;
        _mr[new ActionUpdateMode(inputActionType, UpdateMode.FixedUpdate)] = excuteType;
    }
}


namespace MyInput
{
    /// <summary>�Q�[�����̃��[�h</summary>
    public enum InputModeType
    {
        /// <summary> �Q�[�������샂�[�h </summary>
        InGame,
        /// <summary> ���j���[���t�h���샂�[�h </summary>
        Menu,
        /// <summary> ���̑��ꎞ���s���̑��샂�[�h </summary>
        Other,
    }
    /// <summary>���s�^�C�v</summary>
    public enum ExecuteType
    {
        /// <summary> ���͎� </summary>
        Enter,
        /// <summary> ���͒� </summary>
        Performed,
        /// <summary> ���͏I���� </summary>
        Exit,
        /// <summary> ���͑҂� </summary>
        Waiting,
    }
    /// <summary>���̓A�N�V�����̎�� </summary>
    public enum InputActionType
    {
        ///// <summary> ������� </summary>
        //Submit,
        /// <summary> �L�����Z������ </summary>
        Cancel,
        /// <summary> �W�����v���� </summary>
        Jump,
        ///// <summary> ���[�h�`�F���W���� </summary>
        //ChangeMode,
        ///// <summary> �^�[�Q�b�g�`�F���W���� </summary>
        //ChangeTarget,
        /// <summary> �U�����͂P </summary>
        Fire1,
        /// <summary> �Y�[���@�E�N�� </summary>
        Zoom,
        /// <summary> �J�����I�t�Z�b�g </summary>
        Scroll,
        ///// <summary> �U�����͂Q </summary>
        //Fire2,
        ///// <summary> �U�����͂R </summary>
        //Fire3,
        ///// <summary> �U�����͂S </summary>
        //Fire4,
        ///// <summary> �u�[�X�^�[���� </summary>
        //Booster,
    }
    public enum UpdateMode
    {
        Update,
        FixedUpdate,
    }
    /// <summary>���͎��</summary>
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
    /// <summary>���̓A�N�V�������ʗp�N���X</summary>
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
