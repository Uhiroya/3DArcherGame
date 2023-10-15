using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MyTPSCamera : MonoBehaviour
{
    /// <summary>
    /// Freelook時はマウススクロールでターゲットとの距離を変更出来ます。
    /// </summary>
    [SerializeField] Transform _target = default;
    public CameraMode _cameraMode = CameraMode.TPSMode;
    [SerializeField , Header("カメラとターゲットの距離")] float _cameraRange = 5f;
    [SerializeField, Header("カメラ追従位置のオフセットx")] float _xOffset = 0f;
    [SerializeField, Header("カメラ追従位置のオフセットy")] float _yOffset = 0f;
    [SerializeField, Header("カメラ追従位置のオフセットz")] float _zOffset = 0f;
    [SerializeField, Header("カメラとターゲット距離の最大値")] float _maxScrollLimit = 15.0f;
    [SerializeField, Header("カメラとターゲット距離の最小値")] float _minScrollLimit = 3.0f;
    [SerializeField, Header("スクロール速度")] float _offsetSpeed = 1.0f;
    [Header("X感度")]public float XSensibility = 1f;
    [Header("Y感度")] public  float YSensibility = 1f;
    [SerializeField, Header("ブレンド時間")] float _followTime = 1f;
    [SerializeField, Header("YAxis上限角度")] float _minDownAngle = -30f;
    [SerializeField, Header("YAxis下限角度")] float _maxUpAngle = 40f;
    [SerializeField, Header("ターゲットに与える初期回転")] float _playerRotationY = 0f;
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
        //targetの子オブジェクトにカメラターゲットを生成する
        _offset = new Vector3(_xOffset, _yOffset, _zOffset);
        if ( _offsetObj == null)
        {
            _offsetObj = new GameObject();
            _offsetObj.transform.SetParent(_target);
            _offsetObj.transform.localPosition = _offset;
            _offsetObj.name = "CameraTarget";
        }
    }
    private void OnEnable()
    {
        InputProvider.MouseCallback.AddListener(GetInputMouseMove);
        //アクティブ時にターゲット対象をカメラ正面に向ける
        var CForward = Camera.main.transform.forward;
        _target.transform.forward = new Vector3(CForward.x, 0f, CForward.z).normalized;
        _preCameraPos = Camera.main.transform.position;
        _preCameraRot = Camera.main.transform.rotation;
        //その後ターゲット対象から_cameraRange分離れた位置になるようにブレンド移動する。
        StartCoroutine(StartFollow(() => Initialize()));
    }
    private void OnDisable()
    {
        InputProvider.MouseCallback.RemoveListener(GetInputMouseMove);
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
            _mouseDir.x = dir.x ;
            _mouseDir.y = dir.y ;
        }
    }
    Vector2 _mouseDir = Vector2.zero;
    private void FixedUpdate()
    {
        if (_isInitialized)
        {
            _rotationX += _mouseDir.x * XSensibility;
            _rotationY += _mouseDir.y * YSensibility;
            _rotationY = Mathf.Clamp(_rotationY, -_maxUpAngle, -_minDownAngle);
            if (_cameraMode == CameraMode.TPSMode)
            {
                //ターゲットの回転をこのスクリプトから制御している
                _target.rotation = _preRotation * Quaternion.Euler(-_rotationY, _rotationX, 0f);
                var nextCameraPos = _offsetObj.transform.position - (_target.forward * _cameraRange);
                Camera.main.transform.position = nextCameraPos;
                Camera.main.transform.LookAt(_offsetObj.transform);
                //X軸方向の回転を相殺し、カメラ中心方向にキャラクターが向くように微調整。
                _target.rotation *= Quaternion.Euler(_rotationY, _playerRotationY, 0);
            }
            else if( _cameraMode == CameraMode.FreeLookMode)
            {
                //ターゲットの回転制御は行わない
                _offsetObj.transform.rotation = _preRotation * Quaternion.Euler(-_rotationY, _rotationX, 0f);
                var nextCameraPos = _offsetObj.transform.position - (_offsetObj.transform.forward * _cameraRange);
                Camera.main.transform.position = Vector3.Slerp(Camera.main.transform.position, nextCameraPos, (Camera.main.transform.position - nextCameraPos).magnitude * _dampingRate);
                Camera.main.transform.LookAt(_offsetObj.transform);
            }
        }
    }
    void Update()
    {
        if (_cameraMode == CameraMode.FreeLookMode && _isInitialized)
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _cameraRange +=  _offsetSpeed * -scroll;
                _cameraRange = Mathf.Clamp(_cameraRange, _minScrollLimit, _maxScrollLimit);
            }
        }
    }
}
