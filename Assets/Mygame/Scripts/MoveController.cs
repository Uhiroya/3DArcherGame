using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Animations;
using DG.Tweening;
//using UnityEngine.InputSystem;
using MyInput;


// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
//https://discussions.unity.com/t/how-to-control-aixs-gravity-sensitivity-in-new-input-system/231461/5
public class MoveController : MonoBehaviour
{
    [SerializeField] float _forwardSpeed = 7.0f;
    [SerializeField] float _backwardSpeed = 2.0f;
    [SerializeField] float _sidewardSpeed = 4.0f;
    [SerializeField] float _rotateSpeed = 2.0f;
    [SerializeField] float _jumpPower = 3.0f;
    [SerializeField] float minInputValue;
    [SerializeField] float inputGravity;
    public AnimatorStateInfo CurrentBaseState;
    public AnimatorStateInfo ArrowState;
    float _animMotionDrag = 1f;
    float _inputHorizonal;
    float _inputVertical;
    AnimationController _ac;
    Rigidbody _rb;
    Animator _anim;
    Vector3 _nowVelocity;
    Vector2 _usegravityInputValue = Vector2.zero;
    Vector2 _inputVec;
    float _nextJumpTimer = 0f;
    public enum JumpStatePattern
    {
        Landing,
        JumpWait,
        JumpNow,
        Fall,
    }
    public enum AnimStatePattern
    {
        Idle,
        forward,
        backward,
        Jump,
        ArrowFire,
    }
    JumpStatePattern _jumpState;
    AnimStatePattern _animState;
    void Start()
    {
        _ac = GetComponent<AnimationController>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        print(InputRebinder.FindKeyName("Jump"));
    }
    Inputter _myJumpEnterInputter;
    Inputter _myJumpExitInputter;
    Inputter _myArrowFireEnterInputter;
    Inputter _myArrowFireExitInputter;

    private void OnEnable()
    {
        _myJumpEnterInputter = new(InputModeType.InGame, InputActionType.Jump, ExecuteType.Enter, JumpStart);
        _myJumpExitInputter = new(InputModeType.InGame, InputActionType.Jump, ExecuteType.Exit, JumpEnd);
        _myArrowFireEnterInputter = new(InputModeType.InGame, InputActionType.Fire1, ExecuteType.Enter, ArrowFireStart);
        _myArrowFireExitInputter = new(InputModeType.InGame, InputActionType.Fire1, ExecuteType.Exit, ArrowFireEnd);

        InputProvider.MoveCallback.AddListener(MovePlayer);
        InputProvider.Regist(_myJumpEnterInputter);
        InputProvider.Regist(_myJumpExitInputter);
        InputProvider.Regist(_myArrowFireEnterInputter);
        InputProvider.Regist(_myArrowFireExitInputter);

    }
    private void OnDisable()
    {
        InputProvider.MoveCallback.RemoveListener(MovePlayer);
        InputProvider.UnRegist(_myJumpEnterInputter);
        InputProvider.UnRegist(_myJumpExitInputter);
        InputProvider.UnRegist(_myArrowFireEnterInputter);
        InputProvider.UnRegist(_myArrowFireExitInputter);
    }
    private Vector2 UseInputGravity(Vector2 controlInputValue)
    {
        _usegravityInputValue.x = Mathf.MoveTowards(
            _usegravityInputValue.x, controlInputValue.x, Time.fixedDeltaTime * inputGravity
        );
        _usegravityInputValue.y = Mathf.MoveTowards(
            _usegravityInputValue.y, controlInputValue.y, Time.fixedDeltaTime * inputGravity
        );
        return _usegravityInputValue;
    }
    void MovePlayer(Vector2 inputVec2) => _inputVec = inputVec2;
    private void Update()
    {
        _anim.SetFloat("velocity_y", _rb.velocity.y);
        if (CameraManager._nowCameraMode == MyTPSCamera.CameraMode.FreeLookMode) //フリーカメラ時のアニメーション制御
        {
            if (_animState == AnimStatePattern.ArrowFire)
            {
                _anim.SetFloat("Speed", Mathf.Clamp(_inputVertical, -1f, 0.2f));
                _anim.SetFloat("Direction", 0f);
            }
            else
            {
                _anim.SetFloat("Speed", _inputVertical);
                _anim.SetFloat("Direction", _inputHorizonal);
            }
        }
        else //TPSカメラ時のアニメーション制御
        {
            if (_animState == AnimStatePattern.ArrowFire)
            {
                _anim.SetFloat("Speed", Mathf.Clamp(_inputVertical < 0 ? _inputVertical : _inputHorizonal + _inputVertical, -1f, 0.2f));
                _anim.SetFloat("Direction", 0f);
            }
            else
            {
                _anim.SetFloat("Speed", _inputVertical < 0 ? _inputVertical : Mathf.Abs(_inputHorizonal) + _inputVertical);
                _anim.SetFloat("Direction", _inputHorizonal);
            }
        }
        if (_jumpState == JumpStatePattern.JumpWait)
            _animMotionDrag = 0.7f;
        else if (_animState == AnimStatePattern.ArrowFire)
            _animMotionDrag = 0.5f;
        else
            _animMotionDrag = 1f;
        JumpAction();
        if(_animState == AnimStatePattern.ArrowFire)
            _ac.ArrowCharge();
    }
    void ArrowFireStart()
    {
        print("ArrowStart");
        _animState = AnimStatePattern.ArrowFire;
        _ac.ArrowChargeStart();
    }
    void ArrowFireEnd()
    {
        _ac.ArrowRelease();
        _animState = AnimStatePattern.Idle;
    }
    void JumpStart()
    {
        
        if (CameraManager._nowCameraMode != MyTPSCamera.CameraMode.FreeLookMode || _jumpState != JumpStatePattern.Landing)
        {
            return;
        }
        if (_jumpState == JumpStatePattern.Landing)
        {
            print("Jump!!!!");
            _jumpState = JumpStatePattern.JumpWait;
            _ac.JumpWait();
        }
    }
    void JumpEnd()
    {
        if ( _jumpState == JumpStatePattern.JumpWait)
        {
            _jumpState = JumpStatePattern.JumpNow;
            _rb.AddForce(Vector3.up * _jumpPower, ForceMode.VelocityChange);
            _ac.JumpUp();
        }
    }
    void JumpAction()
    {
        if (_jumpState == JumpStatePattern.JumpNow)
        {
            if (_rb.velocity.y < 0)
            {
                _jumpState = JumpStatePattern.Fall;
            }
        }
        if (_jumpState == JumpStatePattern.Fall)
        {
            if (Mathf.Abs(_rb.velocity.y) < 0.05f)
            {
                _ac.JumpEnd();
                _nextJumpTimer += Time.deltaTime;
                if (_nextJumpTimer > 0.5f)
                {
                    _nextJumpTimer = 0f;
                    _jumpState = JumpStatePattern.Landing;
                }
            }
        }

    }


    void FixedUpdate()
    {
        var moveVec2 = UseInputGravity(_inputVec);
        _inputHorizonal = moveVec2.x;
        _inputVertical = moveVec2.y;
        float moveRate = ( _inputVertical >= 0 ) ?_forwardSpeed : _backwardSpeed;
        if (CameraManager._nowCameraMode == MyTPSCamera.CameraMode.FreeLookMode)
        {
            //FreeLook時の移動制御
            //print(_inputVertical);
            var moveVelo = transform.forward * _inputVertical * moveRate * _animMotionDrag;
            _nowVelocity = new Vector3(moveVelo.x, _rb.velocity.y, moveVelo.z);
            transform.Rotate(0, _inputHorizonal * _rotateSpeed, 0);
        }
        else
        {
            //TPSCamera時の移動制御
            moveRate = (_inputVertical != 0) ? moveRate : _sidewardSpeed;
            var CForward = Camera.main.transform.forward;
            var CRight = Camera.main.transform.right;
            transform.forward = new Vector3(CForward.x, 0f, CForward.z ).normalized;
            //print(_inputVertical);
            var moveX = new Vector3(CForward.x , 0f, CForward.z).normalized * _inputVertical;
            var moveZ = new Vector3(CRight.x, 0f, CRight.z).normalized * _inputHorizonal;
            var dir = ((moveX + moveZ).magnitude < 1f ? (moveX + moveZ) : (moveX + moveZ).normalized) * moveRate * _animMotionDrag;
            //print((moveX + moveZ).magnitude);
            _nowVelocity = new Vector3(dir.x ,_rb.velocity.y , dir.z);
        }
        _rb.velocity = _nowVelocity;
    }
}
