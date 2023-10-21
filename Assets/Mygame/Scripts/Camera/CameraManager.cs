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
    List<(Inputter , Action)> getInputCallBack = new();
    List<(Inputter, Action)> menuInputCallBack = new();

    private void Awake()
    {
        getInputCallBack.Add((new Inputter(InputModeType.InGame, InputActionType.Zoom, ExecuteType.Enter, UpdateMode.Update), ZoomStart));
        getInputCallBack.Add((new Inputter(InputModeType.InGame, InputActionType.Zoom, ExecuteType.Exit, UpdateMode.Update), ZoomEnd));
        getInputCallBack.Add((new Inputter(InputModeType.InGame, InputActionType.Cancel, ExecuteType.Enter, UpdateMode.Update), GetCancel));
        menuInputCallBack.Add((new Inputter(InputModeType.Menu, InputActionType.Cancel, ExecuteType.Enter, UpdateMode.Update), GetCancel));
    }
    void OnEnable()
    {
        GA.Input.Regist(getInputCallBack);
    }
    private void OnDisable()
    {
        GA.Input.UnRegist(getInputCallBack);
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
        GA.Input.Regist(menuInputCallBack);
        GA.Input.ModeChange(InputModeType.Menu);
        _optionImage.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseSetting()
    {
        _IsOption = false;
        GA.Input.UnRegist(menuInputCallBack);
        GA.Input.ModeChange(InputModeType.InGame);
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
