using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TPSCameraController : MonoBehaviour
{
    [SerializeField] Transform _target = default;
    [SerializeField] bool _playerRotate = false;
    [SerializeField] float _cameraRange = 5f;
    [SerializeField] float _xOffset = 0f;
    [SerializeField] float _yOffset = 0f;
    [SerializeField] float _zOffset = 0f;
    [SerializeField] float _offsetLimit = 15.0f;
    [SerializeField] float _offsetSpeed = 1.0f;
    [SerializeField] float _xSensibility = 3f;
    [SerializeField] float _ySensibility = 3f;
    [SerializeField] float _followTime = 1f;
    [SerializeField] private float _minDownAngle = -30f;
    [SerializeField] private float _maxUpAngle = 40f;
    [SerializeField] float _playerRotationY = 0f;
    bool _inisialized = false;
    Quaternion _preRotation = Quaternion.identity;
    private float _rotationY = 0f;
    private float _rotationX = 0f;
    Vector3 _preCameraPos;
    Quaternion _preCameraRot;
    Vector3 _offset;
    GameObject _offsetObj = null;
    void OnEnable()
    {
        _offset = new Vector3(_xOffset, _yOffset, _zOffset);
        if ( _offsetObj == null)
        {
            _offsetObj = Instantiate( new GameObject(),_target.position + _offset , _target.rotation ,_target);
            _offsetObj.name = "CameraOffset";
        }
        var CForward = Camera.main.transform.forward;
        _target.transform.forward = new Vector3(CForward.x, 0f, CForward.z).normalized;
        _preCameraPos = Camera.main.transform.position;
        _preCameraRot = Camera.main.transform.rotation;
        StartCoroutine(StartFollow( () => Inisialized()));
    }
    private void OnDisable()
    {
        _inisialized = false;
    }
    IEnumerator StartFollow(Action callback)
    {
        float timer = 0f;
        while(timer <= _followTime)
        {
            timer += Time.fixedDeltaTime ;
            Camera.main.transform.position = Vector3.Lerp(_preCameraPos, _offsetObj.transform.position - (_target.forward * _cameraRange), timer / _followTime);
            //Camera.main.transform.position = Vector3.Lerp(_preCameraPos,  _target.transform.position - (_target.forward * _cameraRange), timer / _followTime);
            Camera.main.transform.rotation = Quaternion.Slerp(_preCameraRot, _target.rotation, timer / _followTime);
            yield return new WaitForFixedUpdate();
        }
        callback();
    }
    void Inisialized()
    {
        _preRotation = _target.rotation;
        _rotationX = 0;
        _rotationY = 0;
        _inisialized = true;
    }
    private void FixedUpdate()
    {
        if (_inisialized)
        {
            _rotationX += Input.GetAxis("Mouse X") * _xSensibility;
            _rotationY += Input.GetAxis("Mouse Y") * _ySensibility;
            _rotationY = Mathf.Clamp(_rotationY, -_maxUpAngle, -_minDownAngle);
            if (_playerRotate)
            {
                _target.rotation = _preRotation * Quaternion.Euler(-_rotationY, _rotationX, 0f);
                Camera.main.transform.position = _offsetObj.transform.position - (_target.forward * _cameraRange);
                Camera.main.transform.LookAt(_offsetObj.transform);
                _target.rotation *= Quaternion.Euler(_rotationY, _playerRotationY, 0);
            }
            else
            {
                _offsetObj.transform.rotation = _preRotation * Quaternion.Euler(-_rotationY, _rotationX, 0f);
                Camera.main.transform.position = _offsetObj.transform.position - (_offsetObj.transform.forward * _cameraRange);
                Camera.main.transform.LookAt(_offsetObj.transform);
            }
        }
    }
    void Update()
    {
        if (!CameraManager._nowTPSCameraFlag && !_playerRotate)
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _cameraRange +=  _offsetSpeed * -scroll;
                _cameraRange = Mathf.Clamp(_cameraRange, 3f, _offsetLimit);
            }
        }
    }
}
