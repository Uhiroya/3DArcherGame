using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TPSCameraController : MonoBehaviour
{
    [SerializeField]Transform _cameraTarget = default;
    [SerializeField]Transform _forwardTarget = default;
    [SerializeField] float _Camerarange = 5f;
    [SerializeField] float _xSensibility = 3f;
    [SerializeField] float _ySensibility = 3f;
    [SerializeField] float _followTime = 1f;
    [SerializeField] private float _minDownAngle = -30f;
    [SerializeField] private float _maxUpAngle = 40f;
    bool _inisialized = false;
    Quaternion _preRotation = Quaternion.identity;
    private float _offsetY = 0f;
    private float _offsetX = 0f;
    void OnEnable()
    {
        Camera.main.transform.position = _cameraTarget.position;
        Camera.main.transform.rotation = _cameraTarget.rotation;
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
            Camera.main.transform.position = Vector3.Lerp(_cameraTarget.position, _forwardTarget.position - (_forwardTarget.forward * _Camerarange), timer / _followTime);
            Camera.main.transform.rotation = Quaternion.Slerp(_cameraTarget.rotation, _forwardTarget.rotation, timer / _followTime);
            yield return new WaitForFixedUpdate();
        }
        callback();
    }
    void Inisialized()
    {
        _forwardTarget.rotation = _forwardTarget.parent.transform.rotation;
        _preRotation = _forwardTarget.rotation;
        _offsetX = 0;
        _offsetY = 0;
        _inisialized = true;
    }
    void Update()
    {
        if (_inisialized)
        {
            _offsetX += Input.GetAxis("Mouse X") * _xSensibility;
            _offsetY += Input.GetAxis("Mouse Y") * _ySensibility;
            _offsetY = Mathf.Clamp(_offsetY, -_maxUpAngle, -_minDownAngle);
            _forwardTarget.rotation = _preRotation * Quaternion.Euler(-_offsetY, _offsetX, 0f);
            Camera.main.transform.position = _forwardTarget.position - (_forwardTarget.forward * _Camerarange);
            Camera.main.transform.LookAt(_forwardTarget);
        }
    }
}
