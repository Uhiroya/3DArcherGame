using UnityEngine;
using MyInput;
using System.Collections.Generic;
using System;


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
    [SerializeField] float inputGravity = 1f;
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

    InputType _inputJump = new InputType(InputMode.InGame, ActionType.Jump, UpdateMode.Update);
    InputType _inputMove = new InputType(InputMode.InGame, ActionType.Move, UpdateMode.FixedUpdate);
    InputType _inputMoveAnim = new InputType(InputMode.InGame, ActionType.Move, UpdateMode.Update);
    InputType _inputArrow = new InputType(InputMode.InGame, ActionType.Fire1, UpdateMode.Update);

    InputToken _inputArrowEnterToken;
    InputToken _inputArrowExitToken;
    InputToken _inputJumpEnterToken;
    InputToken _inputJumpExitToken;
    InputToken _inputMoveToken;
    InputToken _inputMoveAnimToken;
    void Start()
    {
        _ac = GetComponent<AnimationController>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        _inputJumpEnterToken = GA.Input?.Regist(_inputJump, ExecuteType.Enter, JumpStart);
        _inputJumpExitToken = GA.Input?.Regist(_inputJump, ExecuteType.Exit, JumpEnd);
        _inputMoveToken = GA.Input?.Regist(_inputMove, ExecuteType.Always, MovePlayer);
        _inputMoveAnimToken = GA.Input?.Regist(_inputMove, ExecuteType.Always, AnimationUpdate);
        _inputArrowEnterToken = GA.Input?.Regist(_inputArrow, ExecuteType.Enter, ArrowFireStart);
        _inputArrowExitToken = GA.Input?.Regist(_inputArrow, ExecuteType.Exit, ArrowFireEnd);
    }
    private void OnDisable()
    {
        GA.Input?.UnRegist(_inputJumpEnterToken);
        GA.Input?.UnRegist(_inputJumpExitToken);
        GA.Input?.UnRegist(_inputMoveToken);
        GA.Input?.UnRegist(_inputMoveAnimToken);
        GA.Input?.UnRegist(_inputArrowEnterToken);
        GA.Input?.UnRegist(_inputArrowExitToken);
    }
    void AnimationUpdate(Vector2 dir)
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
        if (GA.Input.GetKeyCondition( ActionType.Jump, UpdateMode.Update) == ExecuteType.Waiting)
            print($"Jumpキーは押されてないですよ-{ GA.Input.GetKeyCondition(ActionType.Jump, UpdateMode.Update)}");
        print("ArrowStart!!!");
        _animState = AnimStatePattern.ArrowFire;
        _ac.ArrowChargeStart();
    }
    void ArrowFireEnd()
    {
        print("ArrowEnd!!");
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

    private Vector2 MoveGravity(Vector2 controlInputValue)
    {
        _usegravityInputValue.x = Mathf.MoveTowards(
            _usegravityInputValue.x, controlInputValue.x, Time.fixedDeltaTime * inputGravity
        );
        _usegravityInputValue.y = Mathf.MoveTowards(
            _usegravityInputValue.y, controlInputValue.y, Time.fixedDeltaTime * inputGravity
        );
        return _usegravityInputValue;
    }
    void MovePlayer(Vector2 inputVec2) 
    {
        var moveVec2 = MoveGravity(inputVec2);
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
            var moveX = new Vector3(CForward.x , 0f, CForward.z).normalized * _inputVertical;
            var moveZ = new Vector3(CRight.x, 0f, CRight.z).normalized * _inputHorizonal;
            var dir = ((moveX + moveZ).magnitude < 1f ? (moveX + moveZ) : (moveX + moveZ).normalized) * moveRate * _animMotionDrag;
            _nowVelocity = new Vector3(dir.x ,_rb.velocity.y , dir.z);
        }
        _rb.velocity = _nowVelocity;
    }
}
