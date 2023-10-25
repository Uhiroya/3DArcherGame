﻿using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Animations;
using DG.Tweening;
//using UnityEngine.InputSystem;
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
    void Start()
    {
        _ac = GetComponent<AnimationController>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }
    List<(Inputter , Action)> InGameInput = new();
    Inputter _playerInput;
    Inputter _playerAnimationInput;

    private void OnEnable()
    {
        InGameInput.Add((new Inputter(InputMode.InGame, ActionType.Jump, ExecuteType.Enter, UpdateMode.Update),JumpStart));
        InGameInput.Add((new Inputter(InputMode.InGame, ActionType.Jump, ExecuteType.Exit, UpdateMode.Update),JumpEnd));
        InGameInput.Add((new Inputter(InputMode.InGame, ActionType.Fire1, ExecuteType.Enter, UpdateMode.Update),ArrowFireStart));
        InGameInput.Add((new Inputter(InputMode.InGame, ActionType.Fire1, ExecuteType.Performed, UpdateMode.Update),() => Debug.Log("なう")));
        InGameInput.Add((new Inputter(InputMode.InGame, ActionType.Fire1, ExecuteType.Exit, UpdateMode.Update),ArrowFireEnd));
        GA.Input.Regist(InGameInput);
        _playerInput = new Inputter(InputMode.InGame, ActionType.Move, ExecuteType.Always, UpdateMode.FixedUpdate);
        _playerAnimationInput = new Inputter(InputMode.InGame, ActionType.Move, ExecuteType.Always, UpdateMode.Update);
        GA.Input.Regist(_playerInput, MovePlayer);
        GA.Input.Regist(_playerAnimationInput, AnimationUpdate);
        
        //_debugPrinter = new(InputModeType.InGame, InputActionType.Jump, ExecuteType.Enter, UpdateMode.Update, () => Debug.Log("Jumpはじめ"));
        //_debugPrinter1 = new(InputModeType.InGame, InputActionType.Jump, ExecuteType.Performed, UpdateMode.FixedUpdate, () => Debug.Log("Jump中！！！"));
        //_debugPrinter2 = new(InputModeType.InGame, InputActionType.Jump, ExecuteType.Exit, UpdateMode.Update, () => Debug.Log("Jumpおわり！！！"));
        //GA.Input.Regist(_debugPrinter);
        //GA.Input.Regist(_debugPrinter1);
        //GA.Input.Regist(_debugPrinter2);

    }
    private void OnDisable()
    {
        GA.Input.UnRegist(InGameInput);
        GA.Input.UnRegist(_playerInput, MovePlayer);
        GA.Input.UnRegist(_playerAnimationInput, AnimationUpdate);
        //GA.Input.UnRegist(_debugPrinter);
        //GA.Input.UnRegist(_debugPrinter1);
        //GA.Input.UnRegist(_debugPrinter2);
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
    void MovePlayer(Vector2 inputVec2) 
    {
        var moveVec2 = UseInputGravity(inputVec2);
        //print(moveVec2);
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
