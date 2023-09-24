﻿using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Animations;
using DG.Tweening;


// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

public class MoveController : MonoBehaviour
{
    [SerializeField] float _forwardSpeed = 7.0f;
    [SerializeField] float _backwardSpeed = 2.0f;
    [SerializeField] float _sidewardSpeed = 4.0f;
    [SerializeField] float _rotateSpeed = 2.0f;
    [SerializeField] float _jumpPower = 3.0f;

    public AnimatorStateInfo CurrentBaseState;
    public AnimatorStateInfo ArrowState;
    float _animMotionDrag = 1f;
    float _inputHorizonal;
    float _inputVertical;
    AnimationController _ac;
    Rigidbody _rb;
    Animator _anim;
    Vector3 _nowVelocity;
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
    void Update()
    {
        _inputHorizonal = Input.GetAxis("Horizontal");              
        _inputVertical = Input.GetAxis("Vertical");
        _anim.SetFloat("velocity_y", _rb.velocity.y);
        if (!CameraManager._nowTPSCameraFlag) //フリーカメラ時のアニメーション制御
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
                _anim.SetFloat("Speed", Mathf.Clamp(_inputVertical != 0 ? _inputVertical : _inputHorizonal, -1f, 0.2f));
                _anim.SetFloat("Direction", 0f);
            }
            else
            {
                _anim.SetFloat("Speed", _inputVertical != 0  ? _inputVertical : Mathf.Abs(_inputHorizonal * (_sidewardSpeed / _forwardSpeed)));
                _anim.SetFloat("Direction", _inputHorizonal);
            }
        }
        if (_jumpState == JumpStatePattern.JumpWait)
            _animMotionDrag = 0.7f;
        else if (_animState == AnimStatePattern.ArrowFire)
            _animMotionDrag = 0.5f;
        else
            _animMotionDrag = 1f;
        ArrowFireAction();
        if(!CameraManager._nowTPSCameraFlag || _jumpState != JumpStatePattern.Landing)
            JumpAction();
    }
    void ArrowFireAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _animState = AnimStatePattern.ArrowFire;
            _ac.ArrowChargeStart();
        }
        if (Input.GetMouseButton(0))
        {
            _ac.ArrowCharge();
        }
        if (Input.GetMouseButtonUp(0))
        {
            _ac.ArrowRelease();
            _animState = AnimStatePattern.Idle;
        }
    }

    void JumpAction()
    {
        if (Input.GetButtonDown("Jump") && _jumpState == JumpStatePattern.Landing)
        {
            _jumpState = JumpStatePattern.JumpWait;
            _ac.JumpWait();
        }
        if (Input.GetButtonUp("Jump") && _jumpState == JumpStatePattern.JumpWait)
        {
            _jumpState = JumpStatePattern.JumpNow;
            _rb.AddForce(Vector3.up * _jumpPower, ForceMode.VelocityChange);
            _ac.JumpUp();
        }
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
                    _jumpState = JumpStatePattern.Landing;
                    _nextJumpTimer = 0f;
                }
            }
        }
    }
    void FixedUpdate()
    {
        float moveRate = ( _inputVertical >= 0 ) ?_forwardSpeed : _backwardSpeed;
        if (!CameraManager._nowTPSCameraFlag)
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
