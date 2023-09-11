using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairRotationChange : MonoBehaviour
{
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            transform.rotation = Quaternion.AngleAxis(5f , Vector3.up) * transform.rotation;
        }
    }
}
