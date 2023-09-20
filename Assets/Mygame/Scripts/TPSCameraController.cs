using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TPSCameraController : MonoBehaviour
{
    [SerializeField]Transform _forwardTarget = default;
    [SerializeField] float _Camerarange = 5f;
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
    void OnEnable()
    {
        var CForward = Camera.main.transform.forward;
        _forwardTarget.parent.transform.forward = new Vector3(CForward.x, 0f, CForward.z).normalized;
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
            Camera.main.transform.position = Vector3.Lerp(_preCameraPos, _forwardTarget.position - (_forwardTarget.forward * _Camerarange), timer / _followTime);
            Camera.main.transform.rotation = Quaternion.Slerp(_preCameraRot, _forwardTarget.rotation * Quaternion.Euler(0, _playerRotationY, 0), timer / _followTime);
            yield return new WaitForFixedUpdate();
        }
        callback();
    }
    void Inisialized()
    {
        _forwardTarget.rotation = _forwardTarget.parent.transform.rotation;
        _preRotation = _forwardTarget.rotation;
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
            _forwardTarget.rotation = _preRotation * Quaternion.Euler(-_rotationY, _rotationX, 0f);
            _forwardTarget.parent.transform.rotation *= Quaternion.Euler(0, _playerRotationY, 0);
            Camera.main.transform.position = _forwardTarget.position + - (_forwardTarget.forward * _Camerarange);
            Camera.main.transform.LookAt(_forwardTarget);
        }
    }
    void Update()
    {

    }
}
