using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class ColliderCallback : MonoBehaviour
{
    [SerializeField] string _detectorTag = "";
    [SerializeField] UnityEvent _onhit;
    [SerializeField] UnityEvent _onOut;
    [SerializeField] UnityEvent _onStay;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == _detectorTag)
        {
            _onhit?.Invoke();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == _detectorTag)
        {
            _onStay?.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == _detectorTag)
        {
            _onOut?.Invoke();
        }
    }
}
