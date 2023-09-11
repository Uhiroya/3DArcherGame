using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    CinemachineFreeLook _myCamera ;
    float _defXSpeed;
    float _defYSpeed;

    void Awake()
    {
        _myCamera = GetComponent<CinemachineFreeLook>();
        _defXSpeed = _myCamera.m_XAxis.m_MaxSpeed;
        _defYSpeed = _myCamera.m_YAxis.m_MaxSpeed;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Debug.Log("‰Ÿ‚³‚ê‚Ä‚é");
            _myCamera.m_XAxis.m_MaxSpeed = _defXSpeed;
            _myCamera.m_YAxis.m_MaxSpeed = _defYSpeed;
        }
        else
        {
            _myCamera.m_XAxis.m_MaxSpeed = 0f;
            _myCamera.m_YAxis.m_MaxSpeed = 0f;
        }
    }
}
