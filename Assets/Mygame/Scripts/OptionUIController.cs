using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour
{
    [SerializeField] TPSCameraController _TPSCamera;
    [SerializeField] TPSCameraController _FreeLookCamera;
    [SerializeField] Slider _TPS_xSensibility;
    [SerializeField] Slider _TPS_ySensibility;
    [SerializeField] Slider _FreeLook_xSensibility;
    [SerializeField] Slider _FreeLook_ySensibility;
    public void UpDateOption()
    {
        _TPSCamera._xSensibility = _TPS_xSensibility.value;
        _TPSCamera._ySensibility = _TPS_ySensibility.value;
        _FreeLookCamera._xSensibility = _FreeLook_xSensibility.value;
        _FreeLookCamera._ySensibility = _FreeLook_ySensibility.value;
    }
}
