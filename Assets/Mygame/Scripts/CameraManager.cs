using Cinemachine;
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
    public static bool _nowTPSCameraFlag = false;
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
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OpenSetting()
    {
        _IsOption = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseSetting()
    {
        _IsOption = false;
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
            _nowTPSCameraFlag = true;
            _targetImage.SetActive(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetMouseButtonUp(1))
        {
            FreeLookCamera.SetActive(true);
            TPSCamera.SetActive(false);
            _nowTPSCameraFlag = false;
            _targetImage.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
