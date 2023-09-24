using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using TMPro;

public class TargetCallback : MonoBehaviour
{
    [SerializeField] string _detectorTag = "";
    [SerializeField] int _thisScore = 0;
    [SerializeField] UnityEvent<int> _onhit;
    [SerializeField] UnityEvent<int> _onOut;
    [SerializeField] UnityEvent<int> _onStay;
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == _detectorTag)
        {
            Destroy(other.gameObject);
            Debug.Log(_thisScore);
            transform.root.GetComponent<GeneratorController>().DestroyTarget();
            transform.parent.parent.GetComponent<TargetGenerator>().HitMyTarget(_thisScore);
            _onhit?.Invoke(_thisScore);
            
        }
    }
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == _detectorTag)
        {
            _onStay?.Invoke(_thisScore);
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == _detectorTag)
        {
            _onOut?.Invoke(_thisScore);
        }
    }
}
