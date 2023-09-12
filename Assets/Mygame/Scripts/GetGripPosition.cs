using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGripPosition : MonoBehaviour
{
    [SerializeField] GameObject _gripPosition;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var getPos = _gripPosition.transform.position;
        transform.position = new Vector3(getPos.x , transform.position.y , getPos.z);
    }
}
