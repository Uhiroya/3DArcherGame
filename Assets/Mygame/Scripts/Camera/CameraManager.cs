using Cinemachine;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyInput;
using System;

public class CameraManager : MonoBehaviour
{
    static CameraManager instance;
    public static CameraManager Instance => instance;
    [SerializeField] GameObject FreeLookCamera;
    [SerializeField] GameObject TPSCamera;
    [SerializeField] GameObject Sholder;
    [SerializeField] GameObject _targetImage ;
    [SerializeField] GameObject _optionImage ;
    public static MyTPSCamera.CameraMode _nowCameraMode ;
    private bool _IsOption = false;
    InputType _zoomInput;
    InputType _inGameMenuInput;
    InputType _menuInput;
    InputToken _zoomEnterInputToken;
    InputToken _zoomExitInputToken;
    InputToken _inGameMenuInputToken;
    InputToken _menuInputToken;
    private void Awake()
    {
        _zoomInput = new InputType (InputMode.InGame , ActionType.Zoom ,UpdateMode.Update);
        _inGameMenuInput = new InputType (InputMode.InGame , ActionType.Cancel ,UpdateMode.Update);
        _menuInput = new InputType (InputMode.Menu , ActionType.Cancel ,UpdateMode.Update);
    }
    void OnEnable()
    {
        _zoomEnterInputToken = GA.Input?.Regist(_zoomInput , ExecuteType.Enter , ZoomStart);
        _zoomExitInputToken = GA.Input?.Regist(_zoomInput , ExecuteType.Exit , ZoomEnd);
    }
    private void OnDisable()
    {
        GA.Input.UnRegist(_zoomEnterInputToken);
        GA.Input.UnRegist(_zoomExitInputToken);

    }
    void Start()
    {
        _nowCameraMode = GetComponentInChildren<MyTPSCamera>()._cameraMode;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OpenSetting()
    {
        _IsOption = true;
        //_inGameMenuInputToken = GA.Input.Regist(menuInputCallBack);
        //GA.Input.ModeChange(InputMode.Menu);
        _optionImage.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseSetting()
    {
        _IsOption = false;
        //GA.Input.UnRegist(menuInputCallBack);
        //GA.Input.ModeChange(InputMode.InGame);
        _optionImage.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Update is called once per frame
    void ZoomStart()
    {            
        FreeLookCamera.SetActive(false);
        TPSCamera.SetActive(true);
        _targetImage.SetActive(true);
        _nowCameraMode = TPSCamera.GetComponent<MyTPSCamera>()._cameraMode;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void ZoomEnd()
    {
        FreeLookCamera.SetActive(true);
        TPSCamera.SetActive(false);
        _targetImage.SetActive(false);
        _nowCameraMode = FreeLookCamera.GetComponent<MyTPSCamera>()._cameraMode;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void GetCancel()
    {
        if (!_IsOption)
        {
            OpenSetting();
        }
        else
        {
            CloseSetting();
        }
    }
}
