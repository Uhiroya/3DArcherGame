using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyInput;
public static class InputProvider 
{
    /// <summary> �ړ����̓R�[���o�b�N�o�^�p </summary>
    public static UnityEvent<Vector2> MoveCallback = new();
    /// <summary> �}�E�X���̓R�[���o�b�N�o�^�p </summary>
    public static UnityEvent<Vector2> MouseCallback = new();
    /// <summary>/// �{�^�����̓R�[���o�b�N�o�^�p/// </summary>
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
    /// <summary>/// �{�^�����̓R�[���o�b�N�o�^�����p/// </summary>
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
    /// <summary>���̓��[�h�ύX/// </summary>
    public static void ModeChange(InputModeType inputMode) => _mode = inputMode;

    /// <summary> ���̑����̓R�[���o�b�N�o�^�p�R���e�i </summary>
    static Dictionary<InputType, Action> _inGameInputContainer = new();
    static Dictionary<InputType, Action> _menuInputContainer = new();
    static Dictionary<InputType, Action> _otherInputContainer = new();

    /// <summary> �R���X�g���N�^ </summary>
    static InputProvider() => Initialize();

    static InputController _controller = default;
    public static InputController Controller => _controller;
    static InputModeType _mode = InputModeType.InGame;
    /// <summary>/// ���������s��/// </summary>
    static void Initialize()
    {
        _controller = new InputController();
        _controller.Enable();
        InitializeContainer();
        _controller.Player.Move.performed += context => { MoveCallback?.Invoke(context.ReadValue<Vector2>()); };
        //MoveCallback.AddListener((Vector2) => print(Vector2));
        _controller.Player.Move.canceled += context => MoveCallback?.Invoke(Vector2.zero);
        _controller.Player.Look.performed += context => MouseCallback?.Invoke(context.ReadValue<Vector2>());
        _controller.Player.Look.canceled += context => MouseCallback?.Invoke(Vector2.zero);
        _controller.Player.Jump.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Jump);
        _controller.Player.Jump.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Jump);
        _controller.Player.Fire1.performed += context => CheckContainer(_mode, ExecuteType.Enter, InputActionType.Fire1);
        _controller.Player.Fire1.canceled += context => CheckContainer(_mode, ExecuteType.Exit, InputActionType.Fire1);
    }
    /// <summary>�R���e�i�̏�����/// </summary>
    static void InitializeContainer()
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
    /// <summary>�R���e�i�ɓo�^���ꂽ�R�[���o�b�N�̌Ăяo��/// </summary>
    static void CheckContainer(InputModeType mode ,ExecuteType excuteType , InputActionType inputActionType)
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
    }
    /// <summary>���̓A�N�V�����̎�� </summary>
    public enum InputActionType
    {
        /// <summary> ������� </summary>
        Submit,
        /// <summary> �L�����Z������ </summary>
        Cancel,
        /// <summary> �W�����v���� </summary>
        Jump,
        /// <summary> ���[�h�`�F���W���� </summary>
        ChangeMode,
        /// <summary> �^�[�Q�b�g�`�F���W���� </summary>
        ChangeTarget,
        /// <summary> �U�����͂P </summary>
        Fire1,
        /// <summary> �U�����͂Q </summary>
        Fire2,
        /// <summary> �U�����͂R </summary>
        Fire3,
        /// <summary> �U�����͂S </summary>
        Fire4,
        /// <summary> �u�[�X�^�[���� </summary>
        Booster,
    }
    /// <summary>���͎��</summary>
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
        public Inputter(InputModeType mode, InputActionType inputActionType, ExecuteType excuteType, Action action)
        {
            _mode = mode;
            _inputType = new InputType(excuteType, inputActionType);
            _action = action;
        }
    }
}
