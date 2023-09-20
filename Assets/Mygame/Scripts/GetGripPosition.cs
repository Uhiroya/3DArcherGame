using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GetGripPosition : MonoBehaviour
{
    [SerializeField] GameObject _gripPosition;
    [SerializeField] GameObject _targetImageObj;
    [SerializeField] float _targetLength = 50f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateArrowTargetPosition();
        Debug.DrawRay(transform.position, transform.forward,Color.red , _targetLength);
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _targetLength );
        if(hit.collider?.tag == "Enemy")
        {
            print("“–‚½‚Á‚½");
            _targetImageObj.transform.position = hit.point;
        }        
        else
        {
            _targetImageObj.transform.position = transform.position + transform.forward * _targetLength ;
        }
        _targetImageObj.transform.LookAt(transform.parent);
    }

    void UpdateArrowTargetPosition()
    {
        var getPos = _gripPosition.transform.position;
        transform.position = new Vector3(getPos.x, transform.position.y, getPos.z);
    }

    
}
