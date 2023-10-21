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
    /////// <summary> �ړ����̓R�[���o�b�N�o�^�p </summary>
    //public Action<Vector2> MoveCallback = null;
    //public Action<Vector2> MoveFixedCallback = null;
    ///// <summary> �n�_���̓R�[���o�b�N�o�^�p </summary>
    //public Action<Vector2> LookCallback = null;
    //public Action<Vector2> LookFixedCallback = null;
    //public Action<float> LookOffsetCallback = null;
    //public Action<float> LookOffsetFixedCallback = null;
    public bool IsNotRegisted(InputType inputType)
    {
        if (!_actionConditionContainer.ContainsKey(inputType))
        {
            _actionConditionContainer.Add(inputType , 
                inputType.ExecuteType == ExecuteType.Always ? ExecuteType.Always :ExecuteType.Waiting );
            return true;
        }
        return false;
    }

    ///// <summary>/// �{�^�����̓R�[���o�b�N�o�^�p/// </summary>
    public void Regist(InputType inputType , Action action)
    {
        if (IsNotRegisted(inputType))
        {
            _inputContainers.Add(inputType, null);
        }
        _inputContainers[inputType] += action;
    }
    public void Regist(InputType inputType, Action<float> action)
    {
        if (IsNotRegisted(inputType))
        {
            _inputValueContainers.Add(inputType, null);
        }
        _inputValueContainers[inputType] += action;
    }
    public void Regist(InputType inputType, Action<Vector2> action)
    {
        if (IsNotRegisted(inputType))
        {
            _inputVector2Containers.Add(inputType, null);
        }
        _inputVector2Containers[inputType] += action;
    }
    public void Regist(List<(InputType , Action)> inputterList)
    {
        foreach(var inputter  in inputterList)
        {
            Regist(inputter.Item1 , inputter.Item2);
        }
    }
    /// <summary>/// �{�^�����̓R�[���o�b�N�o�^�����p/// </summary>
    public void UnRegist(InputType inputType, Action action)
    {
        if (_inputContainers.ContainsKey(inputType))
        {
            _inputContainers[inputType] -= action;
        } 
    }
    public void UnRegist(InputType inputType, Action<float> action)
    {
        if (_inputValueContainers.ContainsKey(inputType))
        {
            _inputValueContainers[inputType] -= action;
        }
    }
    public void UnRegist(InputType inputType, Action<Vector2> action)
    {
        if (_inputVector2Containers.ContainsKey(inputType))
        {
            _inputVector2Containers[inputType] -= action;
        }
    }
    public void UnRegist(List<(InputType, Action)> inputterList)
    {
        foreach (var inputter in inputterList)
        {
            UnRegist(inputter.Item1, inputter.Item2);
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

        public ExecuteType this[InputType key]
        {
            get
            {
                return _actionConditionContainer[key];
            }
            set
            {
                //Always�̏���̂݋N��
                if (key.ExecuteType == ExecuteType.Always && _actionConditionContainer[key] == ExecuteType.Waiting)
                    StartCoroutine(InputCallBack(key));
                //Exit�̎��͊����̃R���[�`���𔲂���悤��
                if (value == ExecuteType.Exit)
                {
                    _actionConditionContainer[key] = value;
                    
                }
                   
                //Enter�̎��̂݃R���[�`�����N��
                if (value == ExecuteType.Enter && _actionConditionContainer[key] != ExecuteType.Enter)
                {
                    _actionConditionContainer[key] = value;
                    StartCoroutine(InputCallBack(key));
                }   
                    
                
            }
        }
        // Unity�̊eUpdate�t���[����҂����玟�̏�ԂɑJ�ڂ������B
        IEnumerator InputCallBack(InputType key)
        {
            while (true)
            {
                yield return WaitUpdateFrame(key);
                
                switch (_actionConditionContainer[key])
                {
                    case ExecuteType.Enter:    
                        _actionConditionContainer[key] = ExecuteType.Performed;
                        break;
                    case ExecuteType.Performed:
                        break;
                    case ExecuteType.Exit:
                        _actionConditionContainer[key] = ExecuteType.Waiting;
                        yield break;
                    case ExecuteType.Always:
                        _actionConditionContainer[key] = ExecuteType.Always;
                        //�Q�[�����[�h���ς�铙�Q�Ɛ悪�Ȃ��Ȃ����珜�O����
                        if (!_inputContainers.ContainsKey(key) && !_inputValueContainers.ContainsKey(key) && !_inputVector2Containers.ContainsKey(key))
                            yield break;
                        else
                        {
                            //_actionConditionContainer[key] = ExecuteType.Waiting;
                            
                        }
                        break;
                    case ExecuteType.Waiting:
                        yield break;
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
                    InvokeCallBack(key);
                    yield return new WaitForEndOfFrame();
                    break;
                case UpdateMode.FixedUpdate:
                    InvokeCallBack(key);
                    yield return new WaitForFixedUpdate();
                    break;
                default:
                    break;
            }
        }
        void InvokeCallBack(InputType key)
        {
            InputType inputType =  new(key.InputModeType , key.InputActionType , _actionConditionContainer[key] , key.UpdateMode);
            if (_inputContainers.ContainsKey(inputType))
                _inputContainers[inputType]?.Invoke();
            if (_inputValueContainers.ContainsKey(inputType))
                _inputValueContainers[inputType]?.Invoke(_inputValueDic[inputType]);
            if (_inputVector2Containers.ContainsKey(inputType))
                _inputVector2Containers[inputType]?.Invoke(_inputVector2Dic[inputType]);
        }
    }
    static MyInputUpdator _mir;
    static Dictionary<InputType, ExecuteType> _actionConditionContainer = new();
    private static Dictionary<InputType, float> _inputValueDic;
    private static Dictionary<InputType, Vector2> _inputVector2Dic;
    static Dictionary<InputType, Action> _inputContainers = new();
    static Dictionary<InputType, Action<float>> _inputValueContainers = new();
    static Dictionary<InputType, Action<Vector2>> _inputVector2Containers = new();
    Dictionary<InputType, ExecuteType> ActionConditionContainer => _actionConditionContainer;
    
    public bool GetKeyDown(InputType inputType)
    => ActionConditionContainer[inputType] == ExecuteType.Enter;
    public bool GetKey(InputType inputType)
        => ActionConditionContainer[inputType] == ExecuteType.Performed;
    public bool GetKeyUp(InputType inputType)
        => ActionConditionContainer[inputType] == ExecuteType.Exit;
    /// <summary> �R���X�g���N�^ </summary>
    public InputProvider() => Initialize();
     InputController _controller = default;
    public InputController Controller => _controller;
    static InputModeType _mode = InputModeType.InGame;
    public static InputModeType Mode => _mode;
    /// <summary>/// ���������s��/// </summary>
    void Initialize()
    {
        var obj = new GameObject("PlayerInput");
        _mir = obj.AddComponent<MyInputUpdator>();
        _controller = new InputController();
        _controller.Enable();
        //InitializeContainer();
        _controller.InputMap.Move.performed += context => CheckContainer(ExecuteType.Always, InputActionType.Move ,context.ReadValue<Vector2>());
        _controller.InputMap.Move.canceled += context => CheckContainer(ExecuteType.Always, InputActionType.Move, Vector2.zero);
        _controller.InputMap.Look.performed += context => CheckContainer(ExecuteType.Always, InputActionType.Look, context.ReadValue<Vector2>());
        _controller.InputMap.Look.canceled += context => CheckContainer(ExecuteType.Always, InputActionType.Look, Vector2.zero);
        _controller.InputMap.Scroll.performed += context => CheckContainer(ExecuteType.Always, InputActionType.Scroll, context.ReadValue<float>());
        _controller.InputMap.Scroll.canceled += context => CheckContainer(ExecuteType.Always, InputActionType.Scroll, 0f);
        _controller.InputMap.Jump.performed += context => CheckContainer(ExecuteType.Enter, InputActionType.Jump);
        _controller.InputMap.Jump.canceled += context => CheckContainer(ExecuteType.Exit, InputActionType.Jump);
        _controller.InputMap.Fire1.performed += context => CheckContainer(ExecuteType.Enter, InputActionType.Fire1);
        _controller.InputMap.Fire1.canceled += context => CheckContainer(ExecuteType.Exit, InputActionType.Fire1);
        _controller.InputMap.Zoom.performed += context => CheckContainer(ExecuteType.Enter, InputActionType.Zoom);
        _controller.InputMap.Zoom.canceled += context => CheckContainer(ExecuteType.Exit, InputActionType.Zoom);
        _controller.InputMap.Cancel.performed += context => CheckContainer(ExecuteType.Enter, InputActionType.Cancel);
        _controller.InputMap.Cancel.canceled += context => CheckContainer(ExecuteType.Exit, InputActionType.Cancel);
    }
    /// <summary>�R���e�i�̏�����/// </summary>
    //void InitializeContainer()
    //{
    //    foreach (InputModeType inputModeType in Enum.GetValues(typeof(InputModeType)))
    //    {
    //        _inputContainers.Add(inputModeType, new Dictionary<InputType, Action>());

    //    }
    //        foreach (MyInput.InputActionType inputActionType in Enum.GetValues(typeof(MyInput.InputActionType)))
    //    {
    //        foreach (UpdateMode updateMode in Enum.GetValues(typeof(UpdateMode)))
    //        {
    //            ActionConditionContainer.Add(new ActionUpdateMode(inputActionType, updateMode), ExecuteType.Waiting);
    //        }
    //    }
    //}
    /// <summary>�R���e�i�ɓo�^���ꂽ�R�[���o�b�N�̌Ăяo��/// </summary>
    void CheckContainer( ExecuteType executeType , InputActionType inputActionType)
    {
        InputType inputType = new InputType(_mode, inputActionType, executeType, UpdateMode.Update);
        if (_actionConditionContainer.ContainsKey(inputType))
        {
            _mir[inputType] = executeType;
        }
        inputType = new InputType(_mode, inputActionType, executeType, UpdateMode.FixedUpdate);
        if (_actionConditionContainer.ContainsKey(inputType))
        {
            _mir[inputType] = executeType;
        }
    }
    void CheckContainer(ExecuteType executeType, InputActionType inputActionType, float value)
    {
    //    InputType inputType = new InputType(_mode, inputActionType, executeType, UpdateMode.Update);
    //    if (_actionConditionContainer.ContainsKey(inputType))
    //    {
    //        _inputValueDic[inputType] = value;
    //        _mir[inputType] = executeType;
    //    }
    //    inputType = new InputType(_mode, inputActionType, executeType, UpdateMode.FixedUpdate);
    //    if (_actionConditionContainer.ContainsKey(inputType))
    //    {
    //        _inputValueDic[inputType] = value;
    //        _mir[inputType] = executeType;
    //    }
    }
    void CheckContainer(ExecuteType executeType, InputActionType inputActionType, Vector2 value)
    {
        //InputType inputType = new InputType(_mode, inputActionType, executeType, UpdateMode.Update);
        //if (_actionConditionContainer.ContainsKey(inputType))
        //{
        //    _inputVector2Dic[inputType] = value;
        //    _mir[inputType] = executeType;
        //}
        //inputType = new InputType(_mode, inputActionType, executeType, UpdateMode.FixedUpdate);
        //if (_actionConditionContainer.ContainsKey(inputType))
        //{
        //    _inputVector2Dic[inputType] = value;
        //    _mir[inputType] = executeType;
        //}
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
        /// <summary> ��� </summary>
        Always
    }
    /// <summary>���̓A�N�V�����̎�� </summary>
    public enum InputActionType
    {
        ///// <summary> ������� </summary>
        //Submit,
        /// <summary> �W�����v���� </summary>
        Jump,
        /// <summary> �L�����Z������ </summary>
        Cancel,

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
        Move,
        Look,

    }
    public enum UpdateMode
    {
        Update,
        FixedUpdate,
    }
    /// <summary>���͎��</summary>
    /// <param name="mode"></param><param name="executeType"></param><param name="inputActionType"></param></summary>
    public struct InputType
    {
        private InputModeType _inputModeType;
        private InputActionType _inputActionType;
        private ExecuteType _excuteType;
        private UpdateMode _updateMode;
        public InputModeType InputModeType => _inputModeType;
        public InputActionType InputActionType => _inputActionType;
        public ExecuteType ExecuteType => _excuteType;
        public UpdateMode UpdateMode => _updateMode;
        public InputType(InputModeType inputModeType , InputActionType inputActionType ,ExecuteType executeType ,UpdateMode updateMode)
        {
            _inputModeType = inputModeType;
            _inputActionType = inputActionType;
            _excuteType = executeType;
            _updateMode = updateMode;
        } 
    }

}
