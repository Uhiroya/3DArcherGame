using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyInput;

public class MyTPSCamera : MonoBehaviour
{
    /// <summary>
    /// Freelook���̓}�E�X�X�N���[���Ń^�[�Q�b�g�Ƃ̋�����ύX�o���܂��B
    /// </summary>
    [SerializeField] Transform _target = default;
    public CameraMode _cameraMode = CameraMode.TPSMode;
    [SerializeField , Header("�J�����ƃ^�[�Q�b�g�̋���")] float _cameraRange = 5f;
    [SerializeField, Header("�J�����Ǐ]�ʒu�̃I�t�Z�b�gx")] float _xOffset = 0f;
    [SerializeField, Header("�J�����Ǐ]�ʒu�̃I�t�Z�b�gy")] float _yOffset = 0f;
    [SerializeField, Header("�J�����Ǐ]�ʒu�̃I�t�Z�b�gz")] float _zOffset = 0f;
    [SerializeField, Header("�J�����ƃ^�[�Q�b�g�����̍ő�l")] float _maxScrollLimit = 15.0f;
    [SerializeField, Header("�J�����ƃ^�[�Q�b�g�����̍ŏ��l")] float _minScrollLimit = 3.0f;
    [SerializeField, Header("�X�N���[�����x")] float _offsetSpeed = 1.0f;
    [Header("X���x")]public float XSensibility = 1f;
    [Header("Y���x")] public  float YSensibility = 1f;
    [SerializeField, Header("�u�����h����")] float _followTime = 1f;
    [SerializeField, Header("YAxis����p�x")] float _minDownAngle = -30f;
    [SerializeField, Header("YAxis�����p�x")] float _maxUpAngle = 40f;
    [SerializeField, Header("�^�[�Q�b�g�ɗ^���鏉����]")] float _playerRotationY = 0f;
    Vector3 _preCameraPos;
    Vector3 _offset;
    Quaternion _preCameraRot;
    Quaternion _preRotation = Quaternion.identity;
    GameObject _offsetObj = null;
    bool _isInitialized = false;
    float _dampingRate = 0.01f;
    float _rotationY = 0f;
    float _rotationX = 0f;
    public enum CameraMode
    {
        TPSMode,
        FreeLookMode,
    }
    
    void Awake()
    {
        //target�̎q�I�u�W�F�N�g�ɃJ�����^�[�Q�b�g�𐶐�����
        _offset = new Vector3(_xOffset, _yOffset, _zOffset);
        if ( _offsetObj == null)
        {
            _offsetObj = new GameObject();
            _offsetObj.transform.SetParent(_target);
            _offsetObj.transform.localPosition = _offset;
            _offsetObj.name = "CameraTarget";
        }
    }
    InputType inputLook;
    InputType inputScroll;
    private void OnEnable()
    {
        inputLook = new InputType(InputModeType.InGame, InputActionType.Look, ExecuteType.Always, UpdateMode.FixedUpdate);
        inputScroll = new InputType(InputModeType.InGame, InputActionType.Scroll, ExecuteType.Always, UpdateMode.FixedUpdate);
        GA.Input.Regist(inputLook, GetInputMouseMove);
        GA.Input.Regist(inputScroll, CameraOffsetUpdate);

        //�A�N�e�B�u���Ƀ^�[�Q�b�g�Ώۂ��J�������ʂɌ�����
        var CForward = Camera.main.transform.forward;
        _target.transform.forward = new Vector3(CForward.x, 0f, CForward.z).normalized;
        _preCameraPos = Camera.main.transform.position;
        _preCameraRot = Camera.main.transform.rotation;
        //���̌�^�[�Q�b�g�Ώۂ���_cameraRange�����ꂽ�ʒu�ɂȂ�悤�Ƀu�����h�ړ�����B
        StartCoroutine(StartFollow(() => Initialize()));
    }
    private void OnDisable()
    {
        GA.Input.UnRegist(inputLook, GetInputMouseMove);
        GA.Input.UnRegist(inputScroll, CameraOffsetUpdate);
        _isInitialized = false;
    }
    IEnumerator StartFollow(Action callback)
    {
        float timer = 0f;
        while(timer <= _followTime)
        {
            timer += Time.fixedDeltaTime ;
            Camera.main.transform.position = Vector3.Lerp(_preCameraPos, _offsetObj.transform.position - (_target.forward * _cameraRange), timer / _followTime);
            Camera.main.transform.rotation = Quaternion.Slerp(_preCameraRot, _target.rotation, timer / _followTime);
            yield return new WaitForFixedUpdate();
        }
        callback();
    }
    void Initialize()
    {
        _preRotation = _target.rotation;
        _rotationX = 0;
        _rotationY = 0;
        _isInitialized = true;
    }
    public void GetInputMouseMove(Vector2 dir)
    {
        if (_isInitialized)
        {
            _rotationX += dir.x * XSensibility;
            _rotationY += dir.y * YSensibility;
            _rotationY = Mathf.Clamp(_rotationY, -_maxUpAngle, -_minDownAngle);
            if (_cameraMode == CameraMode.TPSMode)
            {
                //�^�[�Q�b�g�̉�]�����̃X�N���v�g���琧�䂵�Ă���
                _target.rotation = _preRotation * Quaternion.Euler(-_rotationY, _rotationX, 0f);
                var nextCameraPos = _offsetObj.transform.position - (_target.forward * _cameraRange);
                Camera.main.transform.position = nextCameraPos;
                Camera.main.transform.LookAt(_offsetObj.transform);
                //X�������̉�]�𑊎E���A�J�������S�����ɃL�����N�^�[�������悤�ɔ������B
                _target.rotation *= Quaternion.Euler(_rotationY, _playerRotationY, 0);
            }
            else if (_cameraMode == CameraMode.FreeLookMode)
            {
                //�^�[�Q�b�g�̉�]����͍s��Ȃ�
                _offsetObj.transform.rotation = _preRotation * Quaternion.Euler(-_rotationY, _rotationX, 0f);
                var nextCameraPos = _offsetObj.transform.position - (_offsetObj.transform.forward * _cameraRange);
                Camera.main.transform.position = Vector3.Slerp(Camera.main.transform.position, nextCameraPos, (Camera.main.transform.position - nextCameraPos).magnitude * _dampingRate);
                Camera.main.transform.LookAt(_offsetObj.transform);
            }
        }
    }
    void CameraOffsetUpdate(float scroll)
    {
        if (_cameraMode == CameraMode.FreeLookMode && _isInitialized)
        {
            //print(scroll);
            if (scroll != 0)
            {
                _cameraRange += _offsetSpeed * -scroll;
                _cameraRange = Mathf.Clamp(_cameraRange, _minScrollLimit, _maxScrollLimit);
            }
        }
    }
}
