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
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            FreeLookCamera.SetActive(false);
            TPSCamera.SetActive(true);
            _nowTPSCameraFlag = true;
            _targetImage.SetActive(true);
            Cursor.visible = false;
        }
        if(Input.GetMouseButtonUp(1))
        {
            FreeLookCamera.SetActive(true);
            TPSCamera.SetActive(false);
            _nowTPSCameraFlag = false;
            _targetImage.SetActive(false);
            Cursor.visible = true;
        }
    }
}
