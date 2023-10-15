using Cinemachine;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private void Awake()
    {
        //if (instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
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
        _optionImage.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseSetting()
    {
        _IsOption = false;
        _optionImage.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            FreeLookCamera.SetActive(false);
            TPSCamera.SetActive(true);
            _targetImage.SetActive(true);
            _nowCameraMode = TPSCamera.GetComponent<MyTPSCamera>()._cameraMode;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetMouseButtonUp(1))
        {
            FreeLookCamera.SetActive(true);
            TPSCamera.SetActive(false);
            _targetImage.SetActive(false);
            _nowCameraMode = FreeLookCamera.GetComponent<MyTPSCamera>()._cameraMode;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if(Input.GetKeyDown(KeyCode.Escape))
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
}
