using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;
public class FreeLookCameraController : MonoBehaviour
{
    [SerializeField] float _offsetLimit = 3.0f;
    [SerializeField] float _offsetSpeed = 1.0f;
    CinemachineFreeLook _myCamera ;
    CinemachineCameraOffset _myCameraOffset ;
    float _defXSpeed;
    float _defYSpeed;
    float _nowCameraOffset = 0f;
    void Awake() 
    { 
        _myCamera = GetComponent<CinemachineFreeLook>();
        _myCameraOffset = GetComponent<CinemachineCameraOffset>();
        _defXSpeed = _myCamera.m_XAxis.m_MaxSpeed;
        _defYSpeed = _myCamera.m_YAxis.m_MaxSpeed;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var scroll  = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetMouseButton(1))
        {
            _myCamera.m_XAxis.m_MaxSpeed = _defXSpeed;
            _myCamera.m_YAxis.m_MaxSpeed = _defYSpeed;
        }
        else
        {
            _myCamera.m_XAxis.m_MaxSpeed = 0f;
            _myCamera.m_YAxis.m_MaxSpeed = 0f;
        }
        if (scroll != 0)
        {
            Debug.Log(scroll);
            _nowCameraOffset += Time.deltaTime * _offsetSpeed * scroll;
            _nowCameraOffset = Mathf.Clamp(_nowCameraOffset , -_offsetLimit, _offsetLimit);
            _myCameraOffset.m_Offset = new Vector3(0f, 0f, _nowCameraOffset);
        }
    }
}
