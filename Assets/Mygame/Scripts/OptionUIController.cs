using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour
{
    [SerializeField] MyTPSCamera _TPSCamera;
    [SerializeField] MyTPSCamera _FreeLookCamera;
    [SerializeField] Slider _TPS_xSensibility;
    [SerializeField] Slider _TPS_ySensibility;
    [SerializeField] Slider _FreeLook_xSensibility;
    [SerializeField] Slider _FreeLook_ySensibility;
    public void UpDateOption()
    {
        _TPSCamera.XSensibility = _TPS_xSensibility.value;
        _TPSCamera.YSensibility = _TPS_ySensibility.value;
        _FreeLookCamera.XSensibility = _FreeLook_xSensibility.value;
        _FreeLookCamera.YSensibility = _FreeLook_ySensibility.value;
    }
}
